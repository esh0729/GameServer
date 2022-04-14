using System;
using System.Collections.Generic;

namespace ServerFramework
{
	public abstract class SFHandlerFactory
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Dictionary<int, Type> m_handlers = new Dictionary<int, Type>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected void AddHandler<T>(int nName) where T : SFHandler
		{
			m_handlers.Add(nName, typeof(T));
		}

		protected Type GetHandler(int nName)
		{
			Type type;

			return m_handlers.TryGetValue(nName, out type) ? type : null;
		}

		public void Init()
		{
			InitInternal();
		}

		protected abstract void InitInternal();
	}
}
