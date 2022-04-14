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

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Init(Guid userId, DateTimeOffset time)
		{
			m_id = Guid.NewGuid();

			m_userId = userId;
			m_regTime = time;
		}

		public void Init(DataRow dr)
		{
			if (dr == null)
				throw new ArgumentNullException("dr");

			m_id = DBUtil.ToGuid(dr["accountId"]);

			m_userId = DBUtil.ToGuid(dr["userId"]);
			m_regTime = DBUtil.ToDateTimeOffset(dr["regTime"]);
		}

		public void Logout()
		{
			m_clientPeer.account = null;

			Cache.instance.RemoveAccount(m_id);
		}
	}
}
