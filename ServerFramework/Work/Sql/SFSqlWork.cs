using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace ServerFramework
{
	//=====================================================================================================================
	// MS-SQL 데이터베이스에 대한 Sql(쿼리 또는 저장프로시저) 작업을 실행하는 클래스
	//=====================================================================================================================
	public class SFSqlWork 
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 데이터베이스 연결 객체
		private SqlConnection m_conn = null;

		// 실행할 Sql 작업 저장하고 있는 컬렉션
		private List<SqlCommand> m_sqlCommands = new List<SqlCommand>();
		// 데이터베이스 작업 요청자의 작업순서를 보장시켜주는 동기작업 객체를 저장하는 컬렉션
		private List<SFSyncWork> m_syncWorks = new List<SFSyncWork>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 생성자
		//
		// conn : 연결할 데이터베이스 객체
		//=====================================================================================================================
		public SFSqlWork(SqlConnection conn)
		{
			if (conn == null)
				throw new ArgumentNullException("conn");

			m_conn = conn;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// Sql 작업 추가를 처리하는 함수
		//
		// sqlCommand : 실행 할 Sql 작업
		//=====================================================================================================================
		public void AddCommand(SqlCommand sqlCommand)
		{
			m_sqlCommands.Add(sqlCommand);
		}

		//=====================================================================================================================
		// 동기작업 객체를 추가하는 함수
		//
		// syncWork : 동기작업 객체
		//=====================================================================================================================
		public void AddSyncWork(SFSyncWork syncWork)
		{
			if (syncWork == null)
				throw new ArgumentNullException("syncWork");

			m_syncWorks.Add(syncWork);
		}

		//=====================================================================================================================
		// 저장되어있는 Sql작업을 실행하는 함수
		//=====================================================================================================================
		public void Schedule()
		{
			// 동기작업 내의 신호가 대기중일 경우 데이터베이스 작업을 대기
			// 모든 동기작업이 신호 받음 상태일 경우 신호 대기를 걸고 신호 차단 처리
			foreach (SFSyncWork syncWork in m_syncWorks)
			{
				syncWork.Waiting();
			}

			//
			//
			//

			SqlTransaction trans = null;

			// 데이터베이스 작업중 하나라도 에러가 발생할 경우 Rollback을 처리하기 위해 try문으로 처리
			try
			{
				// 데이터베이스 연결
				m_conn.Open();

				// 트랜잭션 시작
				trans = m_conn.BeginTransaction();

				// Sql 작업 컬렉션에 있는 모든 작업 순차적으로 실행
				foreach (SqlCommand sc in m_sqlCommands)
				{
					sc.Connection = m_conn;
					sc.Transaction = trans;

					sc.ExecuteNonQuery();
				}

				// 모든 작업이 끝났을 경우 트랜잭션 커밋 후 트랜잭션 객체 null 처리
				trans.Commit();
				trans = null;

				// 연결을 닫고 연결객체 null 처리
				m_conn.Close();
				m_conn = null;
			}
			catch
			{
				// m_conn, trans가 null이 아닐경우는 작업도중 에러가 발생할 경우
				if (m_conn != null)
				{
					if (trans != null)
					{
						// 모든 작업데 대한 롤백 처리
						trans.Rollback();
						trans = null;
					}

					// 데이터베이스 연결 닫기
					m_conn.Close();
					m_conn = null;
				}

				throw;
			}
			finally
			{
				// 모든 작업을 끝났을 경우 동기작업의 신호를 받기 상태로 전환하여 대기하고 있던 작업이 진행될수 있도록 처리
				foreach (SFSyncWork syncWork in m_syncWorks)
				{
					syncWork.Set();
				}
			}
		}
	}
}
