using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
	public abstract class Unit
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		protected PhysicalPlace m_currentPlace = null;
		protected Vector3 m_position = Vector3.zero;
		protected float m_fYRotation = 0f;

		protected Sector m_sector = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public PhysicalPlace currentPlace
		{
			get { return m_currentPlace; }
		}

		public Vector3 position
		{
			get { return m_position; }
		}

		public float YRotation
		{
			get { return m_fYRotation; }
		}

		public virtual float moveSpeed
		{
			get { return 0f; }
		}

		public Sector sector
		{
			get { return m_sector; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void SetCurrentPlace(PhysicalPlace place)
		{
			m_currentPlace = place;
		}

		public void SetPosition(Vector3 position, float fYRotation)
		{
			m_position = position;
			m_fYRotation = fYRotation;
		}

		public void SetSector(Sector sector)
		{
			m_sector = sector;
		}
	}
}
