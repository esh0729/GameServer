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
			AddCommandHandler<LoginCommandHandler>(CommandName.Login);
		}
	}
}
