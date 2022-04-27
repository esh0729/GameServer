using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ServerFramework;

namespace GameServer
{
	public class SqlWorkUtil
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public static SFSqlWork CreateUserDBWork(Guid userId)
		{
			SFSqlWork dbWork = new SFSqlWork(Util.OpenUserDBConnection());
			dbWork.AddSyncWork(SyncWorkUtil.CreateUserSyncWork(userId));

			return dbWork;
		}

		public static SFSqlWork CreateHeroGameDBWork(Guid heroId)
		{
			SFSqlWork dbWork = new SFSqlWork(Util.OpenGameDBConnection());
			dbWork.AddSyncWork(SyncWorkUtil.CreateHeroSyncWork(heroId));

			return dbWork;
		}
	}
}
