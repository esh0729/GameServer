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
	public class HeroLoginCommandHandler : LoginRequiredCommandHandler<HeroLoginCommandBody, HeroLoginResponseBody>
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private DateTimeOffset m_currentTime = DateTimeOffset.MinValue;
		private Guid m_heroId = Guid.Empty;

		private DataRow m_drHero = null;
		private Hero m_hero = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		protected override bool globalLockRequired
		{
			get { return true; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnLoginRequiredCommandHandle()
		{
			m_currentTime = DateTimeUtil.currentTime;

			if (m_body == null)
				throw new CommandHandleException(kResult_Error, "body가 null입니다.");

			m_heroId = m_body.heroId;

			if (m_heroId == Guid.Empty)
				throw new CommandHandleException(kResult_Error, "영웅ID가 유효하지 않습니다.");

			RunnableStandaloneWork(Process);
		}

		private void Process()
		{
			SqlConnection conn = null;

			try
			{
				conn = Util.OpenGameDBConnection();

				//
				// 영웅
				//

				m_drHero = GameDBDoc.Hero(conn, null, m_heroId);

				if (m_drHero == null)
					throw new CommandHandleException(kResult_Error, "영웅이 존재하지 않습니다. m_heroId = " + m_heroId);

				Guid accountId = DBUtil.ToGuid(m_drHero["accountId"]);

				if (accountId != m_myAccount.id)
					throw new CommandHandleException(kResult_Error, "다른 계정의 영웅입니다. m_heroId = " + m_heroId);

				//
				//
				//

				Util.Close(ref conn);
			}
			finally
			{
				if (conn != null)
					Util.Close(ref conn);
			}
		}

		protected override void OnWorkSuccess()
		{
			base.OnWorkSuccess();

			ProcessCompleted();
		}

		private void ProcessCompleted()
		{
			m_hero = new Hero(m_myAccount);
			m_hero.CompleteLogin(m_currentTime, m_drHero);
			m_hero.isInitEntered = true;

			Cache.instance.AddHero(m_hero);

			//
			// DB 저장
			//

			SaveToDB_Game();

			//
			// 응답
			//

			HeroLoginResponseBody resBody = new HeroLoginResponseBody();

			//
			// 영웅 정보
			//

			resBody.heroId = m_hero.id;
			resBody.name = m_hero.name;
			resBody.characterId = m_hero.character.id;

			//
			// 위치 정보
			//

			HeroInitEnterParam param = (HeroInitEnterParam)m_hero.entranceParam;
			resBody.enterContinentId = param.continent.id;

			SendResponseOK(resBody);
		}

		private void SaveToDB_Game()
		{
			SFSqlWork dbWork = SqlWorkUtil.CreateHeroGameDBWork(m_hero.id);

			//
			// 영웅 로그인
			//

			dbWork.AddCommand(GameDBDoc.CSC_HeroLogin(m_hero.id, m_hero.lastLoginTime));

			//
			//
			//

			dbWork.Schedule();
		}
	}
}
