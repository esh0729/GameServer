using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClientCommon;

namespace GameServer
{
	public class HeroMoveStartCommandHandler : InGameCommandHandler<HeroMoveStartCommandBody, HeroMoveStartResponseBody>
	{
		protected override void OnInGameCommandHandle()
		{
			PhysicalPlace currentPlace = m_myHero.currentPlace;

			if (currentPlace == null)
				throw new CommandHandleException(kResult_Error, "현재 장소에서 사용할 수 없는 명령입니다.");

			if (m_body == null)
				throw new CommandHandleException(kResult_Error, "body가 null입니다.");

			Guid placeInstanceId = m_body.placeInstanceId;

			if (placeInstanceId == Guid.Empty)
				throw new CommandHandleException(kResult_Error, "장소ID가 유효하지 않습니다.");

			if (placeInstanceId != currentPlace.instanceId)
				throw new CommandHandleException(kResult_Error, "현재 장소에서 사용하지 않은 명령입니다.");

			//
			// 이동 시작
			//

			m_myHero.StartMove();

			//
			// 응답
			//

			SendResponseOK(null);
		}
	}
}
