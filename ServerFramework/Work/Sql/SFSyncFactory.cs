using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFramework
{
	public class SFSyncFactory
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		//
		// User
		//

		private static Dictionary<object, SFSync> m_sUserSyncFactories = new Dictionary<object, SFSync>();

		//
		// Hero
		//

		private static Dictionary<object, SFSync> m_sHeroSyncFactories = new Dictionary<object, SFSync>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static SFSync GetSync(SyncWorkType type, object id)
		{
			SFSync sync = null;

			switch (type)
			{
				case SyncWorkType.User: sync = GetOrCreateUserSync(id); break;
				case SyncWorkType.Hero: sync = GetOrCreateHeroSync(id); break;

				default:
					throw new Exception("Not exist type.");
			}

			return sync;
		}

		//
		// User
		//

		private static SFSync GetOrCreateUserSync(object id)
		{
			SFSync sync = null;

			if (!m_sUserSyncFactories.TryGetValue(id, out sync))
			{
				sync = new SFSync(id);
				m_sUserSyncFactories.Add(id, sync);
			}

			return sync;
		}

		//
		// Hero
		//

		private static SFSync GetOrCreateHeroSync(object id)
		{
			SFSync sync = null;

			if (!m_sHeroSyncFactories.TryGetValue(id, out sync))
			{
				sync = new SFSync(id);
				m_sHeroSyncFactories.Add(id, sync);
			}

			return sync;
		}
	}
}
