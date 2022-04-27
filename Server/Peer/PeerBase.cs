using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
	public abstract class PeerBase
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private ApplicationBase m_applicationBase = null;
		private Socket m_socket = null;

		private DateTime m_lastPingCheckTime = DateTime.MinValue;

		private bool m_bAwaiting = false;

		//
		//
		//

		private Guid m_id = Guid.Empty;

		private bool m_bDisposed = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Construcotrs

		public PeerBase(PeerInit peerInit)
		{
			if (peerInit == null)
				throw new ArgumentNullException("peerInit");

			m_applicationBase = peerInit.applicationBase;
			m_socket = peerInit.socket;

			m_id = Guid.NewGuid();

			m_lastPingCheckTime = DateTime.Now;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public Guid id
		{
			get { return m_id; }
		}

		public string ipAddress
		{
			get { return ((IPEndPoint)m_socket.RemoteEndPoint).Address.ToString(); }
		}

		public string port
		{
			get { return ((IPEndPoint)m_socket.RemoteEndPoint).Port.ToString(); }
		}

		public bool disposed
		{
			get { return m_bDisposed; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Service()
		{
			if (!m_bAwaiting)
				Receive();

			if (m_applicationBase.connectionTimeoutInterval != 0 && (DateTime.Now - m_lastPingCheckTime).TotalMilliseconds > m_applicationBase.connectionTimeoutInterval)
			{
				Console.WriteLine(port + ", TimeOut");
				Disconnect();
			}
		}

		//
		// Send
		//

		public bool SendEvent(EventData eventData)
		{
			try
			{
				if (m_bDisposed)
					return false;

				FullPacket fullPacket = new FullPacket(PacketType.EventData, EventData.ToBytes(eventData));
				byte[] buffer = FullPacket.ToBytes(fullPacket);

				List<byte> fullBuffer = new List<byte>();
				fullBuffer.AddRange(BitConverter.GetBytes(buffer.Length));
				fullBuffer.AddRange(buffer);

				m_socket.Send(fullBuffer.ToArray());
			}
			catch
			{
				return false;
			}

			return true;
		}

		public bool SendResponse(OperationResponse operationResponse)
		{
			try
			{
				if (m_bDisposed)
					return false;

				FullPacket fullPacket = new FullPacket(PacketType.OperationResponse, OperationResponse.ToBytes(operationResponse));
				byte[] buffer = FullPacket.ToBytes(fullPacket);

				List<byte> fullBuffer = new List<byte>();
				fullBuffer.AddRange(BitConverter.GetBytes(buffer.Length));
				fullBuffer.AddRange(buffer);

				m_socket.Send(fullBuffer.ToArray());
			}
			catch
			{
				return false;
			}

			return true;
		}

		//
		// Receive
		//

		protected virtual void OnReceiveError(Exception ex)
		{
		}

		private async void Receive()
		{
			try
			{
				m_bAwaiting = true;

				byte[] buffer = new byte[sizeof(int)];
				int nReceiveCount = await Task.Factory.FromAsync<int>(m_socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, null), m_socket.EndReceive);

				if (nReceiveCount > 0)
				{
					int nBufferLength = BitConverter.ToInt32(buffer, 0);

					buffer = new byte[nBufferLength];

					m_socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

					FullPacket fullPacket = FullPacket.ToFullPacket(buffer);

					switch (fullPacket.type)
					{
						case PacketType.PingCheck: OnPingCheck(); break;
						case PacketType.OperationRequest: OnOperationRequest(OperationRequest.ToOperationRequest(fullPacket.packet));
							break;

						default:
							throw new Exception("Not Valied PacketType");
					}
				}
				else
					Disconnect();
			}
			catch (Exception ex)
			{
				if (!m_socket.Connected)
					Disconnect();

				OnReceiveError(ex);
			}
			finally
			{
				m_bAwaiting = false;
			}
		}

		private void OnPingCheck()
		{
			m_lastPingCheckTime = DateTime.Now;

			ResponsePicgCheck();
		}

		private void ResponsePicgCheck()
		{
			if (m_bDisposed)
				return;

			List<byte> fullBuffer = new List<byte>();

			FullPacket fullPacket = new FullPacket(PacketType.PingCheck, new byte[] { });
			byte[] buffer = FullPacket.ToBytes(fullPacket);

			fullBuffer.AddRange(BitConverter.GetBytes(buffer.Length));
			fullBuffer.AddRange(buffer);

			m_socket.Send(fullBuffer.ToArray());
		}

		protected abstract void OnOperationRequest(OperationRequest request);

		//
		//
		//

		private void SendDisconnectResponse()
		{
			try
			{
				List<byte> fullBuffer = new List<byte>();

				fullBuffer.Add(0);

				m_socket.Send(fullBuffer.ToArray());
			}
			catch
			{

			}
		}

		//
		//
		//

		public void Disconnect()
		{
			if (m_bDisposed)
				return;

			m_bDisposed = true;

			SendDisconnectResponse();

			m_socket.Disconnect(true);
			m_socket.Close();

			m_applicationBase.RemovePeer(this);

			OnDisconnect();
		}

		protected abstract void OnDisconnect();
	}
}
