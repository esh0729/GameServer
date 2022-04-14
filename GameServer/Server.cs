using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Server;
using ServerFramework;

namespace GameServer
{
	public class Server : ApplicationBase
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private SFWorker m_logWorker = null;
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
			}
			finally
			{
				if (conn != null)
					Util.Close(ref conn);
			}

			//
			// 
			//

			Cache.instance.Init();

			//
			// 서버 시작
			//

			Start(7000, 10);

			LogUtil.Info(GetType(), "GameServer Started.");
			Console.WriteLine("Server start");
		}

		//
		// 시스템데이터
		//

		private void InitSystemData(SqlConnection conn)
		{
			LogUtil.Info(GetType(), "SystemData Init Started.");

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

			LogUtil.Info(GetType(), "SystemData Init Completed.");
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

			return clientPeer;
		}

		public void RemovePeer(Guid id)
		{
			m_clientPeers.Remove(id);
		}

		public void AddLogWork(ISFWork work)
		{
			m_logWorker.Add(work);
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
			Server server = Server.instance;

			try
			{
				server.Init();
			}
			catch (Exception ex)
			{
				LogUtil.Error(typeof(Server), ex);
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
				}
			}

			//
			//
			//

			server.Dispose();
		}
	}
}
