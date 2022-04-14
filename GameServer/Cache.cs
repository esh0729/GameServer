using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServerFramework;

namespace GameServer
{
	public class Cache
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private SFWorker m_worker = new SFWorker();

		private object m_syncObject = new object();

		//
		// 계정
		//

		private Dictionary<Guid, Account> m_accounts = new Dictionary<Guid, Account>();

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

			m_worker.Start();

			LogUtil.Info(GetType(), "Cache Init Completed.");
		}

		public void AddWork(ISFWork work)
		{
			m_worker.Add(work);
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
