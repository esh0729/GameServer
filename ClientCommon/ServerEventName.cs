namespace ClientCommon
{
	//=====================================================================================================================
	// 서버 이벤트(클라이언트의 요청 없이 서버에서 송신하는 메세지)의 세부 타입
	//=====================================================================================================================
	public enum ServerEventName : int
	{
		//
		// 계정
		//

		// 계정 로그인 중복
		LoginDuplicated,

		//
		// 영웅
		//

		// 다른 영웅 입장
		HeroEnter,
		// 다른 영웅 퇴장
		HeroExit,
		// 다른 영웅 이동
		HeroMove,
		// 다른 영웅 행동시작
		HeroActionStarted,

		//
		// 관심영역
		//

		// 관심 영역 변경
		InterestedAreaChanged,
		// 다른 영웅 관심 영역 입장
		HeroInterestedAreaEnter,
		// 다른 영웅 관심 영역 퇴장
		HeroInterestedAreaExit
	}
}
