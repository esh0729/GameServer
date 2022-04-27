using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClientCommon;

namespace GameServer
{
	public class HeroMoveEndCommandHandler : InGameCommandHandler<HeroMoveEndCommandBody, HeroMoveEndResponseBody>
	{
		protected override void OnInGameCommandHandle()
		{
			PhysicalPlace currentPlace = m_myHero.currentPlace;

			if (currentPlace == null)
				throw new CommandHandleException(kResult_Error, "현재 장소에서 사용할 수 없는 명령입니다.");

			if (m_body == null)
				throw new CommandHandleException(kResult_Error, "body가 null입니다.");

			Guid placeInstanceId = m_body.placeInstanceId;
			Vector3 position = m_body.position;
			float fYRotation = m_body.yRotation;

			if (placeInstanceId == Guid.Empty)
				throw new CommandHandleException(kResult_Error, "장소ID가 유효하지 않습니다.");

			if (placeInstanceId != currentPlace.instanceId)
				throw new CommandHandleException(kResult_Error, "현재 장소에서 사용하지 않은 명령입니다.");

			if (!currentPlace.ContainsPosition(position))
				throw new CommandHandleException(kResult_Error, "이동위치가 유효하지 않습니다. position = " + position);

			if (!m_myHero.moving)
				throw new CommandHandleException(kResult_Error, "영웅이 이동중이 아닙니다.");

			//
			// 이동
			//

			Console.WriteLine("HeroMoveEndCommandHandler, position = " + position + ", fYRotation = " + fYRotation);

			InterestedAreaInfo info = currentPlace.MoveHero(m_myHero, position, fYRotation, false);

			//
			// 이동종료
			//

			m_myHero.EndMove();

			//
			// 응답
			//

			HeroMoveEndResponseBody resBody = new HeroMoveEndResponseBody();
			resBody.addedHeroes = Hero.ToPDHeroes(info.GetAddedSectorHeroes(m_myHero.id)).ToArray();

			resBody.removedHeroes = info.GetRemovedSectorHeroIds(m_myHero.id).ToArray();

			SendResponseOK(resBody);
		}
	}
}
