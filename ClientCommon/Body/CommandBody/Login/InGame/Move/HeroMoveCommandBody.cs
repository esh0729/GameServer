using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientCommon
{
	public class HeroMoveCommandBody : CommandBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public Guid placeInstanceId;
		public PDVector3 position;
		public float yRotation;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Serialize(PacketWriter writer)
		{
			base.Serialize(writer);

			writer.Write(placeInstanceId);
			writer.Write(position);
			writer.Write(yRotation);
		}

		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			placeInstanceId = reader.ReadGuid();
			position = reader.ReadPDVector3();
			yRotation = reader.ReadSingle();
		}
	}

	public class HeroMoveResponseBody : ResponseBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public PDHero[] addedHeroes;

		public Guid[] removedHeroes;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Serialize(PacketWriter writer)
		{
			base.Serialize(writer);

			writer.Write(addedHeroes);

			writer.Write(removedHeroes);
		}

		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			addedHeroes = reader.ReadPacketDatas<PDHero>();

			removedHeroes = reader.ReadGuids();
		}
	}
}
