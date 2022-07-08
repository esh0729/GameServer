using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClientCommon;

namespace GameServer
{
	//=====================================================================================================================
	// (LoginRequiredCommandHandler 상속) 영웅이동시작 클라이언트 명령을 처리하는 클래스
	//=====================================================================================================================
	public class HeroMoveStartCommandHandler : InGameCommandHandler<HeroMoveStartCommandBody, HeroMoveStartResponseBody>
	{
		//=====================================================================================================================
		// 영웅이동시작 완료 이후 클라이언트 응답 전송을 처리하는 함수
		//=====================================================================================================================
		protected override void OnInGameCommandHandle()
		{
			// 장소 검사
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

			// 이동 시작 함수 호출
			m_myHero.StartMove();

			// 클라이언트 응답 전송
			SendResponseOK(null);
		}
	}
}
