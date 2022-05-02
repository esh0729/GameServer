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

		//
		// Cache의 동기 실행이 필요할 경우 override 하여 true로 변경
		//

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
			//
			// 핸들러 실행 처리는 Receive에서 비동기 처리되어있으므로 Synchronizer에서 동기 처리 해줌
			//

			ClientPeerSynchronizer synchronizer = new ClientPeerSynchronizer(clientPeer, new SFAction(OnHandle));

			//
			// GameServer에 접속하여 로그인도 안된 상태의 경우 Account나 Hero가 없으므로 별도로 globalLockRequired 처리
			//

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
