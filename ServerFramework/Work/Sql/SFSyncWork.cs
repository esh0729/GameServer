using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ServerFramework
{
	public class SFSyncWork
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private object m_id = null;
		private SyncWorkType m_type = default(SyncWorkType);

		private SFSync m_sync = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public SFSyncWork(object id, SyncWorkType type)
		{
			m_id = id;
			m_type = type;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Init()
		{
			m_sync = SFSyncFactory.GetSync(m_type, m_id);
		}

		public void Waiting()
		{
			m_sync.Waiting();
			m_sync.Reset();
		}

		public void Set()
		{
			m_sync.Set();
		}
	}
}
