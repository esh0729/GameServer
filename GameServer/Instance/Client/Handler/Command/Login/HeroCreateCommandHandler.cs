using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

using ClientCommon;

namespace GameServer
{
	public class HeroCreateCommandHandler : LoginRequiredCommandHandler<HeroCreateCommandBody,HeroCreateResponseBody>
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const short kResult_ExistHeroName = 101;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private DateTimeOffset m_currentTime = DateTimeUtil.currentTime;

		private Guid m_heroId = Guid.Empty;
		private string m_sName = null;
		private Character m_character = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnLoginRequiredCommandHandle()
		{
			m_currentTime = DateTimeUtil.currentTime;

			if (m_body == null)
				throw new CommandHandleException(kResult_Error, "body가 null입니다.");

			m_sName = m_body.name;

			if (m_sName == null)
				throw new CommandHandleException(kResult_Error, "영웅 이름이 존재하지 않습니다.");

			if (!Server.instance.currentGameServer.IsMatchHeroNameRegex(m_sName))
				throw new CommandHandleException(kResult_Error, "영웅 이름이 유효하지 않습니다.");

			int nCharacterId = m_body.characterId;

			if (nCharacterId <= 0)
				throw new CommandHandleException(kResult_Error, "캐릭터ID가 유효하지 않습니다. nCharacterId = " + nCharacterId);

			m_character = Resource.instance.GetCharacter(nCharacterId);

			if (m_character == null)
				throw new CommandHandleException(kResult_Error, "캐릭터가 존재하지 않습니다.");

			RunnableStandaloneWork(Process);
		}

		private void Process()
		{
			SqlConnection userConn = null;
			SqlTransaction userTrans = null;

			SqlConnection gameConn = null;
			SqlTransaction gameTrans = null;

			try
			{
				//
				// UserDB
				//

				userConn = Util.OpenUserDBConnection();
				userTrans = userConn.BeginTransaction();

				if (UserDBDoc.HeroName(userConn, userTrans, m_sName) != null)
					throw new CommandHandleException(kResult_ExistHeroName, "이미 존재하는 영웅이름입니다. m_sName = " + m_sName);

				m_heroId = Guid.NewGuid();

				if (UserDBDocEx.AddUserHero(userConn, userTrans, m_myAccount.userId, m_heroId, m_sName, m_character.id) != 0)
					throw new CommandHandleException(kResult_Error, "사용자영웅 등록 실패.");

				if (UserDBDocEx.AddHeroName(userConn, userTrans, m_sName, m_heroId) != 0)
					throw new CommandHandleException(kResult_Error, "영웅이름 등록 실패.");

				//
				// GameDB
				//

				gameConn = Util.OpenGameDBConnection();
				gameTrans = gameConn.BeginTransaction();

				if (GameDBDocEx.AddHero(gameConn, gameTrans, m_myAccount.id, m_heroId, m_sName, m_character.id, m_currentTime) != 0)
					throw new CommandHandleException(kResult_Error, "영웅 등록 실패.");

				//
				//
				//

				Util.Commit(ref userTrans);
				Util.Close(ref userConn);

				Util.Commit(ref gameTrans);
				Util.Close(ref gameConn);
			}
			finally
			{
				if (userConn != null)
				{
					if (userTrans != null)
						Util.Rollback(ref userTrans);

					Util.Close(ref userConn);
				}

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
		}

		private void ProcessCompleted()
		{
			//
			// 응답
			//

			SendResponseOK(null);
		}
	}
}
