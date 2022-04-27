using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClientCommon;

namespace GameServer
{
	public class HeroLogoutCommandHandler : InGameCommandHandler<HeroLogoutCommandBody, HeroLogoutResponseBody>
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		protected override bool globalLockRequired
		{
			get { return true; }
		}

		protected override void OnInGameCommandHandle()
		{
			m_myHero.Logout();

			//
			// 응답
			//

			SendResponseOK(null);
		}
	}
}
