using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ServerFramework;

namespace GameServer
{
	public abstract class Handler : SFHandler
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const short kResult_OK		= 0;
		public const short kResult_Error	= 1;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public ClientPeer clientPeer
		{
			get { return (ClientPeer)m_peer; }
		}

		protected virtual bool globalLockRequired
		{
			get { return false; }
		}

		protected virtual bool isValid
		{
			get { return !m_peer.disposed; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Run()
		{
			ClientPeerSynchronizer synchronizer = new ClientPeerSynchronizer(clientPeer, new SFAction(OnHandle));

			if (globalLockRequired)
			{
				lock (Cache.instance.syncObject)
				{
					synchronizer.Start();
				}
			}
			else
				synchronizer.Start();
		}
	}
}
