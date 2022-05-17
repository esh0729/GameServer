﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace GameServer
{
	public class CharacterAction
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public Character m_character = null;
		private int m_nId = 0;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public CharacterAction(Character character)
		{
			m_character = character;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public Character character
		{
			get { return m_character; }
		}

		public int id
		{
			get { return m_nId; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Set(DataRow dr)
		{
			if (dr == null)
				throw new ArgumentNullException("dr");

			m_nId = Convert.ToInt32(dr["actionId"]);
		}
	}
}