﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
	public abstract class ApplicationBase
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Socket m_listener = null;
		private Dictionary<Guid, PeerBase> m_peers = new Dictionary<Guid, PeerBase>();

		private object m_syncObject = new object();

		private int m_nConnectionTimeoutInternal = 0;

		private bool m_bAccpetAwaiting = false;
		private bool m_bDiposed = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public int connectionTimeoutInterval
		{
			get { return m_nConnectionTimeoutInternal; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected void Start(int nPort, int nBackLogCount = 128, int nConnectionTimeoutInterval = 30000)
		{
			m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, nPort);

			m_listener.Bind(ipEndPoint);
			m_listener.Listen(nBackLogCount);

			m_nConnectionTimeoutInternal = nConnectionTimeoutInterval;
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

				Socket clientSocket = await Task.Factory.FromAsync<Socket>(m_listener.BeginAccept(null, null), m_listener.EndAccept);

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
			if (m_bDiposed)
				return;

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
			if (m_bDiposed)
				return;

			m_bDiposed = true;

			lock (m_syncObject)
			{
				foreach (PeerBase peer in m_peers.Values.ToArray())
				{
					peer.Disconnect();
				}
			}

			m_listener.Close();

			//
			//
			//

			OnTearDown();
		}

		private void CloseConnection()
		{
			Socket closeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000);

			closeSocket.Connect(ipEndPoint);
		}

		private void CloseSend()
		{
			byte[] buffer = new byte[] { };

			m_listener.Send(buffer);
			m_listener.Receive(buffer);
		}

		protected abstract void OnTearDown();
	}
}