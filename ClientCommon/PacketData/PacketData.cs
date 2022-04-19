using System.IO;

namespace ClientCommon
{
	public abstract class PacketData
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public virtual void Serialize(PacketWriter writer)
		{
		}

		public virtual void Deserialize(PacketReader reader)
		{
		}
	}
}
