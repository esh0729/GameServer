using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Threading;

using Server;
using ServerFramework;

namespace GameServer
{
	//=====================================================================================================================
	// (ApplicationBase 상속) 서버의 초기화 및 시작, 인게임의 클라이언트 피어를 관리하는 클래스
	//=====================================================================================================================
	public class Server : ApplicationBase
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 로그 등록 작업자
		private SFWorker m_logWorker = null;
		// 비동기 독립 작업자
		private SFMultiWorker m_standaloneWorker = null;

		// 클라이언트 명령 핸들러 타입 컬렉션 객체
		private CommandHandlerFactory m_commandHandlerFactory = null;

		// 게임서버데이터 컬렉션
		private Dictionary<int,GameServer> m_gameServers = new Dictionary<int,GameServer>();

		// 클라이언트 피어 컬렉션
		private Dictionary<Guid, ClientPeer> m_clientPeers = new Dictionary<Guid, ClientPeer>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 싱글톤 구현을 위한 private 생성자
		//=====================================================================================================================
		private Server()
		{
			// 로그 작업자 시작
			m_logWorker = new SFWorker();
			m_logWorker.Start();

			// 비동기 독립 작업자 시작
			m_standaloneWorker = new SFMultiWorker();
			m_standaloneWorker.Start();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 현재 게임서버 데이터
		public GameServer currentGameServer
		{
			get { return GetGameServer(AppConfig.gameServerId); }
		}

		// 클라이언트 명령 핸들러 타입 컬렉션
		public CommandHandlerFactory commandHnadlerFactory
		{
			get { return m_commandHandlerFactory; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 초기화 함수(스레드풀 초기 갯수 설정 및, 시스템에 필요한 데이터 초기화 후 서버 기동)
		//=====================================================================================================================
		public void Init()
		{
			// 스레드풀 초기 갯수 설정을 위한 변수
			int nMinWorkerThreads;
			int nMaxWorkerThreads;
			int nMinCompletionPortThreads;
			int nMaxCompletionPortThreads;

			// 스레드풀 최소 개수 및 최대 개수 호출
			ThreadPool.GetMinThreads(out nMinWorkerThreads, out nMinCompletionPortThreads);
			ThreadPool.GetMaxThreads(out nMaxWorkerThreads, out nMaxCompletionPortThreads);

			// 드레드풀 최소 개수를 사용할수 있는 최대 개수로 설정
			Console.WriteLine("nMinWorkerThreads = " + nMinWorkerThreads + ", nMaxWorkerThreads = " + nMaxWorkerThreads + ", nMinCompletionPortThreads = " + nMinCompletionPortThreads + ", nMaxCompletionPortThreads = " + nMaxCompletionPortThreads);
			ThreadPool.SetMinThreads(nMaxWorkerThreads, nMinCompletionPortThreads);

			// 클라이언트 명령 핸들러 타입 컬렉션 객체 초기화
			m_commandHandlerFactory = new CommandHandlerFactory();
			m_commandHandlerFactory.Init();

			//
			//
			//

			SqlConnection conn = null;

			try
			{
				conn = Util.OpenUserDBConnection();

				// 시스템관련 데이터 초기화
				InitSystemData(conn);

				// 리소스 데이터 초기화
				Resource.instance.Init(conn);
			}
			finally
			{
				if (conn != null)
					Util.Close(ref conn);
			}

			//
			// 
			//

			// Cache 데이터 접근을 위한 싱크객체 lock 처리
			lock (Cache.instance.syncObject)
			{
				// Cache 초기화
				Cache.instance.Init();
			}

			// 서버 시작
			Start(AppConfig.port);

			LogUtil.System(GetType(), "GameServer Started.");
			Console.WriteLine("Server start");
		}

		//=====================================================================================================================
		// 시스템관련 데이터 초기화 함수
		//
		// conn : 데이터베이스 접근을 위한 연결 객체
		//=====================================================================================================================
		private void InitSystemData(SqlConnection conn)
		{
			LogUtil.System(GetType(), "SystemData Init Started.");

			// 게임서버 데이터 데이터베이스에서 로드
			foreach (DataRow dr in UserDBDoc.GameServers(conn, null))
			{
				GameServer gameServer = new GameServer();
				gameServer.Set(dr);

				m_gameServers.Add(gameServer.id, gameServer);
			}

			//
			//
			//

			LogUtil.System(GetType(), "SystemData Init Completed.");
		}

		//=====================================================================================================================
		// 게임서버 데이터 호출 함수
		// Return : 게임서버 ID와 매칭 되는 게임서버 데이터 또는 null
		//
		// nId : 요청 게임서버 ID
		//=====================================================================================================================
		public GameServer GetGameServer(int nId)
		{
			GameServer value;

			return m_gameServers.TryGetValue(nId, out value) ? value : null;
		}

		//=====================================================================================================================
		// 클라이언트 피어 생성 함수
		// Return : 생성된 클라이언트 피어
		//
		// peerInit : 생성에 필요한 클라이언트 관련 정보
		//=====================================================================================================================
		protected override PeerBase CreatePeer(PeerInit peerInit)
		{
			// 클라이언트 피어 생성 및 초기화
			ClientPeer clientPeer = new ClientPeer(peerInit);
			clientPeer.Start();

			// 컬렉션에 저장
			m_clientPeers.Add(clientPeer.id, clientPeer);

			Console.WriteLine("Connect: Address = " + clientPeer.ipAddress + ", Port = " + clientPeer.port + ", PeerCount = " + m_clientPeers.Count);

			return clientPeer;
		}

		//=====================================================================================================================
		// 클라이언트 피어 삭제 함수
		//
		// clientPeer : 삭제할 클라이언트 피어
		//=====================================================================================================================
		public void RemoveClientPeer(ClientPeer clientPeer)
		{
			m_clientPeers.Remove(clientPeer.id);

			Console.WriteLine("Disconnect = Address = " + clientPeer.ipAddress + ", Port = " + clientPeer.port + ", PeerCount = " + m_clientPeers.Count);
		}

		//
		// 로그 작업자
		//

		//=====================================================================================================================
		// 로그 등록 큐잉 함수
		//
		// work : 로그 등록이 필요한 작업
		//=====================================================================================================================
		public void AddLogWork(ISFWork work)
		{
			// 로그 작업자 큐에 등록
			m_logWorker.Add(work);
		}

		//
		// 비동기 독립 작업자
		//

		//=====================================================================================================================
		// 비동기 작업 큐잉 함수
		//
		// work : 비동기 작업이 필요한 작업
		//=====================================================================================================================
		public void AddStandalonWork(ISFWork work)
		{
			// 비동기 독립 작업자 큐에 등록
			m_standaloneWorker.Add(work);
		}

		//
		//
		//

		//=====================================================================================================================
		// 서버 종료후 후처리 작업 처리 함수
		//=====================================================================================================================
		protected override void OnTearDown()
		{
			LogUtil.System(GetType(), "OnTearDownStarted.");

			//
			//
			//

			// Cache 데이터 접근을 위한 싱크객체 lock 처리
			lock (Cache.instance.syncObject)
			{
				// Cache 리소스 해제
				Cache.instance.Dispose();
			}

			//
			//
			//

			LogUtil.System(GetType(), "OnTearDownFinished.");

			// 비동기 독립 작업자 종료
			m_standaloneWorker.Stop();

			// 로그 작업자 종료
			m_logWorker.Stop();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static variables

		// 싱글톤 객체 인스턴스
		private static Server s_instance = new Server();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 싱글톤 객체 인스턴스
		public static Server instance
		{
			get { return s_instance; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		//=====================================================================================================================
		// 프로그램 메인 함수
		//
		// args : 명령행에서 전달된 매개변수
		//=====================================================================================================================
		public static void Main(string[] args)
		{
			// 프로그램 종료후 호출될 콜백함수 등록
			handler = new ConsoleEventDelegate(ConsoleEventCallback);
			SetConsoleCtrlHandler(handler, true);

			Server server = Server.instance;

			try
			{
				// 게임 서버 초기화
				server.Init();
			}
			catch (Exception ex)
			{
				// 게임 서버 초기화 중 에러 발생시 에러로그 등록 및 리소스 해제
				LogUtil.Error(typeof(Server), ex);
				server.Dispose();
				return;
			}

			// 게임서버 업데이트 함수 주기적으로 호출
			while (true)
			{
				try
				{
					server.Service();
				}
				catch (Exception ex)
				{
					LogUtil.Error(typeof(Server), ex);

					throw;
				}
			}
		}

		//=====================================================================================================================
		// 프로그램 종료후 처리가 필요한 작업을 처리하는 함수
		//
		// nEventType : 프로그램 이벤트 타입
		//=====================================================================================================================
		private static bool ConsoleEventCallback(int nEventType)
		{
			// 프로그램 종료시 처리할 부분
			if (nEventType == 2)
			{
				// 서버 리소스 해제
				Server.instance.Dispose();
			}

			return false;
		}

		//
		//
		//

		// 프로그램 이벤트 발생시 호출 되는 대리자 선언
		private delegate bool ConsoleEventDelegate(int nEventType);
		// 프로그램 이벤트 발생시 호출 되는 대리자 객체 선언
		private static ConsoleEventDelegate handler;

		// 프로그램 이벤트 처리 외부 함수 선언
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool bAdd);
	}
}
