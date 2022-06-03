using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class DataBuffer
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const int kBufferLength = ushort.MaxValue;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private byte[] m_buffer = new byte[kBufferLength];
		private int m_nCurrentIndex = 0;
		private int m_nUseLength = 0;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public byte[] buffer
		{
			get { return m_buffer; }
		}

		public int currentIndex
		{
			get { return m_nCurrentIndex; }
			set { m_nCurrentIndex = value; }
		}

		public int receiveLength
		{
			get { return m_nUseLength; }
			set { m_nUseLength = value; }
		}

		//
		//
		//

		public int receivedLength
		{
			get { return m_nCurrentIndex + m_nUseLength; }
		}

		public int dataLength
		{
			get { return (int)(m_buffer[0]
							 | m_buffer[1] << 8
							 | m_buffer[2] << 16
							 | m_buffer[3] << 24); }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		// 받은데이터가 사용한 데이터보다 클 경우 사용한 데이터를 제외한 나머지 데이터 0번 인덱스 부터 재배치
		public void ClearUsedReceiveData()
		{
			// 받은 데이터가 실제 사용데이터 보다 같거나 작을경우 재배치할 데이터가 없음
			if (receivedLength <= dataLength)
				m_nUseLength = 0;
			else
			{
				// 사용한 데이터 길이
				int nUsedDataLength = dataLength;

				// 사용하지 않은 데이터 첫 인덱스부터 재배치
				int nIndex = 0;
				for (int i = nUsedDataLength; i < receivedLength; i++, nIndex++)
				{
					m_buffer[nIndex] = m_buffer[i];
				}

				// 사용하지 않은데이터 길이(받은 데이터 길이 - 사용데이터 길이)
				m_nUseLength = receivedLength - nUsedDataLength;
			}

			m_nCurrentIndex = 0;
		}

		// 사용길이와 현재인덱스를 0으로 초기화 하여 버퍼 인덱스 0부터 데이터 넣을수 있도록 처리
		public void ClearReceiveData()
		{
			m_nUseLength = 0;
			m_nCurrentIndex = 0;
		}
	}
}
