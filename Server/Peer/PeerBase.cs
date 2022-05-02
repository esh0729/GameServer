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
		private string m_sIPAddress = null;
		private int m_nPort = 0;

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
			m_sIPAddress = ((IPEndPoint)m_socket.RemoteEndPoint).Address.ToString();
			m_nPort = ((IPEndPoint)m_socket.RemoteEndPoint).Port;

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
			get { return m_sIPAddress; }
		}

		public int port
		{
			get { return m_nPort; }
		}

		public bool disposed
		{
			get { return m_bDisposed; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Service()
		{
			if (m_bDisposed)
				return;
				
			Receive();

			if (m_applicationBase.connectionTimeoutInterval != 0 && (DateTime.Now - m_lastPingCheckTime).TotalMilliseconds > m_applicationBase.connectionTimeoutInterval)
				Disconnect("Timeout.");
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

				//
				// 서버 이벤트 직렬화
				//

				FullPacket fullPacket = new FullPacket(PacketType.EventData, EventData.ToBytes(eventData));
				byte[] buffer = FullPacket.ToBytes(fullPacket);

				//
				// Packet의 바이트수 + Packet 클라이언트에 전달
				//

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

				//
				// 클라이언트 응답 직렬화
				//

				FullPacket fullPacket = new FullPacket(PacketType.OperationResponse, OperationResponse.ToBytes(operationResponse));
				byte[] buffer = FullPacket.ToBytes(fullPacket);

				//
				// Packet의 바이트수 + Packet 클라이언트에 전달
				//

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
				//
				// 비동기 대기는 1번만 처리
				//

				if (m_bAwaiting)
					return;

				m_bAwaiting = true;

				//
				// Packet의 바이트수를 먼저 Receive하고 해당 바이트 수많큼 buffer 크기 할당
				//

				byte[] buffer = new byte[sizeof(int)];
				
				//
				// 클라이언트로부터 Packet이 올때까지 비동기 대기
				//

				int nReceiveCount = await Task.Factory.FromAsync<int>(m_socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, null), m_socket.EndReceive);
				if (nReceiveCount > 0)
				{
					int nBufferLength = BitConverter.ToInt32(buffer, 0);

					buffer = new byte[nBufferLength];
					m_socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

					//
					// 역직렬화
					//

					FullPacket fullPacket = FullPacket.ToFullPacket(buffer);

					switch (fullPacket.type)
					{
						// Timeout 갱신 처리
						case PacketType.PingCheck: OnPingCheck(); break;

						// 커맨드 처리
						case PacketType.OperationRequest: OnOperationRequest(OperationRequest.ToOperationRequest(fullPacket.packet)); break;

						default:
							throw new Exception("Not Valied PacketType");
					}
				}
				else
				{
					//
					// 받은 바이트수가 0일 경우 Clinet에서 Socket Close 했을 경우 올수 있으므로 Disconnect 호출
					//

					Disconnect("Client Socket Close.");
				}
			}
			catch (Exception ex)
			{
				//
				// 소켓 Reiceve중 에러 처리 이후 접속이 끊어졌을 경우 Disconnect처리
				//

				if (!m_socket.Connected)
					Disconnect("Client Receive Error.");

				OnReceiveError(ex);
			}
			finally
			{
				m_bAwaiting = false;
			}
		}

		//
		// Timeout 갱신 함수
		//

		private void OnPingCheck()
		{
			m_lastPingCheckTime = DateTime.Now;

			ResponsePingCheck();
		}

		//
		// Timeout갱신 Reuqest 받는 즉시 Response 전송
		//

		private void ResponsePingCheck()
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
		// 서버 종료시 클라이언트에 1바이트 데이터 전송
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

		public void Disconnect(string sDisconnectType)
		{
			if (m_bDisposed)
				return;

			m_bDisposed = true;

			try
			{
				SendDisconnectResponse();

				m_socket.Disconnect(true);
			}
			catch
			{

			}
			finally
			{
				m_socket.Close();

				m_applicationBase.RemovePeer(this);

				OnDisconnect(sDisconnectType);
			}
		}

		protected abstract void OnDisconnect(string sDisconnectType);
	}
}
