using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommon
{
	public class LobbyInfoCommandBody : CommandBody
	{
	}

	public class LobbyInfoResponseBody : ResponseBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public PDLobbyHero[] heroes;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Serialize(PacketWriter writer)
		{
			base.Serialize(writer);

			writer.Write(heroes);
		}

		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			heroes = reader.ReadPacketDatas<PDLobbyHero>();
		}
	}
}
