using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace Server
{
	//=====================================================================================================================
	// 클라이언트와의 연결과 데이터의 송수신을 담당하는 추상 클래스
	//=====================================================================================================================
	public abstract class PeerBase
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private ApplicationBase m_applicationBase = null;
		private Socket m_socket = null;

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
			get { return ((IPEndPoint)m_socket.RemoteEndPoint).Address.ToString(); }
		}

		public int port
		{
			get { return ((IPEndPoint)m_socket.RemoteEndPoint).Port; }
		}

		public bool disposed
		{
			get { return m_bDisposed; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Start()
		{
			// 수신 버퍼 생성(수신처리는 1번에 1건만 처리되기 때문에 BeginReceive()의 state 변수로 전달하며 재활용)
			DataBuffer buffer = new DataBuffer();

			// 수신 비동기 대기
			m_socket.BeginReceive(buffer.buffer, 0, DataBuffer.kBufferLength, 0, new AsyncCallback(ReceiveCallback), buffer);
		}

		public void Service()
		{
			if (m_bDisposed)
				return;

			// Timeout시간 경과시 클라이언트 Disconnect;
			if (m_applicationBase.connectionTimeoutInterval != 0 && (DateTime.Now - m_lastPingCheckTime).TotalMilliseconds > m_applicationBase.connectionTimeoutInterval)
				Disconnect();
		}

		//
		// Receive
		//

		protected virtual void OnReceiveError(Exception ex)
		{
		}

		// 수신 콜백 함수
		private void ReceiveCallback(IAsyncResult result)
		{
			if (m_bDisposed)
				return;

			// 수신 버퍼
			DataBuffer dataBuffer = (DataBuffer)result.AsyncState;

			Data data = null;

			try
			{
				// 데이터 수신 길이
				dataBuffer.receiveLength = m_socket.EndReceive(result);
				if (dataBuffer.receiveLength > 0)
				{
					// TCP의 경우 데이터의 경계가 없어 2번의 Send를 보내더라도 한번의 Receive로 모든 데이터를받을수 있기 때문에
					// 1번째 Send의 길이만큼 데이터 처리 이후 처리된 데이터는 삭제 2번째 데이터의 길이를 dataBuffer.receivedLength로 호출
					// 첫 루프 dataBuffer.receivedLength 0보다 클 경우 여러 Send를 한번의 Receive로 처리 했다고 판단
					while (dataBuffer.totalReceiveLength > 0)
					{
						int nTotalReceivedLength = dataBuffer.totalReceiveLength;

						// 패킷의 헤더에 데이터 길이 삽입
						// 패킷의 길이 체크(패킷의 길이 사이즈 만큼 데이터를 수신하지 않았을 경우 받은 위치부터 다시 비동기 수신 시도)
						if (nTotalReceivedLength < Data.kLengthSize)
						{
							dataBuffer.currentIndex = nTotalReceivedLength;
							break;
						}
						else
						{
							// 패킷 체크(총 패킷의 길이만큼 데이터를 수신하지 않았을 경우 받은 위치부터 다시 비동기 수신 시도)
							if (nTotalReceivedLength < dataBuffer.dataLength)
							{
								dataBuffer.currentIndex = nTotalReceivedLength;
								break;
							}
							else
							{
								// 역직렬화
								data = m_dataQueue.GetData();
								// 유효하지 않은 데이터로 처리 불가능 할 경우
								if (!data.SetData(dataBuffer.buffer, dataBuffer.dataLength))
								{
									// 처리중인 데이터 정리
									dataBuffer.ClearUsedReceiveData();
									throw new Exception("Invalid Data");
								}

								// 사용한 데이터 정리
								dataBuffer.ClearUsedReceiveData();

								// 타입에 따른 처리
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
						}
					}
				}
				else
				{
					// 받은 바이트수가 0일 경우 Clinet에서 Socket Close 했을 경우 올수 있으므로 Disconnect 호출
					Disconnect();
				}
			}
			catch (Exception ex)
			{
				// 소켓 Reiceve중 에러 처리 이후 접속이 끊어졌을 경우 Disconnect처리
				if (!m_socket.Connected)
					Disconnect();

				OnReceiveError(ex);
			}
			finally
			{
				// 전달 데이터를 담고 있는 버퍼를 삭제 하지 않고
				// 데이터 객체 큐에 반납
				if (data != null)
					m_dataQueue.ReturnData(data);

				// 데이터 수신 처리가 모두 끝났으면 다시 비동기 대기 시작
				if (m_socket.Connected)
					m_socket.BeginReceive(dataBuffer.buffer, dataBuffer.currentIndex, DataBuffer.kBufferLength - dataBuffer.currentIndex, SocketFlags.None, ReceiveCallback, dataBuffer);
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
				// 전송 메시지 큐에 메시지 큐잉
				m_sendMessage.Enqueue(message);

				// 전송 메시지 큐에 이미 데이터가 있었을 경우에는 전송중이기 때문에 StartSend() 호출하지 않음
				if (m_sendMessage.Count > 1)
					return;
			}

			// 전송 시작
			StartSend();
		}

		private void StartSend()
		{
			if (m_bDisposed)
				return;

			Data data = null;

			try
			{
				// 전송 버퍼가 없을 경우 생성(전송은 1번에 1건만 처리 되기때문에 전송 버퍼는 1개만 필요)
				if (m_sendBuffer == null)
					m_sendBuffer = new DataBuffer();

				IMessage message = null;

				// 첫번째 메세지 출력
				lock (m_sendLockObject)
				{
					message = m_sendMessage.Peek();
				}

				// Data 인스턴스 호출
				data = m_dataQueue.GetData();

				// 내부 데이터 직렬화

				long lnLength;
				data.type = message.type;
				message.GetBytes(data.packet, out lnLength);
				data.packetLength = Convert.ToInt32(lnLength);

				// 내부 데이터의 길이 + 내부 데이터 직렬화 + 체크섬 처리
				int nBufferLength;
				data.GetBytes(m_sendBuffer.buffer, out nBufferLength);

				// 클라이언트에 비동기 전송
				m_socket.BeginSend(m_sendBuffer.buffer, 0, nBufferLength, SocketFlags.None, new AsyncCallback(SendCallback), null);
			}
			catch
			{
				// 소켓 Send중 에러 처리 이후 접속이 끊어졌을 경우 Disconnect처리
				if (!m_socket.Connected)
					Disconnect();
			}
			finally
			{
				// 데이터 큐 반납
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
				int nSendCount = m_socket.EndSend(result);

				lock (m_sendLockObject)
				{
					// Send완료후 전달메세지큐에서 해당 데이터 삭제
					m_sendMessage.Dequeue();

					if (m_sendMessage.Count == 0)
						return;
				}

				// 이후 전달메세지큐에 아직 전달할 데이터가 있을경우 다시 전달 시작
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
				// 데이터 큐 호출 및 설정
				data = m_dataQueue.GetData();
				data.type = PacketType.PingCheck;
				data.packetLength = 0;

				// 타임아웃갱신용 메시지는 내부 데이터가 없으므로 고정된 길이의 버퍼 사용
				int nLength;
				data.GetBytes(pingCheckBuffer, out nLength);

				// 비동기 송신
				m_socket.BeginSend(pingCheckBuffer, 0, nLength, SocketFlags.None, SendPingCheckCallback, null);
			}
			finally
			{
				// 데이터 큐 반납
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
