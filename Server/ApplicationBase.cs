using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
	//=====================================================================================================================
	// 서버의 메인 클래스, 서버의 시작과 클라이언트 소켓의 aceppt 처리를 담당하는 추상 클래스
	//=====================================================================================================================

	public abstract class ApplicationBase
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables
		
		// 소켓 리스너
		private Socket m_listener = null;
		// 클라이언트 socket accept시 생성되는 PeerBase를 저장하는 컬렉션
		private Dictionary<Guid, PeerBase> m_peers = new Dictionary<Guid, PeerBase>();
		// m_peers 객체에 접근하기 위한 lock object 해당 컬렉션은 비동기로 접근되기 때문에 접근시 해당 객체 lock 처리 이후 접근
		private object m_syncObject = new object();

		// 접속타임아웃간격 0보다 클 경우에만 체크
		private int m_nConnectionTimeoutInternal = 0;
		// 해당객체 리소스 해제 여부
		private bool m_bDiposed = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 접속타임아웃간격 0보다 클 경우에만 체크
		public int connectionTimeoutInterval
		{
			get { return m_nConnectionTimeoutInternal; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 서버의 시작 함수
		//
		// nPort : 리스너 소켓의 포트 번호
		// nBackLogCount : 연결 요청 대기큐의 최대 대기 수
		// nConnectionTimeoutInterval : 접속타임아웃간격
		//=====================================================================================================================
		protected void Start(int nPort, int nBackLogCount = 128, int nConnectionTimeoutInterval = 30000)
		{
			// 리스너 소켓 생성(TCP)
			m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			// 주소 및 포트 설정
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, nPort);
			// 리스너 소켓에 주소 및 포트 할당
			m_listener.Bind(ipEndPoint);
			// 리스너 소켓 연결 대기 시작
			m_listener.Listen(nBackLogCount);
			// 타임아웃간격 설정
			m_nConnectionTimeoutInternal = nConnectionTimeoutInterval;

			// 비동기 연결 대기
			m_listener.BeginAccept(AcceptCallback, null);
		}

		//=====================================================================================================================
		// 클라이언트 연결 완료시 호출 되는 추상 함수
		//
		// peerInit : 클라이언트 소켓 정보가 담겨있는 객체
		//=====================================================================================================================
		protected abstract PeerBase CreatePeer(PeerInit peerInit);

		//=====================================================================================================================
		// 클라이언트 소켓 연결중 에러 발생시 호출 되는 가상 함수
		//
		// ex : 발생 오류
		//=====================================================================================================================
		protected virtual void OnAccpetError(Exception ex)
		{
		}

		//=====================================================================================================================
		// 클라이언트 소켓 연결 완료시 호출 되는 콜백 함수
		//
		// result : 비동기 연결 완료시 전달되는 상태 정보
		//=====================================================================================================================
		private void AcceptCallback(IAsyncResult result)
		{
			if (m_bDiposed)
				return;

			PeerBase peer = null;

			try
			{
				// 연결 처리 종료
				Socket client = m_listener.EndAccept(result);

				// m_peers 객체는 비동기로 접근 되어 추가 / 삭제 될수 있으므로 m_syncObject 객체 lock 처리 이후 접근 
				lock (m_syncObject)
				{
					// Peer 생성하여 컬렉션에 저장
					peer = CreatePeer(new PeerInit(this, client));
					m_peers.Add(peer.id, peer);
				}
			}
			catch (Exception ex)
			{
				// 피어 생성 이후 에러 발생시 해당 피어 삭제
				if (peer != null)
					peer.Disconnect();

				// 에러 전달
				OnAccpetError(ex);
			}
			finally
			{
				// 모든 작업 완료시 다시 비동기 연결 대기 상태로 전환
				m_listener.BeginAccept(AcceptCallback, null);
			}
		}

		//=====================================================================================================================
		// 주기적으로 호출 되어야 할 갱신 함수, 내부에서 주기적으로 처리 할 작업을 처리
		//=====================================================================================================================
		public void Service()
		{
			if (m_bDiposed)
				return;

			// peer의 Service함수 내부에서 Timeout 시간을 체크하기 때문에 m_peers 컬렉션의 수정이 이루어 질 수 있으므로 m_syncObject객체 lock 필요
			lock (m_syncObject)
			{
				// m_peers 컬렉션 수정이 이루어 질 수 있으므로 ToArray함수로 복사하여 foreach 실행
				foreach (PeerBase peer in m_peers.Values.ToArray())
				{
					// 피어 갱신
					peer.Service();
				}
			}
		}

		//=====================================================================================================================
		// 피어를 삭제 할때 호출하는 함수
		//
		// peer : 삭제할 피어
		//=====================================================================================================================
		public void RemovePeer(PeerBase peer)
		{
			// m_peers 컬렉션의 수정이 이루어 지므로 m_syncObject객체 lock 필요
			lock (m_syncObject)
			{
				m_peers.Remove(peer.id);
			}
		}

		//=====================================================================================================================
		// 리소스 해제 함수
		//=====================================================================================================================
		public void Dispose()
		{
			// 이미 리소스 해제가 이루어 졌을 경우에는 return
			if (m_bDiposed)
				return;

			m_bDiposed = true;

			// m_peers 컬렉션의 수정이 이루어 지므로 m_syncObject객체 lock 필요
			lock (m_syncObject)
			{
				// m_peers 컬렉션 수정이 이루어 질 수 있으므로 ToArray함수로 복사하여 foreach 실행
				foreach (PeerBase peer in m_peers.Values.ToArray())
				{
					// 클라이언트 피어 접속 종료 처리
					peer.Disconnect();
				}
			}

			// 리스너 소켓 종료
			m_listener.Close();

			//
			//
			//

			// 서버 종료 작업 완료 이후 호출
			OnTearDown();
		}

		//=====================================================================================================================
		// 서버 종료 이후 처리할 작업을 시작하기 위한 추상 함수
		//=====================================================================================================================
		protected abstract void OnTearDown();
	}
}