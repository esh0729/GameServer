﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ServerFramework
{
	//=====================================================================================================================
	// 많은 작업을 큐잉하여 실행시키는 경우 사용하는 클래스
	//=====================================================================================================================
	public class SFMultiWorker
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// m_works의 접근을 제한하는 잠금전용 오브젝트
		private object m_syncObject = new object();

		// 특정 작업을 큐잉하는 큐
		private Queue<ISFWork> m_works = new Queue<ISFWork>();
		// 실제 작을을 처리하는 작업자 컬렉션
		private List<SFWorker> m_workers = new List<SFWorker>();

		// 작업의 갯수에 따라 작업진행을 제어할 신호
		private ManualResetEvent m_nextSignal = new ManualResetEvent(false);
		// 해당 객체 종료시 남아있는 작업에 대한 실행을 보장하는 신호
		private ManualResetEvent m_endSignal = new ManualResetEvent(false);

		// 작동 여부(작업을 받을 수 있는지 여부)
		private bool m_bRunning = false;
		// 리소스 해제 여부
		private bool m_bDisposed = false;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 작업을 실행하는 작업자를 생성하고 작업을 큐잉처리를 할수 있는 상태로 전환하는 함수
		//=====================================================================================================================
		public void Start(int nWorkerCount = 10)
		{
			// 작동 여부를 true로 하여 작업을 받을수 있는 상태로 전환
			m_bRunning = true;

			// 작업 실행 호출 반복문 신호 차단 설정
			m_nextSignal.Reset();
			// 리소스 해제 신호 받음 설정
			m_endSignal.Set();

			// 작업자 생성 및 작업진행 상태로 전환
			for (int i = 0; i < nWorkerCount; i++)
			{
				SFWorker worker = new SFWorker();
				worker.Start();

				m_workers.Add(worker);
			}

			// 작업큐를 체크하며 작업을 분배할 콜백 함수 대기중인 스레드에서 실행
			ThreadPool.QueueUserWorkItem(Running);
		}

		//=====================================================================================================================
		// 더이상 작업을 받으수 없는 상태로 전환 후 종료 처리
		//=====================================================================================================================
		public void Stop()
		{
			// 이미 작동중이지 않다면 return
			if (!m_bRunning)
				return;

			// 작용 여부를 false로 하여 작업을 받을수 없는 상태로 전환
			m_bRunning = false;

			// 리소스 해제 처리
			Dispose();
		}

		//=====================================================================================================================
		// 작업을 추가 하는 함수
		//
		// work : 실행 가능한 함수 대리자를 가지고 있는 객체
		//=====================================================================================================================
		public void Add(ISFWork work)
		{
			// 작동 중이지 않을 경우 return
			if (!m_bRunning)
				return;

			// 작업큐 객체의 동시 접근을 막기위한 lock 처리
			lock (m_syncObject)
			{
				// 작업 객체 큐잉
				m_works.Enqueue(work);

				// 작업이 없던 상태에서 작업이 들어온 경우 상태 변환
				if (m_works.Count == 1)
				{
					// 리소스 해제 신호 차단 설정
					m_endSignal.Reset();
					// 작업 실행 호출 반복문 신호 받음 설정
					m_nextSignal.Set();
				}
			}
		}

		//=====================================================================================================================
		// 반복문을 돌며 작업 분배 함수를 호출하는 스레드 풀 콜백 함수
		//
		// state : 스레드풀 사용에 필요한 상태 변수
		//=====================================================================================================================
		private void Running(object state)
		{
			// 리소스 해제 전까지 반복 실행
			while (!m_bDisposed)
			{
				// 작업큐에 데이터가 없을 경우 대기
				m_nextSignal.WaitOne();

				// 작업 분배
				RunWork();
			}
		}

		//=====================================================================================================================
		// 작업 분배 함수
		//=====================================================================================================================
		private void RunWork()
		{
			// 작업 처리 여부
			bool bIsProcessed = false;

			try
			{
				// 작업큐에서 가장 처음 들어온 작업 호출
				ISFWork work = m_works.Peek();

				// 작업자 전체를 돌면서 현재 작업을 하고 있지 않는 작업자 호출
				foreach (SFWorker worker in m_workers)
				{
					// 현재 작업중일 경우 다음 작업자로 작업을 넘김
					if (!worker.resting)
						continue;

					// 작업자에 작업 큐잉
					worker.Add(work);

					// 작업 큐잉 완료했을 경우 true로 전환
					bIsProcessed = true;

					break;
				}
			}
			catch
			{

			}

			// 처음 들어온 작업을 처리하지 않았을 경우 return 처리
			if (!bIsProcessed)
				return;

			try
			{
				// 작업큐 객체의 동시 접근을 막기위한 lock 처리
				lock (m_syncObject)
				{
					if (m_works.Count == 0)
						return;

					// 작업큐에서 처리된 첫번째 항목 삭제
					m_works.Dequeue();

					// 작업큐의 갯수 체크
					if (m_works.Count == 0)
					{
						// 작업 실행 호출 반복문 신호 차단 설정
						m_nextSignal.Reset();
						// 리소스 해제 신호 받음 설정
						m_endSignal.Set();
					}
				}
			}
			catch
			{

			}
		}

		//=====================================================================================================================
		// 리소스 해제 함수
		//=====================================================================================================================
		private void Dispose()
		{
			// 현재 큐잉 중인 모든작업의 분배가 끝날때 까지 대기
			m_endSignal.WaitOne();

			// 리소스 해제여부를 true로 하며 작업 분배 반복문의 조건을 빠져나옴
			m_bDisposed = true;

			// 모든 작업자들의 작업이 끝날때 까지 대기 이후 리소스 해제 처리
			foreach (SFWorker worker in m_workers)
			{
				worker.Stop();
			}

			// 스레드 신호 이벤트 관련 객체 리소스 해제
			m_nextSignal.Dispose();
			m_endSignal.Dispose();			
		}
	}
}
