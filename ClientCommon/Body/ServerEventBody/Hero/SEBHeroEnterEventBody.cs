﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientCommon
{
	public class SEBHeroEnterEventBody : ServerEventBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public PDHero hero;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public override void Serialize(PacketWriter writer)
		{
			base.Serialize(writer);

			writer.Write(hero);
		}

		public override void Deserialize(PacketReader reader)
		{
			base.Deserialize(reader);

			hero = reader.ReadPacketData<PDHero>();
		}
	}
}