using System;
using System.IO;

namespace Server
{
	public class Data
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const byte STX = 0x02;
		public const byte ETX = 0x03;

		public const int kNonPacketDataSize = 9;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private PacketType m_type;
		private int m_nPacketLength;
		private byte[] m_packet = new byte[ushort.MaxValue];

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public PacketType type
		{
			get { return m_type; }
			set { m_type = value;}
		}

		public int packetLength
		{
			get { return m_nPacketLength; }
			set { m_nPacketLength = value; }
		}

		public byte[] packet
		{
			get { return m_packet; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void GetBytes(byte[] buffer, out int nBufferLength)
		{
			//
			// 버퍼에 데이터 삽입
			//

			nBufferLength = 0;

			// 본문 시작
			buffer[nBufferLength++] = STX;
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
			// 체크섬(순환중복검사)
			byte[] checkArray = new byte[nBufferLength - 1];
			Array.Copy(buffer, 1, checkArray, 0, checkArray.Length);
			byte[] crc = BitConverter.GetBytes(Crc16.Calc(checkArray));
			buffer[nBufferLength++] = crc[0];
			buffer[nBufferLength++] = crc[1];
			// 본문 종료
			buffer[nBufferLength++] = ETX;
		}

		public bool SetData(byte[] buffer, int nLength)
		{
			if (buffer == null)
				return false;

			//
			// STX, ETX 검사
			//

			if (buffer[0] != STX || buffer[nLength - 1] != ETX)
				return false;

			

			//
			// 체크섬(순환중복검사)
			//

			byte[] checkArray = new byte[nLength - 4];
			Array.Copy(buffer, 1, checkArray, 0, checkArray.Length);
			byte[] crc = BitConverter.GetBytes(Crc16.Calc(checkArray));

			if (buffer[nLength - 3] != crc[0] || buffer[nLength - 2] != crc[1])
				return false;

			//
			// 패킷
			//

			m_type = (PacketType)buffer[1];
			m_nPacketLength = (int)(buffer[2] | buffer[3] << 8 | buffer[4] << 16 | buffer[5] << 24);
			Array.Copy(buffer, 6, m_packet, 0, m_nPacketLength);

			return true;
		}
	}
}
