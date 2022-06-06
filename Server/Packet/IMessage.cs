using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	//=====================================================================================================================
	// 송수신에 필요한 데이터를 저장하는 클래스의 인터페이스
	//=====================================================================================================================
	public interface IMessage
	{
		// 패킷의 전송타입
		PacketType type { get; }

		// 내부 데이터의 직렬화 함수(결과를 매개변수로 반환)
		void GetBytes(byte[] buffer, out long lnLength);
	}
}
