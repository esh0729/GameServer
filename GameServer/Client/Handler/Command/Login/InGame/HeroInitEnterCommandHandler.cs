using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClientCommon;

namespace GameServer
{
	//=====================================================================================================================
	// (InGameCommandHandler 상속) 영웅 로그인 이후 초기 입장에 대한 클라이언트 명령을 처리하는 클래스
	//=====================================================================================================================
	public class HeroInitEnterCommandHandler : InGameCommandHandler<HeroInitEnterCommandBody, HeroInitEnterResponseBody>
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 영웅 객체를 장소 객체에서 추가 하기 때문에 상위 lock 객체인 Cache lock 처리 프로퍼티 true로 오버라이딩
		protected override bool globalLockRequired
		{
			get { return true; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 영웅 초기 입장 이후 클라이언트 응답 전송을 처리하는 함수
		//=====================================================================================================================
		protected override void OnInGameCommandHandle()
		{
			// 현재 시각
			DateTimeOffset currentTime = DateTimeUtil.currentTime;

			// 영웅 로그인때 처리 되었던 초기 입장 정보 확인
			HeroInitEnterParam param = m_myHero.entranceParam as HeroInitEnterParam;

			if (param == null)
				throw new CommandHandleException(kResult_Error, "초기 입장을 할 수 없습니다.");

			Continent continent = param.continent;
			Vector3 position = param.position;
			float fYRotation = param.yRotation;

			ContinentInstance continentInstance = Cache.instance.GetContinentInstance(continent.id);
			if (continentInstance == null)
				throw new CommandHandleException(kResult_Error, "대륙인스턴스가 존재하지 않습니다. continentId = " + continent.id);

			// 입장 처리하기 위해 해당 장소에 lock 처리
			lock (continentInstance.syncObject)
			{
				// 위치 및 방향 수정 후 입장 처리
				m_myHero.SetPosition(position, fYRotation);
				continentInstance.Enter(m_myHero);

				// 응답 객체 생성
				HeroInitEnterResponseBody resBody = new HeroInitEnterResponseBody();
				resBody.placeInstanceId = continentInstance.instanceId;

				resBody.position = m_myHero.position;
				resBody.yRotation = m_myHero.yRotation;

				resBody.heroes = Hero.ToPDHeroes(continentInstance.GetInterestedHeroes(m_myHero.sector, m_myHero.id)).ToArray();

				// 클라이언트 응답 전송 
				SendResponseOK(resBody);
			}
		}
	}
}
