using System.IO;

namespace ClientCommon
{
	public class PDVector3 : PacketData
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public float x;
		public float y;
		public float z;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write(x);
			writer.Write(y);
			writer.Write(z);
		}

		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);

			x = reader.ReadSingle();
			y = reader.ReadSingle();
			z = reader.ReadSingle();
		}
	}
}
