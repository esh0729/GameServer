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

		private bool m_bDisposed = false;

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

		public bool disposed
		{
			get { return m_bDisposed; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnDisconnect()
		{
			if (m_bDisposed)
				return;

			m_bDisposed = true;

			if (m_account != null)
			{
				AccountSynchronizer synchronizer = new AccountSynchronizer(m_account, new SFAction(m_account.Logout), true);
				synchronizer.Start();
			}

			Server.instance.RemovePeer(m_id);
		}
	}
}