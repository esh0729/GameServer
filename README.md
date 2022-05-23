# GameServer
.NET Framework기반의 Socket통신 게임서버

프로젝트 설명
- ClientCommon : 클라이언트와 통신에 필요한 프로토콜
- GameServer : 인게임 구현
- Server : Socket 통신 구현
- ServerFramework : 인게임 구현에 필요한 Base클래스 및 Util클래스의 집합

메인 시작
- Server.ApplicationBase 클래스를 상속받은 GameServer.Server 클래스 Main 함수에서 Init 함수를 호출하여 게임서버를 구동함.
