using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommon
{
	public class SEBInterestedAreaChangedEventBody : ServerEventBody
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
