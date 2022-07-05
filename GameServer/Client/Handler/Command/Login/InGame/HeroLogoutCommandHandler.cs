using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClientCommon;

namespace GameServer
{
	//=====================================================================================================================
	// (InGameCommandHandler 상속) 영웅 로그아웃에 대한 클라이언트 명령을 처리하는 클래스
	//=====================================================================================================================
	public class HeroLogoutCommandHandler : InGameCommandHandler<HeroLogoutCommandBody, HeroLogoutResponseBody>
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 영웅 객체를 Cache에서 삭제하기 때문에 Cache lock 처리 프로퍼티 true로 오버라이딩
		protected override bool globalLockRequired
		{
			get { return true; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 영웅 로그아웃 완료 이후 클라이언트 응답 전송을 처리하는 함수
		//=====================================================================================================================
		protected override void OnInGameCommandHandle()
		{
			// 로그아웃 처리 함수 호출
			m_myHero.Logout();

			// 클라이언트 응답 전송
			SendResponseOK(null);
		}
	}
}
