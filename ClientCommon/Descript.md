./  
#. ClientEventName - 클라이언트 이벤트(응답이 필요 없는 요청)의 세부 타입  
#. CommandName - 클라이언트 명령(응답이 필요한 요청)의 세부 타입  
#. ServerEventName - 서버 이벤트(클라이언트의 요청 없이 서버에서 송신하는 메세지)의 세부 타입  
#. PacketReader(BinaryReader 상속) - 데이터 송수신에 사용되는 패킷의 역직렬화를 추가로 처리하는 클래스(BinaryReader로 역직렬화 할수 없는 객체를 역직렬화)  
#. PacketWriter(BinaryWriter 상속) - 데이터 송수신에 사용되는 패킷의 직렬화를 추가로 처리하는 클래스(BinaryWriter로 직렬화 할수 없는 객체를 직렬화)  

./PacketData  
#. PacketData - 데이터 송수신에 처리될 데이터의 직렬화, 역직렬화 함수를 지원하는 추상 클래스  
#. PDVector3 - 송수신에 사용 될 위치 정보를 저장하고 있는 구조체  
./PacketData/Hero  
#. PDHero(PacketData 상속) - 송수신에 사용 될 다른 영웅의 정보를 저장하는 클래스  
#. PDLobbyHero(PacketData 상속) - 송수신에 사용 될 계정의 생성된 영웅의 정보를 저장하는 클래스  
