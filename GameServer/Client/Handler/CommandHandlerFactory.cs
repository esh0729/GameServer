using System;

using ServerFramework;
using ClientCommon;

namespace GameServer
{
	//=====================================================================================================================
	// (SFHandlerFactory 상속) 클라이언트 명령 핸들러의 객체 생성을 위한 명령 타입과 클래스 타입을 생성 및 관리하는 클래스
	//=====================================================================================================================
	public class CommandHandlerFactory : SFHandlerFactory
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 클라이언트 명령 핸들러 타입을 저장하는 함수
		//
		// nName : 클라이언트 명령 타입
		//=====================================================================================================================
		private void AddCommandHandler<T>(CommandName name) where T : SFHandler
		{
			// 클라이언트 명령 타입과 일반화된 핸들러 클래스 타입을 저장
			AddHandler<T>((int)name);
		}

		//=====================================================================================================================
		// 클라이언트 명령 핸들러 타입을 호출하는 함수
		// Return : 클라이언트 명령 타입에 매칭 되는 핸들러 클래스 타입
		//
		// nName : 클라이언트 명령 타입
		//=====================================================================================================================
		public Type GetCommandHandler(int nName)
		{
			return GetHandler(nName);
		}

		//=====================================================================================================================
		// 초기화 함수
		//=====================================================================================================================
		protected override void InitInternal()
		{
			//
			// 계정
			//

			AddCommandHandler<LoginCommandHandler>(CommandName.Login);
			AddCommandHandler<LobbyInfoCommandHandler>(CommandName.LobbyInfo);

			//
			// 영웅
			//

			AddCommandHandler<HeroCreateCommandHandler>(CommandName.HeroCreate);
			AddCommandHandler<HeroLoginCommandHandler>(CommandName.HeroLogin);
			AddCommandHandler<HeroLogoutCommandHandler>(CommandName.HeroLogout);
			AddCommandHandler<HeroInitEnterCommandHandler>(CommandName.HeroInitEnter);

			AddCommandHandler<HeroMoveStartCommandHandler>(CommandName.HeroMoveStart);
			AddCommandHandler<HeroMoveCommandHandler>(CommandName.HeroMove);
			AddCommandHandler<HeroMoveEndCommandHandler>(CommandName.HeroMoveEnd);

			//
			// 행동
			//

			AddCommandHandler<ActionCommandHandler>(CommandName.Action);
		}
	}
}
