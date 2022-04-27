using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
	public class HeroInitEnterParam : EntranceParam
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Continent m_continent = null;
		private Vector3 m_position = Vector3.zero;
		private float m_fYRotation = 0f;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public HeroInitEnterParam(Continent continent, Vector3 position, float fYRotation)
		{
			if (continent == null)
				throw new ArgumentNullException("continent");

			m_continent = continent;
			m_position = position;
			m_fYRotation = fYRotation;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public Continent continent
		{ 
			get { return m_continent; }
		}

		public Vector3 position
		{
			get { return m_position; }
		}

		public float yRotation
		{
			get { return m_fYRotation; }
		}
	}
}
