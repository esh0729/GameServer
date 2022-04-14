using System;
using System.Data;
using System.Data.SqlClient;

namespace GameServer
{
	public class GameDBDocEx
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public static int AddAccount(SqlConnection conn, SqlTransaction trans, Guid accountId, Guid userId, DateTimeOffset regTime)
		{
			SqlCommand sc = GameDBDoc.CSC_AddAccount(accountId, userId, regTime);
			sc.Connection = conn;
			sc.Transaction = trans;
			sc.Parameters.Add("ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

			sc.ExecuteNonQuery();

			return (int)sc.Parameters["ReturnValue"].Value;
		}
	}
}
