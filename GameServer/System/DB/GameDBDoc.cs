using System;
using System.Data;
using System.Data.SqlClient;

namespace GameServer
{
	public class GameDBDoc
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public static SqlCommand CSC_AddAccount(Guid accountId, Guid userId, DateTimeOffset regTime)
		{
			SqlCommand sc = new SqlCommand();
			sc.CommandType = CommandType.StoredProcedure;
			sc.CommandText = "uspGSApi_AddAccount";
			sc.Parameters.Add("@accountId", SqlDbType.UniqueIdentifier).Value = accountId;
			sc.Parameters.Add("@userId", SqlDbType.UniqueIdentifier).Value = userId;
			sc.Parameters.Add("@regTime", SqlDbType.DateTimeOffset).Value = regTime;

			return sc;
		}

		public static DataRow Account(SqlConnection conn, SqlTransaction trans, Guid userId)
		{
			SqlCommand sc = new SqlCommand();
			sc.Connection = conn;
			sc.Transaction = trans;
			sc.CommandType = CommandType.StoredProcedure;
			sc.CommandText = "uspGSApi_Account";
			sc.Parameters.Add("@userId", SqlDbType.UniqueIdentifier).Value = userId;

			DataTable dt = new DataTable();

			SqlDataAdapter sda = new SqlDataAdapter();
			sda.SelectCommand = sc;
			sda.Fill(dt);

			return dt.Rows.Count > 0 ? dt.Rows[0] : null;
		}

		public static SqlCommand CSC_AddHero(Guid accountId, Guid heroId, string sName, int nCharacterId, DateTimeOffset regTime)
		{
			SqlCommand sc = new SqlCommand();
			sc.CommandType = CommandType.StoredProcedure;
			sc.CommandText = "uspGSApi_AddHero";
			sc.Parameters.Add("@accountId", SqlDbType.UniqueIdentifier).Value = accountId;
			sc.Parameters.Add("@heroId", SqlDbType.UniqueIdentifier).Value = heroId;
			sc.Parameters.Add("@sName", SqlDbType.NVarChar, 50).Value = sName;
			sc.Parameters.Add("@nCharacterId", SqlDbType.Int).Value = nCharacterId;
			sc.Parameters.Add("@regTime", SqlDbType.DateTimeOffset).Value = regTime;

			return sc;
		}

		public static DataRowCollection Heroes(SqlConnection conn, SqlTransaction trans, Guid accountId)
		{
			SqlCommand sc = new SqlCommand();
			sc.Connection = conn;
			sc.Transaction = trans;
			sc.CommandType = CommandType.StoredProcedure;
			sc.CommandText = "uspGSApi_Heroes";
			sc.Parameters.Add("@accountId", SqlDbType.UniqueIdentifier).Value = accountId;

			DataTable dt = new DataTable();

			SqlDataAdapter sda = new SqlDataAdapter();
			sda.SelectCommand = sc;
			sda.Fill(dt);

			return dt.Rows;
		}

		public static int HeroCount(SqlConnection conn, SqlTransaction trans, Guid accountId)
		{
			SqlCommand sc = new SqlCommand();
			sc.Connection = conn;
			sc.Transaction = trans;
			sc.CommandType = CommandType.StoredProcedure;
			sc.CommandText = "uspGSApi_Heroes";
			sc.Parameters.Add("@accountId", SqlDbType.UniqueIdentifier).Value = accountId;

			return Convert.ToInt32(sc.ExecuteScalar());
		}
	}
}
