using System;

using Server;
using ServerFramework;

namespace GameServer
{
	public class ClientPeer : PeerImpl
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private object m_syncObject = new object();

		private Account m_account = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public ClientPeer(PeerInit peerInit)
			: base(peerInit)
		{
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public object syncObject
		{
			get { return m_syncObject; }
		}

		public Account account
		{ 
			get { return m_account; }
			set { m_account = value; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnDisconnect(string sDisconnectType)
		{
			LogUtil.Error(GetType(), sDisconnectType);

			if (m_account != null)
			{
				AccountSynchronizer synchronizer = new AccountSynchronizer(m_account, new SFAction(m_account.Logout), true);
				synchronizer.Start();
			}

			Server.instance.RemoveClientPeer(this);
		}
	}
}