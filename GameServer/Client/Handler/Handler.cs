using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ServerFramework;

namespace GameServer
{
	//=====================================================================================================================
	// (SFHandler 상속) 클라이언트 요청에 대한 작업을 동기처리 하여 시작하는 추상 클래스
	//=====================================================================================================================
	public abstract class Handler : SFHandler
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		// 클라이언트 작업 요청에 대한 결과 상수
		public const short kResult_OK		= 0;
		public const short kResult_Error	= 1;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 클라이언트 피어
		public ClientPeer clientPeer
		{
			get { return (ClientPeer)m_peer; }
		}

		//
		// Cache의 동기 실행이 필요할 경우 override 하여 true로 변경
		//

		// Cache lock 필요 여부
		protected virtual bool globalLockRequired
		{
			get { return false; }
		}

		// 핸들러의 실행 가능 여부
		protected virtual bool isValid
		{
			get { return !m_peer.disposed; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 필요한 동기 처리를 하여 클라이언트 요청을 시작하는 함수
		//=====================================================================================================================
		public override void Run()
		{
			// 핸들러 실행 처리는 Receive에서 비동기 처리되어있으므로 Synchronizer에서 동기 처리 해줌
			ClientPeerSynchronizer synchronizer = new ClientPeerSynchronizer(clientPeer, new SFAction(OnHandle));

			// GameServer에 접속하여 로그인도 안된 상태의 경우 Account나 Hero가 없으므로 별도로 globalLockRequired 처리하여 작업 시작
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
