using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

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

		private PacketQueue m_packetQueue = null;

		private object m_sendLockObject = new object();
		private Queue<FullPacket> m_sendPackets = new Queue<FullPacket>();

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

			m_packetQueue = new PacketQueue();

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

			//
			// 이미 대기중일 경우 Receive함수 호출X
			//

			if (!m_bAwaiting)
				Receive();

			//
			// Timeout시간 경과시 클라이언트 Disconnect;
			//

			if (m_applicationBase.connectionTimeoutInterval != 0 && (DateTime.Now - m_lastPingCheckTime).TotalMilliseconds > m_applicationBase.connectionTimeoutInterval)
				Disconnect();
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
				// 패킷큐에서 인스턴스 호출
				//

				FullPacket fullPacket = m_packetQueue.GetPacket();
				fullPacket.Set(PacketType.EventData, EventData.ToBytes(eventData));

				Send(fullPacket);
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
				// 패킷큐에서 인스턴스 호출
				//

				FullPacket fullPacket = m_packetQueue.GetPacket();
				fullPacket.Set(PacketType.OperationResponse, OperationResponse.ToBytes(operationResponse));

				Send(fullPacket);
			}
			catch
			{
				return false;
			}

			return true;
		}

		private void Send(FullPacket packet)
		{
			lock (m_sendLockObject)
			{
				m_sendPackets.Enqueue(packet);

				if (m_sendPackets.Count > 1)
					return;
			}

			StartSend();
		}

		private void StartSend()
		{
			try
			{
				if (m_bDisposed)
					return;

				FullPacket fullPacket = null;

				lock (m_sendLockObject)
				{
					fullPacket = m_sendPackets.Peek();
				}

				//
				// 직렬화
				//

				byte[] buffer = FullPacket.ToBytes(fullPacket);

				//
				// Packet의 바이트수 + Packet 클라이언트에 전달
				//

				List<byte> fullBuffer = new List<byte>();
				fullBuffer.AddRange(BitConverter.GetBytes(buffer.Length));
				fullBuffer.AddRange(buffer);

				Task.Factory.FromAsync(m_socket.BeginSend(fullBuffer.ToArray(), 0, fullBuffer.Count, SocketFlags.None, CompleteSend, null), m_socket.EndSend);
			}
			catch
			{
				//
				// 소켓 Send중 에러 처리 이후 접속이 끊어졌을 경우 Disconnect처리
				//

				if (!m_socket.Connected)
					Disconnect();
			}
		}

		private void CompleteSend(object state)
		{
			lock (m_sendLockObject)
			{
				if (m_bDisposed)
					return;

				//
				// Send완료후 전달패킷큐에서 해당 데이터 삭제
				//

				FullPacket packet = m_sendPackets.Dequeue();
				m_packetQueue.ReturnPacket(packet);

				if (m_sendPackets.Count == 0)
					return;
			}

			//
			// 이후 전달패킷큐에 아직 전달할 데이터가 있을경우 다시 전달 시작
			//

			StartSend();
		}

		//
		// Receive
		//

		protected virtual void OnReceiveError(Exception ex)
		{
		}

		private async void Receive()
		{
			FullPacket fullPacket = null;

			try
			{
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
					fullPacket = m_packetQueue.GetPacket();
					FullPacket.ToFullPacket(buffer, ref fullPacket);

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

					Disconnect();
				}
			}
			catch (Exception ex)
			{
				//
				// 소켓 Reiceve중 에러 처리 이후 접속이 끊어졌을 경우 Disconnect처리
				//

				if (!m_socket.Connected)
					Disconnect();

				OnReceiveError(ex);
			}
			finally
			{
				m_bAwaiting = false;

				m_packetQueue.ReturnPacket(fullPacket);
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

			FullPacket fullPacket = m_packetQueue.GetPacket();
			fullPacket.Set(PacketType.PingCheck, new byte[] { });

			Send(fullPacket);
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

		public void Disconnect()
		{
			if (m_bDisposed)
				return;

			m_bDisposed = true;

			try
			{
				lock (m_sendLockObject)
				{
					m_sendPackets.Clear();
					m_packetQueue.Clear();
				}

				SendDisconnectResponse();

				m_socket.Shutdown(SocketShutdown.Both);
			}
			catch
			{

			}
			finally
			{
				m_socket.Close();

				m_applicationBase.RemovePeer(this);

				OnDisconnect();
			}
		}

		protected abstract void OnDisconnect();
	}
}
