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
	//=====================================================================================================================
	// (LoginRequiredCommandHandler 상속) 영웅 로그인에 대한 클라이언트 명령을 처리하는 클래스
	//=====================================================================================================================
	public class HeroLoginCommandHandler : LoginRequiredCommandHandler<HeroLoginCommandBody, HeroLoginResponseBody>
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 현재 시각
		private DateTimeOffset m_currentTime = DateTimeOffset.MinValue;
		// 영웅 ID
		private Guid m_heroId = Guid.Empty;

		// 영웅 정보가 담긴 데이터베이스 Row
		private DataRow m_drHero = null;
		// 영웅 객체
		private Hero m_hero = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 영웅 객체를 생성하여 Cache에 등록하기 때문에 Cache lock 처리 프로퍼티 true로 오버라이딩
		protected override bool globalLockRequired
		{
			get { return true; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 유효성 검사 이후 영웅 조회에 대한 비동기 작업을 요청하는 함수
		//=====================================================================================================================
		protected override void OnLoginRequiredCommandHandle()
		{
			// 현재 시각 등록
			m_currentTime = DateTimeUtil.currentTime;

			// 클라이언트가 보내온 데이터의 유효성 검사

			if (m_body == null)
				throw new CommandHandleException(kResult_Error, "body가 null입니다.");

			m_heroId = m_body.heroId;

			if (m_heroId == Guid.Empty)
				throw new CommandHandleException(kResult_Error, "영웅ID가 유효하지 않습니다.");

			// 영웅 조회에 대한 비동기 작업 요청
			RunnableStandaloneWork(Process);
		}

		//=====================================================================================================================
		// 영웅 조회에 대한 비동기 작업 함수
		//=====================================================================================================================
		private void Process()
		{
			// Game DB에 대한 연결 객체
			SqlConnection conn = null;

			try
			{
				// GameDB 연결 객체 생성
				conn = Util.OpenGameDBConnection();

				// 영웅 유효성 검사

				m_drHero = GameDBDoc.Hero(conn, null, m_heroId);

				if (m_drHero == null)
					throw new CommandHandleException(kResult_Error, "영웅이 존재하지 않습니다. m_heroId = " + m_heroId);

				Guid accountId = DBUtil.ToGuid(m_drHero["accountId"]);

				if (accountId != m_myAccount.id)
					throw new CommandHandleException(kResult_Error, "다른 계정의 영웅입니다. m_heroId = " + m_heroId);

				//
				//
				//

				// 데이터베이스 연결 닫기
				Util.Close(ref conn);
			}
			finally
			{
				// 중간에 에러가 발생하여 데이터베이스 종료 처리가 되지 않았을 경우 연결 종료 처리
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
		// 영웅 로그인 완료 이후 데이터 저장 및 클라이언트 응답 전송을 처리하는 함수
		//=====================================================================================================================
		private void ProcessCompleted()
		{
			// 계정 객체에 영웅 로그인 처리
			m_hero = new Hero(m_myAccount);
			m_hero.CompleteLogin(m_currentTime, m_drHero);
			m_hero.isInitEntered = true;

			// Cache에 영웅 객체 저장
			Cache.instance.AddHero(m_hero);

			// DB 저장
			SaveToDB_Game();

			// 응답객체 생성
			HeroLoginResponseBody resBody = new HeroLoginResponseBody();

			// 영웅 정보
			resBody.heroId = m_hero.id;
			resBody.name = m_hero.name;
			resBody.characterId = m_hero.character.id;

			// 위치 정보
			HeroInitEnterParam param = (HeroInitEnterParam)m_hero.entranceParam;
			resBody.enterContinentId = param.continent.id;

			// 클라이언트 응답 전송
			SendResponseOK(resBody);
		}

		//=====================================================================================================================
		// 영웅 로그인시 등록되는 데이터 DB에 저장하는 함수
		//=====================================================================================================================
		private void SaveToDB_Game()
		{
			// heroId로 데이터베이스 작업 실행 객체 생성
			SFSqlWork dbWork = SqlWorkUtil.CreateHeroGameDBWork(m_hero.id);

			// 사용자 수정(영웅 로그인)에 대한 Sql명령 생성
			dbWork.AddCommand(GameDBDoc.CSC_HeroLogin(m_hero.id, m_hero.lastLoginTime));

			// DB 작업 실행 요청
			dbWork.Schedule();
		}
	}
}
