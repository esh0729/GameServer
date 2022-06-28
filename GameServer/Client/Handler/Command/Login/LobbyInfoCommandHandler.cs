using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ClientCommon;
using ServerFramework;

namespace GameServer
{
	//=====================================================================================================================
	// (LoginRequiredCommandHandler 상속) 계정 영웅 정보 호출 대한 클라이언트 명령을 처리하는 클래스
	//=====================================================================================================================
	public class LobbyInfoCommandHandler : LoginRequiredCommandHandler<LobbyInfoCommandBody, LobbyInfoResponseBody>
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 생성된 영웅 컬렉션
		private List<PDLobbyHero> m_heroes = new List<PDLobbyHero>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 유효성 검사 이후 계정에 생성된 영웅 조회 대한 비동기 작업을 요청하는 함수
		//=====================================================================================================================
		protected override void OnLoginRequiredCommandHandle()
		{
			// 유효성 검사
			if (m_myAccount.isHeroLoggedIn)
				throw new CommandHandleException(kResult_Error, "현재 상태에서 사용할 수 없는 명령입니다.");

			// 계정에 생성된 영웅 조회 대한 비동기 작업 요청
			RunnableStandaloneWork(Process);
		}

		//=====================================================================================================================
		// 계정에 생성된 영웅 조회 비동기 작업 함수
		//=====================================================================================================================
		private void Process()
		{
			// Game DB 연결 객체
			SqlConnection conn = null;

			try
			{
				// GameDB 연결 객체 생성
				conn = Util.OpenGameDBConnection();

				// 영웅 목록 조회
				DataRowCollection drcHeroes = GameDBDoc.Heroes(conn, null, m_myAccount.id);

				foreach (DataRow drHero in drcHeroes)
				{
					PDLobbyHero hero = new PDLobbyHero();
					hero.heroId = DBUtil.ToGuid(drHero["heroId"]);
					hero.name = Convert.ToString(drHero["name"]);
					hero.characterId = Convert.ToInt32(drHero["characterId"]);

					m_heroes.Add(hero);
				}

				//
				//
				//

				// 데이터베이스 연결 닫기
				Util.Close(ref conn);
			}
			finally
			{
				// 중간에 에러가 발생하여 데이터베이스 종료 처리가 되지 않았을 경우 연결 닫기
				if (conn != null)
					Util.Close(ref conn);
			}
		}

		//=====================================================================================================================
		// 비동기 작업이 완료됬을 경우 호출 되는 함수
		//=====================================================================================================================
		protected override void OnWorkSuccess()
		{
			base.OnWorkSuccess();

			// 작업 완료 처리 함수 호출
			ProcessCompleted();
		}

		//=====================================================================================================================
		// 응답 전송을 처리하는 함수
		//=====================================================================================================================
		private void ProcessCompleted()
		{
			// 응답 전송
			LobbyInfoResponseBody resBody = new LobbyInfoResponseBody();
			resBody.heroes = m_heroes.ToArray();

			SendResponseOK(resBody);
		}
	}
}
