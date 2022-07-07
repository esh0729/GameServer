using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientCommon;

namespace GameServer
{
	//=====================================================================================================================
	// (LoginRequiredCommandHandler 상속) 캐릭터 행동 클라이언트 명령을 처리하는 클래스
	//=====================================================================================================================
	public class ActionCommandHandler : InGameCommandHandler<ActionCommandBody, ActionResponseBody>
	{
		//=====================================================================================================================
		// 캐릭터 행동 완료 이후 클라이언트 응답 전송을 처리하는 함수
		//=====================================================================================================================
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnInGameCommandHandle()
		{
			if (m_body == null)
				throw new CommandHandleException(kResult_Error, "body가 null입니다.");

			// 장소 검사
			PhysicalPlace currentPlace = m_myHero.currentPlace;

			if (currentPlace == null)
				throw new CommandHandleException(kResult_Error, "현재 장소에서 사용할 수 없는 명령입니다.");

			// 유효성 검사
			int nActionId = m_body.actionId;
			Vector3 position = m_body.position;
			float fYRotation = m_body.yRotation;

			if (nActionId == 0)
				throw new CommandHandleException(kResult_Error, "행동ID가 유효하지 않습니다. nActionId = " + nActionId);

			CharacterAction action = m_myHero.character.GetAction(nActionId);
			if (action == null)
				throw new CommandHandleException(kResult_Error, "행동이 존재하지 않습니다. nActionId = " + nActionId);
			
			if (!currentPlace.ContainsPosition(position))
				throw new CommandHandleException(kResult_Error, "행동위치가 유효하지 않습니다. position = " + position);

			// 이동 처리
			currentPlace.MoveHero(m_myHero, position, fYRotation, true);

			// 이동 중지
			m_myHero.EndMove();

			// 행동 시작
			m_myHero.StartAction(action);

			// 클라이언트 응답 전송
			SendResponseOK(null);
		}
	}
}
