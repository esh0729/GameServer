using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClientCommon;

namespace GameServer
{
	public class HeroInitEnterCommandHandler : InGameCommandHandler<HeroInitEnterCommandBody, HeroInitEnterResponseBody>
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		protected override bool globalLockRequired
		{
			get { return true; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnInGameCommandHandle()
		{
			DateTimeOffset currentTime = DateTimeUtil.currentTime;

			HeroInitEnterParam param = m_myHero.entranceParam as HeroInitEnterParam;

			if (param == null)
				throw new CommandHandleException(kResult_Error, "초기 입장을 할 수 없습니다.");

			Continent continent = param.continent;
			Vector3 position = param.position;
			float fYRotation = param.yRotation;

			ContinentInstance continentInstance = Cache.instance.GetContinentInstance(continent.id);
			if (continentInstance == null)
				throw new CommandHandleException(kResult_Error, "대륙인스턴스가 존재하지 않습니다. continentId = " + continent.id);

			//
			// 입장
			//

			lock (continentInstance.syncObject)
			{
				m_myHero.SetPosition(position, fYRotation);
				continentInstance.Enter(m_myHero);

				//
				// 응답
				//

				HeroInitEnterResponseBody resBody = new HeroInitEnterResponseBody();
				resBody.placeInstanceId = continentInstance.instanceId;

				resBody.position = m_myHero.position;
				resBody.yRotation = m_myHero.yRotation;

				resBody.heroes = Hero.ToPDHeroes(continentInstance.GetInterestedHeroes(m_myHero.sector, m_myHero.id)).ToArray();

				SendResponseOK(resBody);
			}
		}
	}
}
