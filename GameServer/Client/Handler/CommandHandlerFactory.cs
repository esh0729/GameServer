using System;

using ServerFramework;
using ClientCommon;

namespace GameServer
{
	public class CommandHandlerFactory : SFHandlerFactory
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		private void AddCommandHandler<T>(CommandName name) where T : SFHandler
		{
			AddHandler<T>((int)name);
		}

		public Type GetCommandHandler(int nName)
		{
			return GetHandler(nName);
		}

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
