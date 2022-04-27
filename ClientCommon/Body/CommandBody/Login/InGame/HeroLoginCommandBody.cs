using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientCommon
{
	public class HeroLoginCommandBody : CommandBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public Guid heroId;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Serialize(PacketWriter writer)
		{
			base.Serialize(writer);

			writer.Write(heroId);
		}

		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			heroId = reader.ReadGuid();
		}
	}

	public class HeroLoginResponseBody : ResponseBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		//
		// 영웅 정보
		//

		public Guid heroId;
		public string name;
		public int characterId;

		//
		// 위치 정보
		//

		public int enterContinentId;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Serialize(PacketWriter writer)
		{
			base.Serialize(writer);

			//
			// 영웅 정보
			//

			writer.Write(heroId);
			writer.Write(name);
			writer.Write(characterId);

			//
			// 위치 정보
			//

			writer.Write(enterContinentId);
		}

		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			//
			// 영웅 정보
			//

			heroId = reader.ReadGuid();
			name = reader.ReadString();
			characterId = reader.ReadInt32();

			//
			// 위치 정보
			//

			enterContinentId = reader.ReadInt32();
		}
	}
}
