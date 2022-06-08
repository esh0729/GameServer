using System.Data.SqlClient;

namespace GameServer
{
	public class Util
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//
		// Sql
		//

		public static SqlConnection CreateDBConnection(string sDBPath)
		{
			return new SqlConnection(sDBPath);
		}

		public static SqlConnection CreateUserDBConnection()
		{
			return CreateDBConnection(AppConfig.userDBConnection);
		}

		public static SqlConnection CreateGameDBConnection()
		{
			return CreateDBConnection(Server.instance.currentGameServer.dbPath);
		}

		public static SqlConnection OpenDBConnection(string sDBPath)
		{
			SqlConnection conn = new SqlConnection(sDBPath);
			conn.Open();

			return conn;
		}

		public static SqlConnection OpenUserDBConnection()
		{
			return OpenDBConnection(AppConfig.userDBConnection);
		}

		public static SqlConnection OpenGameDBConnection()
		{
			return OpenDBConnection(Server.instance.currentGameServer.dbPath);
		}

		public static void Close(ref SqlConnection conn)
		{
			conn.Close();
			conn = null;
		}

		public static void Commit(ref SqlTransaction trans)
		{
			trans.Commit();
			trans = null;
		}

		public static void Rollback(ref SqlTransaction trans)
		{
			trans.Rollback();
			trans = null;
		}
	}
}
