﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace ServerFramework
{
	public class SFWorker
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private object m_syncObject = new object();

		private Queue<ISFWork> m_works = new Queue<ISFWork>();

		private ManualResetEvent m_nextSignal = new ManualResetEvent(false);
		private ManualResetEvent m_endSignal = new ManualResetEvent(false);

		private bool m_bRunning = false;
		private bool m_bResting = false;
		private bool m_bDisposed = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public bool resting
		{
			get { return m_bResting; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Start()
		{
			m_bRunning = true;
			m_bResting = true;

			m_nextSignal.Reset();
			m_endSignal.Set();

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
					m_bResting = false;

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

			work.Run();

			lock (m_syncObject)
			{
				m_works.Dequeue();

				if (m_works.Count == 0)
				{
					m_bResting = true;

					m_nextSignal.Reset();
					m_endSignal.Set();
				}
			}
		}

		private void Dispose()
		{
			//
			// 모든작업이 끝날때 까지 대기
			//

			m_endSignal.WaitOne();

			m_bDisposed = true;
			m_nextSignal.Dispose();
			m_endSignal.Dispose();
		}
	}
}
