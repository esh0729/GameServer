namespace ServerFramework
{
	//=====================================================================================================================
	// 서버 이벤트(클라이언트의 요청 없이 서버에서 송신하는 메세지)의 데이터를 저장하는 컬렉션의 세부 내용 열거형
	//=====================================================================================================================
	public enum ServerEventParameter : byte
	{
		// 서버 이벤트 타입
		Name = 1,
		// 패킷
		Packet
	}
}
