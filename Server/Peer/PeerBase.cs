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

		// 서버의 메인 클래스 객체
		private ApplicationBase m_applicationBase = null;
		// 클라이언트 소켓
		private Socket m_socket = null;

		// 송수신할 데이터를 저장하는 Data객체를 관리하는 변수
		private DataQueue m_dataQueue = null;

		// m_sendMessageQueue의 접근을 한번에 1건 처리하기 위해 필요한 lock에 필요한 객체
		private object m_sendLockObject = new object();
		// 송신 데이터를 저장하고 있는 큐
		private Queue<IMessage> m_sendMessageQueue = new Queue<IMessage>();
		// 송신 버퍼(송신은 한번에 1건만 처리 되기 때문에 멤버변수로 가지고 있음)
		private DataBuffer m_sendBuffer = null;

		// 최근 핑체크 시각
		private DateTime m_lastPingCheckTime = DateTime.MinValue;

		//
		//
		//

		// 피어의 고유ID
		private Guid m_id = Guid.Empty;

		// 리소스 해제 여부
		private bool m_bDisposed = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Construcotrs

		//=====================================================================================================================
		// 생성자
		//
		// peerInit : 서버의 메인 클래스 객체와 소켓 정보를 담고 있는 객체
		//=====================================================================================================================
		public PeerBase(PeerInit peerInit)
		{
			if (peerInit == null)
				throw new ArgumentNullException("peerInit");

			m_applicationBase = peerInit.applicationBase;
			m_socket = peerInit.socket;

			// 데이터큐는 피어별로 관리
			m_dataQueue = new DataQueue();

			// 피어의 고유ID 생성
			m_id = Guid.NewGuid();

			// 핑체크 시각 해당 객체 생성 기준으로 갱신
			m_lastPingCheckTime = DateTime.Now;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 피어의 고유ID
		public Guid id
		{
			get { return m_id; }
		}

		// 소켓에 연결된 클라이언트의 주소
		public string ipAddress
		{
			get { return ((IPEndPoint)m_socket.RemoteEndPoint).Address.ToString(); }
		}

		// 소켓에 연결된 클라이언트의 포트번호
		public int port
		{
			get { return ((IPEndPoint)m_socket.RemoteEndPoint).Port; }
		}

		// 리소스 해제 여부
		public bool disposed
		{
			get { return m_bDisposed; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 피어의 시작 함수
		//=====================================================================================================================
		public void Start()
		{
			// 수신 버퍼 생성(수신처리는 1번에 1건만 처리되기 때문에 BeginReceive()의 state 변수로 전달하며 재활용)
			DataBuffer buffer = new DataBuffer();

			// 비동기 수신 대기
			m_socket.BeginReceive(buffer.buffer, 0, DataBuffer.kBufferLength, 0, new AsyncCallback(ReceiveCallback), buffer);
		}

		//=====================================================================================================================
		// 갱신 함수(타임아웃 체크용)
		//=====================================================================================================================
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

		//=====================================================================================================================
		// 클라이언트 데이터 수신중 에러 발생시 호출되는 함수
		//
		// ex : 발생 오류
		//=====================================================================================================================
		protected virtual void OnReceiveError(Exception ex)
		{
		}

		//=====================================================================================================================
		// 비동기 수신 콜백 함수
		//
		// result : 비동기 수신의 결과
		//=====================================================================================================================
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

		//=====================================================================================================================
		// 클라이언트의 요청 없이 서버에서 클라이언트로 데이터 송신시 호출하는 함수
		//
		// eventData : 송신할 이벤트 데이터
		//=====================================================================================================================
		public bool SendEvent(EventData eventData)
		{
			if (m_bDisposed)
				return false;

			try
			{
				// 송신 메세지 큐에 큐잉하는 함수 호출
				Send(eventData);
			}
			catch
			{
				return false;
			}

			return true;
		}

		//=====================================================================================================================
		// 클라이언트의 요청에 대한 응답 데이터 송신시 호출하는 함수
		//
		// operationResponse : 송신할 응답 데이터
		//=====================================================================================================================
		public bool SendResponse(OperationResponse operationResponse)
		{
			if (m_bDisposed)
				return false;

			try
			{
				// 송신 메세지 큐에 큐잉하는 함수 호출
				Send(operationResponse);
			}
			catch
			{
				return false;
			}

			return true;
		}

		//=====================================================================================================================
		// 송신 메세지를 큐잉 하고 송신을 시작하는 함수
		//
		// message : 송신할 데이터
		//=====================================================================================================================
		private void Send(IMessage message)
		{
			// 송신 메세지 큐 접근을 한번에 1건 처리하기 위해 m_sendLockObject 객체 lock 처리
			lock (m_sendLockObject)
			{
				// 송신 메세지 큐에 메시지 큐잉
				m_sendMessageQueue.Enqueue(message);

				// 송신 메세지 큐에 이미 데이터가 있었을 경우에는 전송중이기 때문에 StartSend() 호출하지 않음
				if (m_sendMessageQueue.Count > 1)
					return;
			}

			// 송신 시작
			StartSend();
		}

		//=====================================================================================================================
		// 데이터 송신을 위해 직렬화 처리와 송신을 처리하는 함수
		//=====================================================================================================================
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

				// 송신 메세지 큐 접근을 한번에 1건 처리하기 위해 m_sendLockObject 객체 lock 처리
				lock (m_sendLockObject)
				{
					// 첫번째 메세지 출력
					message = m_sendMessageQueue.Peek();
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
				// 사용을 끝낸 data 객체 큐에 반납
				if (data != null)
					m_dataQueue.ReturnData(data);
			}
		}

		//=====================================================================================================================
		// 데이터 송신 이후 호출 되는 콜백 함수
		//
		// result : 비동기 송신의 결과
		//=====================================================================================================================
		private void SendCallback(IAsyncResult result)
		{
			if (m_bDisposed)
				return;

			try
			{
				// 비동기 송신을 종료
				m_socket.EndSend(result);

				// 송신 메세지 큐 접근을 한번에 1건 처리하기 위해 m_sendLockObject 객체 lock 처리
				lock (m_sendLockObject)
				{
					// 송신 완료후 송신 메세지 큐에서 해당 데이터 삭제
					m_sendMessageQueue.Dequeue();

					// 송신 메세지 큐에 데이터가 없을 경우 송신 종료
					if (m_sendMessageQueue.Count == 0)
						return;
				}

				// 송신 메세지 큐에 아직 전달할 데이터가 있을경우 다시 송신 시작
				StartSend();
			}
			catch
			{
				// 소켓이 연결상태가 아닐경우 연결종료 처리
				if (!m_socket.Connected)
					Disconnect();
			}
		}

		//
		// Timeout
		//

		//=====================================================================================================================
		// 핑체크시간을 갱신하는 함수(클라이언트로부터 핑체크갱신 메세지가 왔을 경우 호출)
		//=====================================================================================================================
		private void OnPingCheck()
		{
			// 메세지가 도착한 시각으로 갱신
			m_lastPingCheckTime = DateTime.Now;

			// 클라이언트로 응답 송신
			ResponsePingCheck();
		}
		
		// 핑체크에 대한 응답시 사용하는 버퍼(전달할 내용이 없으므로 패킷 전송시 필요한 부가 정보만을 저장)
		private byte[] pingCheckBuffer = new byte[Data.kNonPacketDataSize];
		//=====================================================================================================================
		// 핑체크 메세지에 대한 응답
		//=====================================================================================================================
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

		//=====================================================================================================================
		// 핑체크응답 비동기 송신 이후 호출되는 콜백 함수
		//
		// result : 비동기 송신의 결과
		//=====================================================================================================================
		private void SendPingCheckCallback(IAsyncResult result)
		{
			if (m_bDisposed)
				return;

			try
			{
				// 송신 종료 처리
				m_socket.EndSend(result);
			}
			catch
			{
				// 소켓이 연결상태가 아닐경우 연결종료 처리
				if (!m_socket.Connected)
					Disconnect();
			}
		}

		//
		// Request
		//

		//=====================================================================================================================
		// 클라이언트 요청 메세지를 처리하는 추상 함수
		//
		// request : 클라이언트 요청 메세지
		//=====================================================================================================================
		protected abstract void OnOperationRequest(OperationRequest request);

		//=====================================================================================================================
		// 서버 종료시 클라이언트에 종료 패킷을 송신하는 함수
		//=====================================================================================================================
		private void SendDisconnectResponse()
		{
			try
			{
				// 클라이언트에 1바이트 메세지 송신
				m_socket.Send(new byte[] { 0 });
			}
			catch
			{
			}
		}

		//
		//
		//

		//=====================================================================================================================
		// 피어 연결종료 및 리소스 해제 처리 함수
		//=====================================================================================================================
		public void Disconnect()
		{
			if (m_bDisposed)
				return;

			m_bDisposed = true;

			try
			{
				// 송신 메세지 큐 접근을 한번에 1건 처리하기 위해 m_sendLockObject 객체 lock 처리
				lock (m_sendLockObject)
				{
					// 송신 메세지 큐의 데이터 전부 삭제
					m_sendMessageQueue.Clear();
				}

				// 데이터 큐 전부 삭제
				m_dataQueue.Clear();

				// 클라이언트에 종료 패킷 전달 함수 호출
				SendDisconnectResponse();

				// 클라이언트 소켓의 입력/출력스트림 종료
				m_socket.Shutdown(SocketShutdown.Both);
			}
			catch
			{

			}
			finally
			{
				// 클라이언트 소켓 종료
				m_socket.Close();

				// 서버 메인 클래스 객체에서 가지고 있는 해당 객체 삭제 처리
				m_applicationBase.RemovePeer(this);

				// 접속 종료 이후 호출되는 함수
				OnDisconnect();
			}
		}

		//=====================================================================================================================
		// 해당 클래스를 상속 받는 객체에서 접속 종료 이후 처리될 작업이 있을경우 오버라이딩하여 사용 할 수 있는 추상 함수
		//=====================================================================================================================
		protected abstract void OnDisconnect();
	}
}
