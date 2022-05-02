using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ServerFramework
{
	public class SFMultiWorker
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private object m_syncObject = new object();

		private Queue<ISFWork> m_works = new Queue<ISFWork>();
		private List<SFWorker> m_workers = new List<SFWorker>();

		private ManualResetEvent m_nextSignal = new ManualResetEvent(false);
		private ManualResetEvent m_endSignal = new ManualResetEvent(false);

		private bool m_bRunning = false;
		private bool m_bDisposed = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Start(int nWorkerCount = 10)
		{
			m_bRunning = true;

			m_nextSignal.Reset();
			m_endSignal.Set();

			for (int i = 0; i < nWorkerCount; i++)
			{
				SFWorker worker = new SFWorker();
				worker.Start();

				m_workers.Add(worker);
			}

			ThreadPool.QueueUserWorkItem(Running);
		}

		public void Stop()
		{
			m_bRunning = false;

			Dispose();
		}

		public void Add(ISFWork work)
		{
			if (!m_bRunning)
				return;

			lock (m_syncObject)
			{
				m_works.Enqueue(work);

				if (m_works.Count == 1)
				{
					m_endSignal.Reset();
					m_nextSignal.Set();
				}
			}
		}

		private void Running(object state)
		{
			while (!m_bDisposed)
			{
				m_nextSignal.WaitOne();

				RunWork();
			}
		}

		private void RunWork()
		{
			ISFWork work = m_works.Peek();

			foreach (SFWorker worker in m_workers)
			{
				if (!worker.resting)
					continue;

				worker.Add(work);

				lock (m_syncObject)
				{
					m_works.Dequeue();

					if (m_works.Count == 0)
					{
						m_nextSignal.Reset();
						m_endSignal.Set();
					}

					break;
				}
			}
		}

		private void Dispose()
		{
			//
			// 모든작업이 끝날때 까지 대기
			//

			m_endSignal.WaitOne();

			foreach (SFWorker worker in m_workers)
			{
				worker.Stop();
			}

			m_endSignal.Dispose();
			m_bDisposed = true;
		}
	}
}
