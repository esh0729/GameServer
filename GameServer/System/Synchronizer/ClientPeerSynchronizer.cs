using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ServerFramework;

namespace GameServer
{
	public class ClientPeerSynchronizer : ISynchronizer
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		protected ClientPeer m_clientPeer = null;
		protected ISFWork m_work = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public object syncObject
		{
			get { return m_clientPeer.syncObject; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public ClientPeerSynchronizer(ClientPeer clientPeer, ISFWork work)
		{
			if (clientPeer == null)
				throw new ArgumentNullException("clientPeer");

			if (work == null)
				throw new ArgumentNullException("work");

			m_clientPeer = clientPeer;
			m_work = work;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public virtual void Start()
		{
			if (m_clientPeer.account != null)
			{
				AccountSynchronizer accountSynchronizer = new AccountSynchronizer(m_clientPeer.account, m_work, false);
				accountSynchronizer.Start();
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
