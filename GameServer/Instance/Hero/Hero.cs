using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;

using ClientCommon;
using ServerFramework;

namespace GameServer
{
	public class Hero : Unit
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const short kUpdateTimeTicks = 500;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Account m_account = null;
		private Guid m_id = Guid.Empty;
		private string m_sName = null;
		private Character m_character = null;

		private SFWorker m_worker = null;
		private Timer m_timer = null;

		private DateTimeOffset m_currentUpdateTime = DateTimeOffset.MinValue;
		private DateTimeOffset m_prevUpdateTime = DateTimeOffset.MinValue;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public Hero(Account account)
		{
			if (m_account == null)
				throw new ArgumentNullException("account");

			m_account = account;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public Account account
		{
			get { return m_account; }
		}

		public ClientPeer clientPeer
		{
			get { return m_account.clientPeer; }
		}

		public object syncObject
		{
			get { return m_account.syncObject; }
		}

		public Guid id
		{ 
			get { return m_id; }
		}

		public string name
		{
			get { return m_sName; }
		}

		public Character character
		{
			get { return m_character; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void CompleteLogin(DataRow dr)
		{
			if (dr == null)
				throw new ArgumentNullException("dr");

			m_id = DBUtil.ToGuid(dr["heroId"]);

			//
			//
			//

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

		private void OnUpdateInternal()
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
			HeroSynchronizer heroSynchronizer = new HeroSynchronizer(this, work, bRequiredGlobalLock);
			heroSynchronizer.Start();
		}

		//
		//
		//

		public void Logout()
		{
			m_timer.Dispose();

			//
			//
			//

			m_account.currentHero = null;

			//
			//
			//

			m_worker.Stop();
		}

		//
		//
		//

		public PDHero ToPDHero()
		{
			PDHero inst = new PDHero();
			inst.heroId = m_id;
			inst.name = m_sName;
			inst.characterId = m_character.id;
			inst.position = m_position;
			inst.yRotation = m_fYRotation;

			return inst;
		}
	}
}
