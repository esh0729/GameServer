using System;
using System.IO;

namespace Server
{
	public class Data
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const int kLengthSize = 4;
		public const int kChecksumSize = 2;
		public const int kNonPacketDataSize = 11;

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

		public bool SetData(byte[] buffer, int nLength)
		{
			if (buffer == null)
				return false;

			//
			// 체크섬(순환중복검사)
			//

			int nCurrentIndex = kLengthSize;


			ushort usChecksum = Crc16.Calc(buffer, nCurrentIndex, nLength - 2);
			if (buffer[nLength - 2] != (byte)(0x00ff & usChecksum) || buffer[nLength - 1] != (byte)(0x00ff & (usChecksum >> 8)))
				return false;

			//
			// 패킷
			//

			m_type = (PacketType)buffer[nCurrentIndex++];
			m_nPacketLength = (int)(buffer[nCurrentIndex++] | buffer[nCurrentIndex++] << 8 | buffer[nCurrentIndex++] << 16 | buffer[nCurrentIndex++] << 24);
			Array.Copy(buffer, nCurrentIndex, m_packet, 0, m_nPacketLength);

			return true;
		}
	}
}
