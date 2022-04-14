using System;
using System.Net.Sockets;

namespace Server
{
	public class PeerInit
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private ApplicationBase m_applicationBase = null;
		private Socket m_socket = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public PeerInit(ApplicationBase applicationBase, Socket socket)
		{
			if (applicationBase == null)
				throw new ArgumentNullException("applicationBase");

			if (socket == null)
				throw new ArgumentNullException("socket");

			m_applicationBase = applicationBase;
			m_socket = socket;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public ApplicationBase applicationBase
		{
			get { return m_applicationBase; }
		}

		public Socket socket
		{
			get { return m_socket; }
		}
	}
}
