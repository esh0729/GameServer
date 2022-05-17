using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class DataQueue
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private object m_lockObject = new object();
		private Queue<Data> m_queue = new Queue<Data>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public Data GetData()
		{
			lock (m_lockObject)
			{
				if (m_queue.Count > 0)
					return m_queue.Dequeue();
				else
					return CreateData();
			}
		}

		public void ReturnData(Data packet)
		{
			if (packet == null)
				return;

			lock (m_lockObject)
			{
				m_queue.Enqueue(packet);
			}
		}

		private Data CreateData()
		{
			return new Data();
		}

		public void Clear()
		{
			lock (m_lockObject)
			{
				m_queue.Clear();
			}
		}
	}
}
