# GameServer
.NET Framework기반의 Socket통신 게임서버  

프로젝트 설명  
- ClientCommon : 클라이언트와 통신에 필요한 프로토콜을 관리하는 프로젝트  
- GameServer : 게임서버가 동작하는 프로젝트  
- Server : 클라이언트와의 연결 및 데이터 송수신 담당하는 프로젝트  
- ServerFramework : 인게임 구현에 필요한 추상 클래스 및 중요 유틸을 담당하는 프로젝트  

메세지 전송 타입  
- 클라이언트 명령 : 클라이언트에서 전송한 메세지로 서버 응답을 필요로함  
- 클라이언트 이벤트 : 클라이언트에서 전송한 메세지로 서버의 응답을 필요로 하지 않음  
- 서버 이벤트 : 클라이언트의 요청 없이 클라이언트로 메세지를 전송  

#. 세부 내용은 각 프로젝트에 있는 Descript.md 참조  

메인 시작  
- Server.ApplicationBase 클래스를 상속받은 GameServer.Server 클래스 Main 함수에서 Init 함수를 호출하여 게임서버를 구동함.  
- Init 함수에서 시스템 데이터 및 게임서버에 필요한 메타데이터를 DB에서 불러온 후 필요 객체 생성, 모든 작업을 마친 후 Socket Listen 시작  

