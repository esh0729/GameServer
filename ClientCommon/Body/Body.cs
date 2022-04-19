using System.IO;

namespace ClientCommon
{
	public abstract class Body
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public byte[] SerializeRaw()
		{
			MemoryStream stream = new MemoryStream();
			PacketWriter writer = new PacketWriter(stream);

			Serialize(writer);

			return stream.ToArray();
		}

		public virtual void Serialize(PacketWriter writer)
		{
		}

		public void DeserializeRaw(byte[] bytes)
		{
			MemoryStream stream = new MemoryStream(bytes);
			PacketReader reader = new PacketReader(stream);

			Deserialize(reader);
		}

		public virtual void Deserialize(PacketReader reader)
		{
		}
	}
}
