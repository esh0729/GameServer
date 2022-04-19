﻿using System;
using System.Data;
using System.Data.SqlClient;

namespace GameServer
{
	public class GameDBDocEx
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static int AddAccount(SqlConnection conn, SqlTransaction trans, Guid accountId, Guid userId, DateTimeOffset regTime)
		{
			SqlCommand sc = GameDBDoc.CSC_AddAccount(accountId, userId, regTime);
			sc.Connection = conn;
			sc.Transaction = trans;
			sc.Parameters.Add("ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

			sc.ExecuteNonQuery();

			return Convert.ToInt32(sc.Parameters["ReturnValue"].Value);
		}

		public static int AddHero(
			SqlConnection conn,
			SqlTransaction trans,
			Guid accountId,
			Guid heroId,
			string sName,
			int nCharacterId,
			DateTimeOffset regTime)
		{
			SqlCommand sc = GameDBDoc.CSC_AddHero(accountId, heroId, sName, nCharacterId, regTime);
			sc.Connection = conn;
			sc.Transaction = trans;
			sc.Parameters.Add("ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

			sc.ExecuteNonQuery();

			return Convert.ToInt32(sc.Parameters["ReturnValue"].Value);
		}
	}
}
