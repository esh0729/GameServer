using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace GameServer
{
	public class Character
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private int m_nId = 0;

		private float m_fMoveSpeed = 0f;

		//
		// 행동
		//

		private Dictionary<int,CharacterAction> m_actions = new Dictionary<int, CharacterAction>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public int id
		{
			get { return m_nId; }
		}

		public float moveSpeed
		{
			get { return m_fMoveSpeed; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Set(DataRow dr)
		{
			if (dr == null)
				throw new ArgumentNullException("dr");

			m_nId = Convert.ToInt32(dr["characterId"]);

			m_fMoveSpeed = Convert.ToSingle(dr["moveSpeed"]);
		}

		//
		// 행동
		//

		public void AddAction(CharacterAction action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			m_actions.Add(action.id, action);
		}

		public CharacterAction GetAction(int nId)
		{
			CharacterAction value;

			return m_actions.TryGetValue(nId, out value) ? value : null;
		}
	}
}
