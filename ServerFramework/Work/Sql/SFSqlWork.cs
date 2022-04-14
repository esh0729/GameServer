using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace ServerFramework
{
	public class SFSqlWork 
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private SqlConnection m_conn = null;

		private List<SqlCommand> m_sqlCommands = new List<SqlCommand>();
		private List<SFSyncWork> m_syncWorks = new List<SFSyncWork>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public SFSqlWork(SqlConnection conn)
		{
			if (conn == null)
				throw new ArgumentNullException("conn");

			m_conn = conn;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void AddCommand(SqlCommand sqlCommand)
		{
			m_sqlCommands.Add(sqlCommand);
		}

		public void AddSyncWork(SFSyncWork syncWork)
		{
			if (syncWork == null)
				throw new ArgumentNullException("syncWork");

			m_syncWorks.Add(syncWork);
		}

		public void Schedule()
		{
			foreach (SFSyncWork syncWork in m_syncWorks)
			{
				syncWork.Waiting();
			}

			//
			//
			//

			SqlTransaction trans = null;

			try
			{
				trans = m_conn.BeginTransaction();

				foreach (SqlCommand sc in m_sqlCommands)
				{
					sc.Connection = m_conn;
					sc.Transaction = trans;

					sc.ExecuteNonQuery();
				}

				trans.Commit();
				trans = null;

				m_conn.Close();
				m_conn = null;
			}
			catch
			{
				if (m_conn != null)
				{
					if (trans != null)
					{
						trans.Rollback();
						trans = null;
					}

					m_conn.Close();
					m_conn = null;
				}

				throw;
			}
			finally
			{
				foreach (SFSyncWork syncWork in m_syncWorks)
				{
					syncWork.Set();
				}
			}
		}
	}
}
