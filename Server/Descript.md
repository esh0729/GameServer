./  
#. ApplicationBase - 서버의 메인 클래스, 서버의 시작과 클라이언트 소켓의 연결수락 처리를 담당하는 추상 클래스  
#. Crc16 - 순환 중복 검사 계산을 처리하는 클래스  

./Packet  
#. Data - 수신받은 패킷 중 실제 사용할 데이터를 추출 하거나, 전달할 패킷을 전송하기 위해서 부가 정보를 추가 하는 클래스  
#. DataBuffer - 송신할 최종패킷을 담거나 수신 받은 패킷을 저장하는 버퍼를 관리하는 클래스  
#. DataQueue - Data 객체를 재활용 하기 위해 Data 객체를 생성/전달/반환하는 클래스  
#. IMessage - 송수신에 필요한 데이터를 저장하는 클래스의 인터페이스  
#. EventData(IMessage 상속) - 클라이언트의 요청 없이 서버에서 클라이언트로 데이터를 송신할 때 사용하는 데이터를 저장하는 클래스  
#. OperationRequest(IMessage 상속) - 클라이언트로부터 수신한 요청 데이터를 저장하는 클래스  
#. OperationResponse(IMessage 상속) - 클라이언트의 요청에대한 응답 데이터를 송신할 때 사용되는 데이터를 저장하는 클래스  
#. PacketType - 패킷의 타입 열거형  

./Peer  
#. PeerBase - 클라이언트와의 연결과 데이터의 송수신을 담당하는 추상 클래스  
#. PeerInit - PeerBase 객체에 서버 메인클래스 객체와 클라이언트 소켓 정보를 전달하기 위한 클래스  
