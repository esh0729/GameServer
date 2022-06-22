./  
#. Cache - 생성된 시스템관련 객체를 관리하는 클래스  
#. Resource - 시스템에서 사용될 리소스 데이터를 호출 및 관리하는 클래스  
#. Server(ApplicationBase 상속) - 서버의 초기화 및 시작, 인게임의 클라이언트 피어를 관리하는 클래스  

./Client/Handler  
#. CommandHandlerFactory (SFHandlerFactory 상속) - 클라이언트 명령 핸들러의 객체 생성을 위한 명령 타입과 클래스 타입을 생성 및 관리하는 클래스  
#. Handler(SFHandler 상속) - 클라이언트 요청에 대한 작업을 동기처리 하여 시작하는 추상 클래스  
#. CommandHandler(Handler 상속) - 클라이언트 명령에 대한 작업의 초기화, 처리, 응답, 에러처리에 대한 함수를 제공하는 클래스  
