using System;
using System.Collections.Generic;
using System.Data;

using ServerFramework;

namespace GameServer
{
	public class Account
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private ClientPeer m_clientPeer = null;
		private Guid m_id = Guid.Empty;

		private Guid m_userId = Guid.Empty;
		private DateTimeOffset m_regTime = DateTimeOffset.MinValue;

		private Hero m_currentHero = null;

		private AccountStatus m_status = AccountStatus.Logout;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public Account(ClientPeer clientPeer)
		{
			if (clientPeer == null)
				throw new ArgumentNullException("clientPeer");

			m_clientPeer = clientPeer;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public ClientPeer clientPeer
		{
			get { return m_clientPeer; }
		}

		public Guid id
		{
			get { return m_id; }
		}

		public Guid userId
		{
			get { return m_userId; }
		}

		public DateTimeOffset regTime
		{
			get { return m_regTime; }
		}

		public object syncObject
		{
			get { return m_clientPeer.syncObject; }
		}

		public Hero currentHero
		{
			get { return m_currentHero; }
			set { m_currentHero = value; }
		}

		public AccountStatus status
		{
			get { return m_status; }
		}

		public bool isLoggedIn
		{
			get { return m_status == AccountStatus.Login; }
		}

		public bool isHeroLoggedIn
		{
			get { return m_currentHero != null; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Init(Guid accountId, Guid userId, DateTimeOffset time)
		{
			m_id = accountId;

			m_userId = userId;
			m_regTime = time;

			m_status = AccountStatus.Login;
		}

		public void Init(DataRow dr)
		{
			if (dr == null)
				throw new ArgumentNullException("dr");

			m_id = DBUtil.ToGuid(dr["accountId"]);

			m_userId = DBUtil.ToGuid(dr["userId"]);
			m_regTime = DBUtil.ToDateTimeOffset(dr["regTime"]);

			m_status = AccountStatus.Login;
		}

		public void Logout()
		{
			if (!isLoggedIn)
				return;

			m_status = AccountStatus.Logout;

			if (m_currentHero != null)
				m_currentHero.Logout();

			m_clientPeer.account = null;

			Cache.instance.RemoveAccount(m_id);
		}
	}
}
