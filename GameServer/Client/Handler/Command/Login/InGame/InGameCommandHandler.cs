using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClientCommon;

namespace GameServer
{
	public abstract class InGameCommandHandler<T1,T2> : LoginRequiredCommandHandler<T1,T2>
		where T1 : CommandBody where T2 : ResponseBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		protected Hero m_myHero = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		protected override bool isValid
		{
			get { return base.isValid && m_myHero.isLoggedIn; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnLoginRequiredCommandHandle()
		{
			if (m_myAccount.currentHero == null)
				throw new CommandHandleException(kResult_Error, "영웅로그인이 필요한 명령입니다.");

			m_myHero = m_myAccount.currentHero;

			OnInGameCommandHandle();
		}

		protected abstract void OnInGameCommandHandle();
	}
}
