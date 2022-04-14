using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server;
using ClientCommon;
using ServerFramework;

namespace GameServer
{
	public abstract class CommandHandler<T1, T2> : Handler
		where T1 : CommandBody where T2 : ResponseBody	
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private CommandName m_name = CommandName.None;
		private long m_lnCommandId = 0;
		protected T1 m_body = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public CommandName name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		public long commandId
		{
			get { return m_lnCommandId; }
			set { m_lnCommandId = value; }
		}

		public T1 body
		{
			get { return m_body; }
			set { m_body = value; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void InitInternal(int nName, long lnCommandId, byte[] packet)
		{
			m_name = (CommandName)nName;
			m_lnCommandId = lnCommandId;
			m_body = (T1)Activator.CreateInstance(typeof(T1));

			m_body.DeserializeRaw(packet);
		}

		protected override void OnHandle()
		{
			try
			{
				OnCommandHandle();
			}
			catch (CommandHandleException ex)
			{
				ErrorLog("Error " + ex.result + ", " + ex.Message, true, ex.StackTrace);

				SendResponse(ex.result, ex.Message);
			}
			catch (Exception ex)
			{
				ErrorLog("Error " + kResult_Error + ", " + ex.Message, true, ex.StackTrace);

				SendResponse(kResult_Error, ex.Message);
			}
		}

		protected abstract void OnCommandHandle();

		protected void RunnableStandaloneWork(Action action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			SFStandaloneWork work = new SFStandaloneWork();
			work.work = new SFAction<Action>(Runnable, action);

			work.Schedule();
		}

		private void Runnable(Action action)
		{
			try
			{
				//
				// 비동기
				//

				action();

				//
				// 내부에서 동기실행
				//

				FinishWork(null);
			}
			catch (Exception ex)
			{
				FinishWork(ex);
			}
		}

		protected void FinishWork(Exception ex)
		{
			if (ex == null)
			{
				ClientPeerSynchronizer synchronizer = new ClientPeerSynchronizer(clientPeer, new SFAction(OnWorkSuccess));

				if (globalLockRequired)
				{
					lock (Cache.instance.syncObject)
					{
						synchronizer.Start();
					}
				}
				else
					synchronizer.Start();
			}
			else
			{
				ErrorLog("Error " + kResult_Error + ", " + ex.Message, true, ex.StackTrace);

				SendResponse(kResult_Error, ex.Message);

				OnWorkFail(ex);
			}
		}

		protected virtual void OnWorkSuccess()
		{

		}

		protected virtual void OnWorkFail(Exception ex)
		{

		}


		//
		//
		//

		private void SendResponse(OperationResponse response)
		{
			m_peer.SendResponse(response);
		}

		protected void SendResponse(short nResult, string sDebugMessage)
		{
			OperationResponse response = m_peer.CreateOperationResponse(nResult, m_peer.CreateCommandParameters((int)m_name, new byte[] { }, m_lnCommandId));
			response.debugMessage = sDebugMessage;

			SendResponse(response);
		}

		protected void SendResponse(short nResult, T2 responseBody)
		{
			SendResponse(m_peer.CreateOperationResponse(
				nResult, m_peer.CreateCommandParameters((int)m_name, responseBody != null ? responseBody.SerializeRaw() : new byte[] { }, m_lnCommandId)));
		}

		protected void SendResponseOK(T2 responseBody)
		{
			SendResponse(kResult_OK, responseBody);
		}

		//
		//
		//

		protected void ErrorLog(string sMessage)
		{
			ErrorLog(sMessage, false, null);
		}

		protected void ErrorLog(string sMessage, bool bLoggingTrace, string sStackTrace)
		{
			StringBuilder sb = new StringBuilder();

			ErrorFrom(sb);

			LogUtil.Error(GetType(), sb, sMessage, bLoggingTrace, sStackTrace); 
		}

		protected virtual void ErrorFrom(StringBuilder sb)
		{
			sb.Append("# PeerId : ");
			sb.Append(m_peer.id);
			sb.AppendLine();
		}
	}
}
