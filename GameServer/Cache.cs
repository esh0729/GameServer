using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ServerFramework;

namespace GameServer
{
	public class Cache
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const short kUpdateTimeTicks = 500;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private object m_syncObject = new object();

		private SFWorker m_worker = null;
		private Timer m_timer = null;

		private DateTimeOffset m_currentUpdateTime = DateTimeOffset.MinValue;
		private DateTimeOffset m_prevUpdateTime = DateTimeOffset.MinValue;

		//
		// 계정
		//

		private Dictionary<Guid, Account> m_accounts = new Dictionary<Guid, Account>();

		//
		// 장소
		//

		private Dictionary<Guid, Place> m_places = new Dictionary<Guid, Place>();

		//
		// 대륙
		//

		//
		//
		//

		private bool m_disposed = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		private Cache()
		{

		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public object syncObject
		{
			get { return m_syncObject; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Init()
		{
			LogUtil.Info(GetType(), "Cache Init Started.");

			m_worker = new SFWorker();
			m_worker.Start();

			m_timer = new Timer(Update, null, kUpdateTimeTicks, kUpdateTimeTicks);

			LogUtil.Info(GetType(), "Cache Init Completed.");
		}

		private void Update(object state)
		{
			AddWork(new SFAction(Onupdate));
		}

		private void Onupdate()
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

		public void AddWork(ISFWork work)
		{
			m_worker.Add(new SFAction<ISFWork>(RunWork, work));
		}

		private void RunWork(ISFWork work)
		{
			lock (m_syncObject)
			{
				work.Run();
			}
		}

		//
		// 계정
		//

		public void AddAccount(Account account)
		{
			if (account == null)
				throw new ArgumentNullException("account");

			m_accounts.Add(account.id, account);
		}

		public Account GetAccount(Guid id)
		{
			Account value;

			return m_accounts.TryGetValue(id, out value) ? value : null;
		}

		public void RemoveAccount(Guid id)
		{
			m_accounts.Remove(id);
		}

		//
		//
		//

		public void Dispose()
		{
			if (m_disposed)
				return;

			m_disposed = true;

			//
			//
			//

			m_timer.Dispose();

			//
			// 계정 로그아웃 처리
			//

			foreach (Account account in m_accounts.Values.ToArray())
			{
				AccountSynchronizer accountSynchronizer = new AccountSynchronizer(account, new SFAction(account.Logout), false);
				accountSynchronizer.Start();
			}

			//
			//
			//

			m_worker.Stop();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		private static Cache s_instance = new Cache();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static properties

		public static Cache instance
		{
			get { return s_instance; }
		}
	}
}
