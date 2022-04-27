using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
	public class ContinentInstance : PhysicalPlace
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Continent m_continent = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public override PlaceType type
		{
			get { return PlaceType.Continent; }
		}

		public override Location location
		{
			get { return m_continent; }
		}

		public override Rect3D rect
		{
			get { return m_continent.rect; }
		}

		public Continent continent
		{
			get { return m_continent; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Init(Continent continent)
		{
			if (continent == null)
				throw new ArgumentNullException("continent");

			m_continent = continent;

			//
			//
			//

			InitPhysicalPlace();
		}

		//
		//
		//

		protected override void OnHeroExit(Hero hero, bool bIsLogout, EntranceParam entranceParam)
		{
			base.OnHeroExit(hero, bIsLogout, entranceParam);

			if (!bIsLogout)
				hero.SetPreviousContinent();
		}
	}
}
