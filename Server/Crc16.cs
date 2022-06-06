using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	//=====================================================================================================================
	// 순환 중복 검사 계산을 처리는 하는 클래스
	//=====================================================================================================================
	public class Crc16
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions


		//=====================================================================================================================
		// 순환 중복 검사 계산식을 처리하는 함수
		//
		// pakcet : 전송할 패킷 데이터 버퍼
		// nStartIndex : 계산식을 시작할 인덱스
		// nEndIndex : 계산식을 끝낼 인덱스	(nStartIndex ~ nEndIndex - 1 까지 처리)
		//=====================================================================================================================
		public static ushort Calc(byte[] packet, int nStartIndex, int nEndIndex)
		{
			ushort usCRC = 0xFFFF;
			ushort usTemp = 0;

			for (int i = nStartIndex; i < nEndIndex; i++)
			{
				byte bPacket = packet[i];

				// 하위 4비트에 대한 체크섬 계산
				usTemp = s_Crc16Table[usCRC & 0x000F];
				usCRC = (ushort)((usCRC >> 4) & 0x0FFF);
				usCRC = (ushort)(usCRC ^ usTemp ^ s_Crc16Table[bPacket & 0x000F]);
				// 상위 4비트에 대한 체크섬 계산
				usTemp = s_Crc16Table[usCRC & 0x000F];
				usCRC = (ushort)((usCRC >> 4) & 0x0FFF);
				usCRC = (ushort)(usCRC ^ usTemp ^ s_Crc16Table[(bPacket >> 4) & 0x000F]);
			}

			return usCRC;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		// CRC16 테이블 [참조 : http://www.darkridge.com/~jpr5/mirror/alg/node191.html]
		private static ushort[] s_Crc16Table = { 0x0000, 0xCC01, 0xD801, 0x1400, 0xF001, 0x3C00, 0x2800, 0xE401, 0xA001, 0x6C00, 0x7800, 0xB401, 0x5000, 0x9C01, 0x8801, 0x4400 };
	}
}
