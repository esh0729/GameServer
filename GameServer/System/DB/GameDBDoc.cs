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
			sc.CommandText = "uspGSApi_HeroCount";
			sc.Parameters.Add("@accountId", SqlDbType.UniqueIdentifier).Value = accountId;

			return Convert.ToInt32(sc.ExecuteScalar());
		}

		public static DataRow Hero(SqlConnection conn, SqlTransaction trans, Guid heroId)
		{
			SqlCommand sc = new SqlCommand();
			sc.Connection = conn;
			sc.Transaction = trans;
			sc.CommandType = CommandType.StoredProcedure;
			sc.CommandText = "uspGSApi_Hero";
			sc.Parameters.Add("@heroId", SqlDbType.UniqueIdentifier).Value = heroId;

			DataTable dt = new DataTable();

			SqlDataAdapter sda = new SqlDataAdapter();
			sda.SelectCommand = sc;
			sda.Fill(dt);

			return dt.Rows.Count > 0 ? dt.Rows[0] : null;
		}

		public static SqlCommand CSC_HeroLogin(Guid heroId, DateTimeOffset lastLoginTime)
		{
			SqlCommand sc = new SqlCommand();
			sc.CommandType = CommandType.StoredProcedure;
			sc.CommandText = "uspGSApi_HeroLogin";
			sc.Parameters.Add("@heroId", SqlDbType.UniqueIdentifier).Value = heroId;
			sc.Parameters.Add("@lastLoginTime", SqlDbType.DateTimeOffset).Value = lastLoginTime;

			return sc;
		}

		public static SqlCommand CSC_HeroLogout(
			Guid heroId,
			DateTimeOffset lastLogoutTime,
			int nLastContinentId,
			float fLastXPosition,
			float fLastYPosition,
			float fLastZPosition,
			float fLastYRotation,
			int nPreviousContinentId,
			float fPreviousXPosition,
			float fPreviousYPosition,
			float fPreviousZPosition,
			float fPreviousYRotation)
		{
			SqlCommand sc = new SqlCommand();
			sc.CommandType = CommandType.StoredProcedure;
			sc.CommandText = "uspGSApi_HeroLogout";
			sc.Parameters.Add("@heroId", SqlDbType.UniqueIdentifier).Value = heroId;
			sc.Parameters.Add("@lastLogoutTime", SqlDbType.DateTimeOffset).Value = lastLogoutTime;
			sc.Parameters.Add("@nLastContinentId", SqlDbType.Int).Value = nLastContinentId;
			sc.Parameters.Add("@fLastXPosition", SqlDbType.Float).Value = fLastXPosition;
			sc.Parameters.Add("@fLastYPosition", SqlDbType.Float).Value = fLastYPosition;
			sc.Parameters.Add("@fLastZPosition", SqlDbType.Float).Value = fLastZPosition;
			sc.Parameters.Add("@fLastYRotation", SqlDbType.Float).Value = fLastYRotation;
			sc.Parameters.Add("@nPreviousContinentId", SqlDbType.Int).Value = nPreviousContinentId;
			sc.Parameters.Add("@fPreviousXPosition", SqlDbType.Float).Value = fPreviousXPosition;
			sc.Parameters.Add("@fPreviousYPosition", SqlDbType.Float).Value = fPreviousYPosition;
			sc.Parameters.Add("@fPreviousZPosition", SqlDbType.Float).Value = fPreviousZPosition;
			sc.Parameters.Add("@fPreviousYRotation", SqlDbType.Float).Value = fPreviousYRotation;

			return sc;
		}
	}
}
