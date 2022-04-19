using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientCommon
{
	public class PDHero : PacketData
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public Guid heroId;
		public string name;
		public int characterId;
		public PDVector3 position;
		public float yRotation;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Serialize(PacketWriter writer)
		{
			base.Serialize(writer);

			writer.Write(heroId);
			writer.Write(name);
			writer.Write(characterId);
			writer.Write(position);
			writer.Write(yRotation);
		}

		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			heroId = reader.ReadGuid();
			name = reader.ReadString();
			characterId = reader.ReadInt32();
			position = reader.ReadPDVector3();
			yRotation = reader.ReadSingle();
		}
	}
}
