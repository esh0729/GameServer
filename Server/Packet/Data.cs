using System;
using System.IO;

namespace Server
{
	//=====================================================================================================================
	// 수신받은 패킷 중 실제 사용할 데이터를 추출 하거나, 전달할 패킷을 전송하기 위해서 부가 정보를 추가 하는 클래스
	//=====================================================================================================================
	public class Data
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		// 버퍼사이즈를 저장할 데이터의 길이
		public const int kLengthSize = 4;
		// 순환중복검사의 결과 데이터의 길이
		public const int kChecksumSize = 2;
		// PingCheck에 사용될 버퍼 길이(내부 데이터를 사용하지 않으므로 
		public const int kNonPacketDataSize = 11;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 패킷데이터의 사용처
		private PacketType m_type;
		// 실제 사용할 패킷 데이터의 길이
		private int m_nPacketLength;
		// 실제 사용할 패킷데이터를 담고 있는 버퍼
		private byte[] m_packet = new byte[ushort.MaxValue];

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 패킷데이터의 사용처
		public PacketType type
		{
			get { return m_type; }
			set { m_type = value;}
		}

		// 실제 사용할 패킷 데이터의 길이
		public int packetLength
		{
			get { return m_nPacketLength; }
			set { m_nPacketLength = value; }
		}

		// 실제 사용할 패킷데이터를 담고 있는 버퍼
		public byte[] packet
		{
			get { return m_packet; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//=====================================================================================================================
		// 패킷데이터를 전송하기 전 필요한 정보를 추가 하는 함수
		//
		// buffer : 패킷데이터 및 추가 정보를 담을 버퍼
		// nBufferLength : 데이터가 담긴 버퍼의 길이를 반환하는 변수
		//=====================================================================================================================
		public void GetBytes(byte[] buffer, out int nBufferLength)
		{
			// 상단에 정수(int)로 버퍼 길이 삽입
			nBufferLength = kLengthSize;

			// 패킷 타입
			buffer[nBufferLength++] = (byte)m_type;
			// 패킷 길이
			buffer[nBufferLength++] = (byte)(0x000000ff & m_nPacketLength);
			buffer[nBufferLength++] = (byte)(0x000000ff & (m_nPacketLength >> 8));
			buffer[nBufferLength++] = (byte)(0x000000ff & (m_nPacketLength >> 16));
			buffer[nBufferLength++] = (byte)(0x000000ff & (m_nPacketLength >> 24));
			// 패킷
			if (m_nPacketLength > 0)
			{
				Array.Copy(m_packet, 0, buffer, nBufferLength, m_nPacketLength);
				nBufferLength += m_nPacketLength;
			}
			// 체크섬(순환중복검사) 전체 길이는 체크하지 않음
			ushort usChecksum = Crc16.Calc(buffer, kLengthSize, nBufferLength);
			buffer[nBufferLength++] = (byte)(0x00ff & usChecksum);
			buffer[nBufferLength++] = (byte)(0x00ff & (usChecksum >> 8));
			// 전체 길이
			buffer[0] = (byte)(0x000000ff & nBufferLength);
			buffer[1] = (byte)(0x000000ff & (nBufferLength >> 8));
			buffer[2] = (byte)(0x000000ff & (nBufferLength >> 16));
			buffer[3] = (byte)(0x000000ff & (nBufferLength >> 24));
		}

		//=====================================================================================================================
		// 수신한 데이터의 유효성 검사와 실제 사용할 패킷데이터를 추출하는 함수
		// Return : 유효한 데이터일 경우 true 반환
		//
		// buffer : 수신데이터가 저장되어 있는 버퍼
		// nLength : 수신데이터의 길이
		//=====================================================================================================================
		public bool SetData(byte[] buffer, int nLength)
		{
			if (buffer == null)
				return false;

			// 상단에 정수(int)로 버퍼 길이 부터 시작
			int nCurrentIndex = kLengthSize;

			// 체크섬(순환중복검사)
			ushort usChecksum = Crc16.Calc(buffer, nCurrentIndex, nLength - 2);
			if (buffer[nLength - 2] != (byte)(0x00ff & usChecksum) || buffer[nLength - 1] != (byte)(0x00ff & (usChecksum >> 8)))
				return false;

			// 게임서버에 사용될 데이터를 m_packet 버퍼에 복사
			m_type = (PacketType)buffer[nCurrentIndex++];
			m_nPacketLength = (int)(buffer[nCurrentIndex++] | buffer[nCurrentIndex++] << 8 | buffer[nCurrentIndex++] << 16 | buffer[nCurrentIndex++] << 24);
			Array.Copy(buffer, nCurrentIndex, m_packet, 0, m_nPacketLength);

			return true;
		}
	}
}
