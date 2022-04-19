using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using ClientCommon;
using ServerFramework;

namespace GameServer
{
	public class LoginCommandHandler : CommandHandler<LoginCommandBody,LoginResponseBody>
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private AccessToken m_accessToken = null;
		private Account m_account = null;

		private DateTimeOffset m_currentTime = DateTimeOffset.MinValue;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		protected override bool globalLockRequired
		{
			get { return true; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnCommandHandle()
		{
			m_currentTime = DateTimeUtil.currentTime;

			if (m_body == null)
				throw new CommandHandleException(kResult_Error, "body가 null입니다.");

			string sAccessToken = m_body.accessToken;

			if (string.IsNullOrEmpty(sAccessToken))
				throw new CommandHandleException(kResult_Error, "액세스 토큰이 존재하지 않습니다.");

			m_accessToken = new AccessToken();

			if (!m_accessToken.Parse(sAccessToken))
				throw new CommandHandleException(kResult_Error, "액세스 토큰 파싱에 실패하였습니다.");

			if (!m_accessToken.Verify())
				throw new CommandHandleException(kResult_Error, "엑세스 토큰이 유효하지 않습니다.");

			RunnableStandaloneWork(Process);
		}

		private void Process()
		{
			SqlConnection userConn = null;
			SqlConnection gameConn = null;
			SqlTransaction gameTrans = null;

			try
			{
				//
				// UserDB 작업
				//

				{
					userConn = Util.OpenUserDBConnection();

					//
					// 사용자 조회
					//

					DataRow drUser = UserDBDoc.User(userConn, null, m_accessToken.userId);

					//
					// 사용자 검사
					//

					if (drUser == null)
						throw new CommandHandleException(kResult_Error, "사용자 정보가 존재하지 않습니다.");

					Guid userId = DBUtil.ToGuid(drUser["userId"]);
					string sAccessSecret = Convert.ToString(drUser["accessSecret"]);

					if (m_accessToken.accessSecret != sAccessSecret)
						throw new CommandHandleException(kResult_Error, "사용자 정보가 유효하지 않습니다.");
				}

				//
				// GameDB 작업
				//

				{
					gameConn = Util.OpenGameDBConnection();
					gameTrans = gameConn.BeginTransaction();

					//
					// 계정 검사
					//

					DataRow drAccount = GameDBDoc.Account(gameConn, gameTrans, m_accessToken.userId);

					if (drAccount != null)
					{
						Guid accountId = DBUtil.ToGuid(drAccount["accountId"]);

						m_account = new Account(clientPeer);
						m_account.Init(drAccount);
					}
					else
					{
						m_account = new Account(clientPeer);
						m_account.Init(m_accessToken.userId, m_currentTime);

						//
						// 계정 등록
						//

						if (GameDBDocEx.AddAccount(gameConn, gameTrans, m_account.id, m_account.userId, m_account.regTime) != 0)
							throw new CommandHandleException(kResult_Error, "계정 등록에 실패 했습니다.");
					}
				}

				//
				//
				//

				Util.Close(ref userConn);

				Util.Commit(ref gameTrans);
				Util.Close(ref gameConn);
			}
			finally
			{
				if (userConn != null)
					Util.Close(ref userConn);

				if (gameConn != null)
				{
					if (gameTrans != null)
						Util.Rollback(ref gameTrans);

					Util.Close(ref gameConn);
				}
			}
		}

		protected override void OnWorkSuccess()
		{
			base.OnWorkSuccess();

			CheckDuplicated();
		}

		private void CheckDuplicated()
		{
			Account loginedAccount = Cache.instance.GetAccount(m_account.id);

			if (loginedAccount != null)
			{
				//
				// 서버 이벤트
				//

				ServerEvent.SendLoginDuplicatedEvent(loginedAccount.clientPeer);

				//
				// 로그아웃 처리
				//

				loginedAccount.Logout();
			}

			//
			//
			//

			ProcessCompleted();
		}

		private void ProcessCompleted()
		{
			Cache.instance.AddAccount(m_account);

			clientPeer.account = m_account;

			//
			// DB 저장
			//

			SaveToDB_User();

			//
			// 응답
			//

			SendResponseOK(null);
		}

		private void SaveToDB_User()
		{
			SFSqlWork dbWork = SqlWorkUtil.CreateUserDBWork(m_account.userId);

			//
			// 사용자 수정(로그인)
			//

			dbWork.AddCommand(UserDBDoc.CSC_UpdateUser_Login(m_account.userId, Server.instance.currentGameServer.id, clientPeer.ipAddress));

			//
			//
			//

			dbWork.Schedule();
		}
	}
}
