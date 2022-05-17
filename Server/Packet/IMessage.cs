using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public interface IMessage
	{
		PacketType type { get; }

		byte[] GetBytes();
		void GetBytes(byte[] buffer, out long lnLength);
	}
}
