using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ServerFramework;

namespace GameServer
{
	public class HeroSynchronizer : AccountSynchronizer
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		protected Hero m_hero = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public HeroSynchronizer(Hero hero, ISFWork work, bool bRequiredGlobalLock)
			: base(hero.account, work, bRequiredGlobalLock)
		{
			if (hero == null)
				throw new ArgumentNullException("hero");

			m_hero = hero;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//
		// 영웅은 현재 장소가 있을경우 현재 장소 Lock 처리후 진행
		//

		public override void Start()
		{
			if (m_bRequiredGlobalLock)
			{
				lock (Cache.instance.syncObject)
				{
					Place place = m_hero.currentPlace;

					if (place != null)
					{
						lock (place.syncObject)
						{
							RunWork();
						}
					}
					else
						RunWork();
				}
			}
			else
			{
				Place place = m_hero.currentPlace;

				if (place != null)
				{
					lock (place.syncObject)
					{
						RunWork();
					}
				}
				else
					RunWork();
			}
		}
	}
}
