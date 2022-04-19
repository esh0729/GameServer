using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ServerFramework;

namespace GameServer
{
	public abstract class Place
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const short kUpdateTimeTicks = 500;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		protected Guid m_id = Guid.Empty;
		protected object m_syncObject = new object();

		protected SFWorker m_worker = null;
		protected Timer m_timer = null;

		private DateTimeOffset m_currentUpdateTime = DateTimeOffset.MinValue;
		private DateTimeOffset m_prevUpdateTime = DateTimeOffset.MinValue;

		protected Dictionary<Guid, Hero> m_heroes = new Dictionary<Guid, Hero>();

		protected bool m_bDisposed = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public Place()
		{
			m_id = Guid.NewGuid();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public object syncObject
		{
			get { return m_syncObject; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected void InitPlace()
		{
			m_worker = new SFWorker();
			m_worker.Start();

			m_timer = new Timer(Update, null, kUpdateTimeTicks, kUpdateTimeTicks);
		}

		private void Update(object state)
		{
			AddWork(new SFAction(OnUpdate), false);
		}

		private void OnUpdate()
		{
			try
			{
				OnUpdateInternal();
			}
			catch (Exception ex)
			{
				LogUtil.Error(GetType(), ex);
			}
		}

		protected virtual void OnUpdateInternal()
		{
			m_prevUpdateTime = m_currentUpdateTime;
			m_currentUpdateTime = DateTimeUtil.currentTime;

			if (m_prevUpdateTime == DateTimeOffset.MinValue)
				m_prevUpdateTime = m_currentUpdateTime;
		}

		//
		//
		//

		public void AddWork(ISFWork work, bool bRequiredGlobalLock)
		{
			m_worker.Add(new SFAction<ISFWork, bool>(RunWork, work, bRequiredGlobalLock));
		}

		private void RunWork(ISFWork work, bool bRequiredGlobalLock)
		{
			if (bRequiredGlobalLock)
			{
				lock (Cache.instance.syncObject)
				{
					lock (m_syncObject)
					{
						work.Run();
					}
				}
			}
			else
			{
				lock (m_syncObject)
				{
					work.Run();
				}
			}
		}

		//
		//
		//

		private void AddHero(Hero hero)
		{
			if (hero == null)
				throw new ArgumentNullException("hero");
		}

		private bool RemoveHero(Guid heroId)
		{
			return m_heroes.Remove(heroId);
		}

		public void Enter(Hero hero)
		{
			if (hero == null)
				throw new ArgumentNullException("hero");

			AddHero(hero);

			OnHeroEnter(hero);
		}

		protected virtual void OnHeroEnter(Hero hero)
		{
		}

		public void Exit(Hero hero)
		{
			if (hero == null)
				throw new ArgumentNullException("hero");

			if (!RemoveHero(hero.id))
				return;

			OnHeroExit(hero);
		}

		protected virtual void OnHeroExit(Hero hero)
		{
		}

		public List<ClientPeer> GetClientPeers(Guid heroIdToExclude)
		{
			List<ClientPeer> clientPeers = new List<ClientPeer>();

			foreach (Hero hero in m_heroes.Values)
			{
				if (hero.id == heroIdToExclude)
					continue;

				clientPeers.Add(hero.clientPeer);
			}

			return clientPeers;
		}

		//
		//
		//

		public void Dispose()
		{
			if (m_bDisposed)
				return;

			m_bDisposed = true;

			OnDisposed();
		}

		protected virtual void OnDisposed()
		{

		}
	}
}
