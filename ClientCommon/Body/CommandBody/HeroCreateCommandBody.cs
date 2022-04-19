using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientCommon
{
	public class HeroCreateCommandBody : CommandBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public string name;
		public int characterId;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Serialize(PacketWriter writer)
		{
			base.Serialize(writer);

			writer.Write(name);
			writer.Write(characterId);
		}

		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			name = reader.ReadString();
			characterId = reader.ReadInt32();
		}
	}

	public class HeroCreateResponseBody : ResponseBody
	{
	}
}
