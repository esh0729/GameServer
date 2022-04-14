using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ServerFramework;

namespace GameServer
{
	public class AccountSynchronizer : ClientPeerSynchronizer
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		protected Account m_account = null;
		protected bool m_bRequiredGlobalLock = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public AccountSynchronizer(Account account, ISFWork work, bool bRequiredGlobalLock)
			: base(account.clientPeer, work)
		{
			if (account == null)
				throw new ArgumentNullException("account");

			m_account = account;
			m_bRequiredGlobalLock = bRequiredGlobalLock;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Start()
		{
			if (m_bRequiredGlobalLock)
			{
				lock (Cache.instance.syncObject)
				{
					lock (syncObject)
					{
						m_work.Run();
					}
				}
			}
			else
			{
				lock (syncObject)
				{
					m_work.Run();
				}
			}
		}
	}
}
