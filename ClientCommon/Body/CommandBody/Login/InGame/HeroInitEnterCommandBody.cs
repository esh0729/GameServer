using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommon
{
	public class HeroInitEnterCommandBody : CommandBody
	{
	}

	public class HeroInitEnterResponseBody : ResponseBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public Guid placeInstanceId;

		public PDVector3 position;
		public float yRotation;

		public PDHero[] heroes;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Serialize(PacketWriter writer)
		{
			base.Serialize(writer);

			writer.Write(placeInstanceId);

			writer.Write(position);
			writer.Write(yRotation);

			writer.Write(heroes);
		}

		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			placeInstanceId = reader.ReadGuid();

			position = reader.ReadPDVector3();
			yRotation = reader.ReadSingle();

			heroes = reader.ReadPacketDatas<PDHero>();
		}
	}
}
