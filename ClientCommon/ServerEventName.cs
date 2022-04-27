namespace ClientCommon
{
	public enum ServerEventName : int
	{
		//
		// 계정
		//

		LoginDuplicated,

		//
		// 영웅
		//

		HeroEnter,
		HeroExit,
		HeroMove,

		//
		// 관심영역
		//

		InterestedAreaChanged,
		HeroInterestedAreaEnter,
		HeroInterestedAreaExit
	}
}
