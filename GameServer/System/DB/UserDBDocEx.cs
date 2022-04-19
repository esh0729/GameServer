using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace GameServer
{
	public class UserDBDocEx
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static int AddUserHero(
			SqlConnection conn,
			SqlTransaction trans,
			Guid userId,
			Guid heroId,
			string sName,
			int nCharacterId)
		{
			SqlCommand sc = UserDBDoc.CSC_AddUserHero(userId, heroId, sName, nCharacterId);
			sc.Connection = conn;
			sc.Transaction = trans;
			sc.Parameters.Add("ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

			sc.ExecuteNonQuery();

			return Convert.ToInt32(sc.Parameters["ReturnValue"].Value);
		}

		public static int AddHeroName(SqlConnection conn, SqlTransaction trans, string sName, Guid heroId)
		{
			SqlCommand sc = UserDBDoc.CSC_AddHeroName(sName, heroId);
			sc.Connection = conn;
			sc.Transaction = trans;
			sc.Parameters.Add("ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

			sc.ExecuteNonQuery();

			return Convert.ToInt32(sc.Parameters["ReturnValue"].Value);
		}
	}
}
