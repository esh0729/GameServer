using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
	public abstract class ApplicationBase
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Socket m_serverSocket = null;
		private Dictionary<Guid, PeerBase> m_peers = new Dictionary<Guid, PeerBase>();

		private object m_syncObject = new object();

		private bool m_bAccpetAwaiting = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected void Start(int nPort, int nBackLogCount)
		{
			m_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, nPort);

			m_serverSocket.Bind(ipEndPoint);
			m_serverSocket.Listen(nBackLogCount);
		}

		protected abstract PeerBase CreatePeer(PeerInit peerInit);
		
		protected virtual void OnAccpetError(Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

		private async void Accpet()
		{
			try
			{
				m_bAccpetAwaiting = true;

				PeerBase peer = null;

				Socket clientSocket = await Task.Factory.FromAsync<Socket>(m_serverSocket.BeginAccept(null, null), m_serverSocket.EndAccept);

				lock (m_syncObject)
				{
					try
					{
						peer = CreatePeer(new PeerInit(this, clientSocket));
						m_peers.Add(peer.id, peer);
					}
					catch (Exception ex)
					{
						if (peer != null)
							peer.Disconnect();

						OnAccpetError(ex);
					}
				}
			}
			finally
			{
				m_bAccpetAwaiting = false;
			}
		}

		public void Service()
		{
			if (!m_bAccpetAwaiting)
				Accpet();

			lock (m_syncObject)
			{
				foreach (PeerBase peer in m_peers.Values.ToArray())
				{
					peer.Service();
				}
			}
		}

		public void RemovePeer(PeerBase peer)
		{
			lock (m_syncObject)
			{
				m_peers.Remove(peer.id);
			}
		}

		//
		//
		//

		public void Dispose()
		{
			lock (m_syncObject)
			{
				foreach (PeerBase peer in m_peers.Values.ToArray())
				{
					peer.Disconnect();
				}
			}

			m_serverSocket.Disconnect(true);
			m_serverSocket.Close();
		}
	}
}