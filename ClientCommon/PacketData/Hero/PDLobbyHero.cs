using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommon
{
	public class PDLobbyHero : PacketData
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public Guid heroId;
		public string name;
		public int characterId;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Serialize(PacketWriter writer)
		{
			base.Serialize(writer);

			writer.Write(heroId);
			writer.Write(name);
			writer.Write(characterId);
		}

		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			heroId = reader.ReadGuid();
			name = reader.ReadString();
			characterId = reader.ReadInt32();
		}
	}
}
