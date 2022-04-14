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

		private bool m_bAwaiting = false;

		//
		//
		//

		protected Guid m_id = Guid.Empty;

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

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Service()
		{
			if (!m_bAwaiting)
				Receive();
		}

		//
		// Send
		//

		public void SendEvent(EventData eventData)
		{
			FullPacket fullPacket = new FullPacket(PacketType.EventData, EventData.ToBytes(eventData));
			byte[] buffer = FullPacket.ToBytes(fullPacket);

			List<byte> fullBuffer = new List<byte>();
			fullBuffer.AddRange(BitConverter.GetBytes(buffer.Length));
			fullBuffer.AddRange(buffer);

			m_socket.Send(fullBuffer.ToArray());
		}

		public void SendResponse(OperationResponse operationResponse)
		{
			FullPacket fullPacket = new FullPacket(PacketType.OperationResponse, OperationResponse.ToBytes(operationResponse));
			byte[] buffer = FullPacket.ToBytes(fullPacket);

			List<byte> fullBuffer = new List<byte>();
			fullBuffer.AddRange(BitConverter.GetBytes(buffer.Length));
			fullBuffer.AddRange(buffer);

			m_socket.Send(fullBuffer.ToArray());
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
						case PacketType.OperationRequest:
							{
								OnOperationRequest(OperationRequest.ToOperationRequest(fullPacket.packet));
							}
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

		protected abstract void OnOperationRequest(OperationRequest request);

		//
		//
		//

		public void Disconnect()
		{
			if (m_bDisposed)
				return;

			m_bDisposed = true;

			m_socket.Disconnect(true);
			m_socket.Close();

			m_applicationBase.RemovePeer(this);

			OnDisconnect();
		}

		protected abstract void OnDisconnect();
	}
}
