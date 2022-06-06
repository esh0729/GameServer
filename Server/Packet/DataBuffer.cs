using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	//=====================================================================================================================
	// 송신할 최종패킷을 담거나 수신 받은 패킷을 저장하는 버퍼를 관리하는 클래스
	//=====================================================================================================================
	public class DataBuffer
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const int kBufferLength = ushort.MaxValue;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 송수신할 패킷을 저장하는 버퍼
		private byte[] m_buffer = new byte[kBufferLength];
		// 수신시 패킷을 이어받을 인덱스 / 패킷 직렬화 시 버퍼 내에 여러 패킷이 있을경우 첫번째 패킷의 시작 인덱스
		private int m_nCurrentIndex = 0;
		// 수신시 패킷의 길이를 담는 변수 / 패킷 직렬화 시 버퍼 내에 앞으로 사용할 패킷의 길이
		private int m_nUseLength = 0;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 송수신할 패킷을 저장하는 버퍼
		public byte[] buffer
		{
			get { return m_buffer; }
		}

		// 수신시 패킷을 이어받을 인덱스 / 패킷 직렬화 시 버퍼 내에 여러 패킷이 있을경우 첫번째 패킷의 시작 인덱스
		public int currentIndex
		{
			get { return m_nCurrentIndex; }
			set { m_nCurrentIndex = value; }
		}

		// 수신시 패킷의 길이를 담는 변수 / 패킷 직렬화 시 버퍼 내에 아직 사용하지 않은 패킷의 길이
		public int receiveLength
		{
			get { return m_nUseLength; }
			set { m_nUseLength = value; }
		}

		//
		//
		//

		// 버퍼내의 패킷의 총 길이
		public int totalReceiveLength
		{
			get { return m_nCurrentIndex + m_nUseLength; }
		}

		// 첫번째 패킷의 길이
		public int dataLength
		{
			get { return (int)(m_buffer[0]
							 | m_buffer[1] << 8
							 | m_buffer[2] << 16
							 | m_buffer[3] << 24); }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 사용한 패킷을 정리하는 함수, 받은데이터가 사용한 데이터보다 클 경우 사용한 데이터를 제외한 나머지 데이터 0번 인덱스 부터 재배치
		//=====================================================================================================================
		public void ClearUsedReceiveData()
		{
			// 받은 데이터가 실제 사용데이터 보다 같거나 작을경우 재배치할 데이터가 없음
			if (totalReceiveLength <= dataLength)
				m_nUseLength = 0;
			else
			{
				// 사용한 데이터 길이
				int nUsedDataLength = dataLength;

				// 사용하지 않은 데이터 첫 인덱스부터 재배치
				int nIndex = 0;
				for (int i = nUsedDataLength; i < totalReceiveLength; i++, nIndex++)
				{
					m_buffer[nIndex] = m_buffer[i];
				}

				// 사용하지 않은데이터 길이(총 받은 데이터 길이 - 사용데이터 길이)
				m_nUseLength = totalReceiveLength - nUsedDataLength;
			}

			m_nCurrentIndex = 0;
		}

		//=====================================================================================================================
		// 버퍼 초기화 함수, 사용길이와 현재인덱스를 0으로 초기화 하여 버퍼 인덱스 0부터 데이터 넣을수 있도록 처리
		//=====================================================================================================================
		public void ClearReceiveData()
		{
			m_nUseLength = 0;
			m_nCurrentIndex = 0;
		}
	}
}
