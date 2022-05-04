using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class PacketQueue
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private object m_lockObject = new object();
		private Queue<FullPacket> m_queue = new Queue<FullPacket>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public FullPacket GetPacket()
		{
			lock (m_lockObject)
			{
				if (m_queue.Count > 0)
					return m_queue.Dequeue();
				else
					return CreatePacket();
			}
		}

		public void ReturnPacket(FullPacket packet)
		{
			if (packet == null)
				return;

			packet.Clear();

			lock (m_lockObject)
			{
				m_queue.Enqueue(packet);
			}
		}

		private FullPacket CreatePacket()
		{
			return new FullPacket();
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
