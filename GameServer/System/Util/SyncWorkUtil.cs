using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ServerFramework;

namespace GameServer
{
	public class SyncWorkUtil
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static SFSyncWork CreateUserSyncWork(Guid userId)
		{
			SFSyncWork syncWork = new SFSyncWork(userId, SyncWorkType.User);
			syncWork.Init();

			return syncWork;
		}

		public static SFSyncWork CreateHeroSyncWork(Guid heroId)
		{
			SFSyncWork syncWork = new SFSyncWork(heroId, SyncWorkType.Hero);
			syncWork.Init();

			return syncWork;
		}
	}
}
