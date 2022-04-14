using System;
using System.Data;
using System.Data.SqlClient;

namespace GameServer
{
	public class UserDBDoc
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public static DataRow User(SqlConnection conn, SqlTransaction trans, Guid userId)
		{
			SqlCommand sc = new SqlCommand();
			sc.Connection = conn;
			sc.Transaction = trans;
			sc.CommandType = CommandType.StoredProcedure;
			sc.CommandText = "uspGSApi_User";
			sc.Parameters.Add("@userId", SqlDbType.UniqueIdentifier).Value = userId;

			DataTable dt = new DataTable();

			SqlDataAdapter sda = new SqlDataAdapter();
			sda.SelectCommand = sc;
			sda.Fill(dt);

			return dt.Rows.Count > 0 ? dt.Rows[0] : null;
		}

		public static DataRowCollection GameServers(SqlConnection conn, SqlTransaction trans)
		{
			SqlCommand sc = new SqlCommand();
			sc.Connection = conn;
			sc.Transaction = trans;
			sc.CommandType = CommandType.StoredProcedure;
			sc.CommandText = "uspGSApi_GameServers";

			DataTable dt = new DataTable();

			SqlDataAdapter sda = new SqlDataAdapter();
			sda.SelectCommand = sc;
			sda.Fill(dt);

			return dt.Rows;
		}

		public static SqlCommand CSC_UpdateUser_Login(Guid userId, int nLastJoinedServer, string sLastJoinedIPAddress)
		{
			SqlCommand sc = new SqlCommand();
			sc.CommandType = CommandType.StoredProcedure;
			sc.CommandText = "uspGSApi_UpdateUser_Login";
			sc.Parameters.Add("@userId", SqlDbType.UniqueIdentifier).Value = userId;
			sc.Parameters.Add("@nLastJoinedServer", SqlDbType.Int).Value = nLastJoinedServer;
			sc.Parameters.Add("@sLastJoinedIPAddress", SqlDbType.VarChar, 50).Value = sLastJoinedIPAddress;

			return sc;
		}
	}
}
