using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFramework
{
	public abstract class SFHandler : ISFWork
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		protected SFPeerImpl m_peer = null;

		private bool m_bLogged = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public SFPeerImpl peer
		{
			get { return m_peer; }
		}

		public bool logged
		{
			get { return m_bLogged; }
			set { m_bLogged = value; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Init(SFPeerImpl peer, int nName, long lnCommandId, byte[] packet)
		{
			if (peer == null)
				throw new ArgumentNullException("peer");

			m_peer = peer;

			InitInternal(nName, lnCommandId, packet);
		}

		protected abstract void InitInternal(int nName, long lnCommandId, byte[] packet);

		public abstract void Run();

		protected abstract void OnHandle();
	}
}
