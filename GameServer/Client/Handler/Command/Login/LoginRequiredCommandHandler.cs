﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientCommon;

namespace GameServer
{
	public abstract class LoginRequiredCommandHandler<T1, T2> : CommandHandler<T1, T2>
		where T1 : CommandBody where T2 : ResponseBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		protected Account m_myAccount = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		protected override bool isValid
		{
			get { return base.isValid && m_myAccount.isLoggedIn; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnCommandHandle()
		{
			if (clientPeer.account == null)
				throw new CommandHandleException(kResult_Error, "로그인이 필요한 명령입니다.");

			m_myAccount = clientPeer.account;

			OnLoginRequiredCommandHandle();
		}

		protected abstract void OnLoginRequiredCommandHandle();

		protected override void ErrorFrom(StringBuilder sb)
		{
			base.ErrorFrom(sb);

			sb.Append("# AccoutId : ");
			sb.Append(m_myAccount.id);
			sb.AppendLine();
		}
	}
}
