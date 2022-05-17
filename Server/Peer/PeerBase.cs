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

		private DataQueue m_dataQueue = null;

		private object m_sendLockObject = new object();
		private Queue<IMessage> m_sendMessage = new Queue<IMessage>();
		private DataBuffer m_sendBuffer = null;

		private DateTime m_lastPingCheckTime = DateTime.MinValue;

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

			m_dataQueue = new DataQueue();

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

		public void Start()
		{
			//
			// 수신 버퍼 생성
			//

			DataBuffer buffer = new DataBuffer();

			//
			// 수신 비동기 대기
			//

			m_socket.BeginReceive(buffer.buffer, 0, DataBuffer.kBufferLength, 0, new AsyncCallback(ReceiveCallback), buffer);
		}

		public void Service()
		{
			if (m_bDisposed)
				return;

			//
			// Timeout시간 경과시 클라이언트 Disconnect;
			//

			if (m_applicationBase.connectionTimeoutInterval != 0 && (DateTime.Now - m_lastPingCheckTime).TotalMilliseconds > m_applicationBase.connectionTimeoutInterval)
				Disconnect();
		}

		//
		// Receive
		//

		protected virtual void OnReceiveError(Exception ex)
		{
		}

		private void ReceiveCallback(IAsyncResult result)
		{
			if (m_bDisposed)
				return;

			DataBuffer dataBuffer = (DataBuffer)result.AsyncState;

			Data data = null;

			try
			{
				dataBuffer.useLength = m_socket.EndReceive(result);
				if (dataBuffer.useLength > 0)
				{
					// 역직렬화
					data = m_dataQueue.GetData();
					if (!data.SetData(dataBuffer.buffer, dataBuffer.useLength))
						throw new Exception("Invalid Data");

					switch (data.type)
					{
						// Timeout 갱신 처리
						case PacketType.PingCheck: OnPingCheck(); break;

						// 커맨드 처리
						case PacketType.OperationRequest: OnOperationRequest(OperationRequest.ToOperationRequest(data.packet)); break;

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
				//
				// 데이터 객체 큐에 반납
				//

				if (data != null)
					m_dataQueue.ReturnData(data);

				//
				// 수신 비동기 대기
				//

				if (m_socket.Connected)
					m_socket.BeginReceive(dataBuffer.buffer, 0, DataBuffer.kBufferLength, SocketFlags.None, ReceiveCallback, dataBuffer);
			}
		}

		//
		// Send
		//

		public bool SendEvent(EventData eventData)
		{
			if (m_bDisposed)
				return false;

			try
			{
				Send(eventData);
			}
			catch
			{
				return false;
			}

			return true;
		}

		public bool SendResponse(OperationResponse operationResponse)
		{
			if (m_bDisposed)
				return false;

			try
			{
				Send(operationResponse);
			}
			catch
			{
				return false;
			}

			return true;
		}

		private void Send(IMessage message)
		{
			lock (m_sendLockObject)
			{
				m_sendMessage.Enqueue(message);

				if (m_sendMessage.Count > 1)
					return;
			}

			StartSend();
		}

		private void StartSend()
		{
			if (m_bDisposed)
				return;

			Data data = null;

			try
			{
				if (m_sendBuffer == null)
					m_sendBuffer = new DataBuffer();

				IMessage message = null;

				//
				// 첫번째 메세지 출력
				//

				lock (m_sendLockObject)
				{
					message = m_sendMessage.Peek();
				}

				//
				// Data 인스턴스 설정
				//

				data = m_dataQueue.GetData();

				long lnLength;
				data.type = message.type;
				message.GetBytes(data.packet, out lnLength);
				data.packetLength = Convert.ToInt32(lnLength);

				//
				// 직렬화
				//

				int nBufferLength;
				data.GetBytes(m_sendBuffer.buffer, out nBufferLength);
				m_sendBuffer.useLength = nBufferLength;

				//
				// 클라이언트에 비동기 전송
				//

				m_socket.BeginSend(m_sendBuffer.buffer, 0, m_sendBuffer.useLength, SocketFlags.None, new AsyncCallback(SendCallback), null);
			}
			catch
			{
				//
				// 소켓 Send중 에러 처리 이후 접속이 끊어졌을 경우 Disconnect처리
				//

				if (!m_socket.Connected)
					Disconnect();
			}
			finally
			{
				//
				// 데이터 큐 반납
				//

				if (data != null)
					m_dataQueue.ReturnData(data);
			}
		}

		private void SendCallback(IAsyncResult result)
		{
			if (m_bDisposed)
				return;

			try
			{
				m_socket.EndSend(result);

				lock (m_sendLockObject)
				{
					//
					// Send완료후 전달메세지큐에서 해당 데이터 삭제
					//

					m_sendMessage.Dequeue();

					if (m_sendMessage.Count == 0)
						return;
				}

				//
				// 이후 전달메세지큐에 아직 전달할 데이터가 있을경우 다시 전달 시작
				//

				StartSend();
			}
			catch
			{
				if (!m_socket.Connected)
					Disconnect();
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

		private byte[] pingCheckBuffer = new byte[Data.kNonPacketDataSize];
		private void ResponsePingCheck()
		{
			if (m_bDisposed)
				return;

			Data data = null;

			try
			{
				data = m_dataQueue.GetData();
				data.type = PacketType.PingCheck;
				data.packetLength = 0;

				int nLength;
				data.GetBytes(pingCheckBuffer, out nLength);

				m_socket.BeginSend(pingCheckBuffer, 0, nLength, SocketFlags.None, SendPingCheckCallback, null);
			}
			finally
			{
				if (data != null)
					m_dataQueue.ReturnData(data);
			}
		}

		private void SendPingCheckCallback(IAsyncResult result)
		{
			if (m_bDisposed)
				return;

			try
			{
				m_socket.EndSend(result);
			}
			catch
			{
				if (!m_socket.Connected)
					Disconnect();
			}
		}

		protected abstract void OnOperationRequest(OperationRequest request);

		//
		// 서버 종료시 클라이언트에 1바이트 데이터 전송
		//

		private void SendDisconnectResponse()
		{
			try
			{
				m_socket.Send(new byte[] { 0 });
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
					m_sendMessage.Clear();
				}

				m_dataQueue.Clear();

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
