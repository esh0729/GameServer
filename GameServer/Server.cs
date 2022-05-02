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
	public class Server : ApplicationBase
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private SFWorker m_logWorker = null;
		private SFMultiWorker m_standaloneWorker = null;

		private CommandHandlerFactory m_commandHandlerFactory = null;

		private Dictionary<int,GameServer> m_gameServers = new Dictionary<int,GameServer>();

		private Dictionary<Guid, ClientPeer> m_clientPeers = new Dictionary<Guid, ClientPeer>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		private Server()
		{
			//
			// 로그 작업자
			//

			m_logWorker = new SFWorker();
			m_logWorker.Start();

			//
			// 독립 작업자
			//

			m_standaloneWorker = new SFMultiWorker();
			m_standaloneWorker.Start();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public GameServer currentGameServer
		{
			get { return GetGameServer(AppConfig.gameServerId); }
		}

		public CommandHandlerFactory commandHnadlerFactory
		{
			get { return m_commandHandlerFactory; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Init()
		{
			int nMinWorkerThreads;
			int nMaxWorkerThreads;
			int nMinCompletionPortThreads;
			int nMaxCompletionPortThreads;

			ThreadPool.GetMinThreads(out nMinWorkerThreads, out nMinCompletionPortThreads);
			ThreadPool.GetMaxThreads(out nMaxWorkerThreads, out nMaxCompletionPortThreads);

			Console.WriteLine("nMinWorkerThreads = " + nMinWorkerThreads + ", nMaxWorkerThreads = " + nMaxWorkerThreads + ", nMinCompletionPortThreads = " + nMinCompletionPortThreads + ", nMaxCompletionPortThreads = " + nMaxCompletionPortThreads);
			ThreadPool.SetMinThreads(nMaxWorkerThreads, nMaxCompletionPortThreads);

			//
			// 명령핸들러팩토리
			//

			m_commandHandlerFactory = new CommandHandlerFactory();
			m_commandHandlerFactory.Init();

			//
			//
			//

			SqlConnection conn = null;

			try
			{
				conn = Util.OpenUserDBConnection();

				//
				// 시스템 데이터
				//

				InitSystemData(conn);

				//
				// 리소스
				//

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

			lock (Cache.instance.syncObject)
			{
				Cache.instance.Init();
			}

			//
			// 서버 시작
			//

			Start(7000);

			LogUtil.System(GetType(), "GameServer Started.");
			Console.WriteLine("Server start");
		}

		//
		// 시스템데이터
		//

		private void InitSystemData(SqlConnection conn)
		{
			LogUtil.System(GetType(), "SystemData Init Started.");

			//
			// 게임서버
			//

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

		//
		// 게임서버
		//

		public GameServer GetGameServer(int nId)
		{
			GameServer value;

			return m_gameServers.TryGetValue(nId, out value) ? value : null;
		}

		//
		// 피어
		//

		protected override PeerBase CreatePeer(PeerInit peerInit)
		{
			ClientPeer clientPeer = new ClientPeer(peerInit);
			m_clientPeers.Add(clientPeer.id, clientPeer);

			Console.WriteLine("Connect: Address = " + clientPeer.ipAddress + ", Port = " + clientPeer.port + ", PeerCount = " + m_clientPeers.Count);

			return clientPeer;
		}

		public void RemoveClientPeer(ClientPeer clientPeer)
		{
			m_clientPeers.Remove(clientPeer.id);

			Console.WriteLine("Disconnect = Address = " + clientPeer.ipAddress + ", Port = " + clientPeer.port + ", PeerCount = " + m_clientPeers.Count);
		}

		//
		// 로그 작업자
		//

		public void AddLogWork(ISFWork work)
		{
			m_logWorker.Add(work);
		}
		
		//
		// 독립 작업자
		//

		public void AddStandalonWork(ISFWork work)
		{
			m_standaloneWorker.Add(work);
		}

		//
		//
		//

		protected override void OnTearDown()
		{
			LogUtil.System(GetType(), "OnTearDownStarted.");

			//
			//
			//

			lock (Cache.instance.syncObject)
			{
				Cache.instance.Dispose();
			}

			//
			//
			//

			LogUtil.System(GetType(), "OnTearDownFinished.");

			//
			// 독립 작업자 종료
			//

			m_standaloneWorker.Stop();

			//
			// 로그 작업자 종료
			//

			m_logWorker.Stop();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static variables

		private static Server s_instance = new Server();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public static Server instance
		{
			get { return s_instance; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static void Main(string[] args)
		{
			handler = new ConsoleEventDelegate(ConsoleEventCallback);
			SetConsoleCtrlHandler(handler, true);

			Server server = Server.instance;

			try
			{
				server.Init();
			}
			catch (Exception ex)
			{
				LogUtil.Error(typeof(Server), ex);
				server.Dispose();
				return;
			}

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

		private static bool ConsoleEventCallback(int nEventType)
		{
			if (nEventType == 2)
			{
				Server.instance.Dispose();
			}

			return false;
		}

		private delegate bool ConsoleEventDelegate(int nEventType);
		private static ConsoleEventDelegate handler;
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool bAdd);
	}
}
