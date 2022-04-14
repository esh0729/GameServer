using System.IO;

namespace ClientCommon
{
	public abstract class PacketData
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public virtual void Serialize(BinaryWriter writer)
		{
		}

		public virtual void Deserialize(BinaryReader reader)
		{
		}
	}
}
