./  
#. Cache - 생성된 시스템관련 객체를 관리하는 클래스  
#. Resource - 시스템에서 사용될 리소스 데이터를 호출 및 관리하는 클래스  
#. Server(ApplicationBase 상속) - 서버의 초기화 및 시작, 인게임의 클라이언트 피어를 관리하는 클래스  

./Client/Handler  
#. CommandHandlerFactory (SFHandlerFactory 상속) - 클라이언트 명령 핸들러의 객체 생성을 위한 명령 타입과 클래스 타입을 생성 및 관리하는 클래스  
#. Handler(SFHandler 상속) - 클라이언트 요청에 대한 작업을 동기처리 하여 시작하는 추상 클래스 

./Client/Handler/Command  
#. CommandHandler(Handler 상속) - 클라이언트 명령에 대한 작업의 초기화, 처리, 응답, 에러처리에 대한 함수를 제공하는 클래스  

./Client/Handler/Command/Login  
#. LoginCommandHandler(CommandHandler 상속) - 계정 로그인에 대한 클라이언트 명령을 처리하는 클래스  
#. LoginRequiredCommandHandler(CommandHandler 상속) - 계정 로그인이 필요한 클라이언트 명령을 지원하는 추상 클래스  
#. LobbyInfoCommandHandler(LoginRequiredCommandHandler 상속) - 계정 영웅 정보 호출 대한 클라이언트 명령을 처리하는 클래스  
#. HeroCreateCommandHandler(LoginRequiredCommandHandler 상속) - 영웅 생성에 대한 클라이언트 명령을 처리하는 클래스  

./Client/Handler/Command/Login/InGame  
#. InGameCommandHandler(LoginRequiredCommandHandler 상속) - 영웅 로그인이 필요한 클라이언트 명령을 지원하는 추상 클래스  
#. HeroLoginCommandHandler(LoginRequiredCommandHandler 상속) - 영웅 로그인에 대한 클라이언트 명령을 처리하는 클래스  
#. HeroLogoutCommandHandler(InGameCommandHandler 상속) - 영웅 로그아웃에 대한 클라이언트 명령을 처리하는 클래스  
#. HeroInitEnterCommandHandler(InGameCommandHandler 상속) - 영웅 로그인 이후 초기 입장에 대한 클라이언트 명령을 처리하는 클래스  

./Client/Handler/Command/Login/InGame/Action  
#. ActionCommandHandler(LoginRequiredCommandHandler 상속) - 캐릭터 행동 클라이언트 명령을 처리하는 클래스  

./Client/Handler/Command/Login/InGame/Move  
#. HeroMoveStartCommandHandler(LoginRequiredCommandHandler 상속) - 영웅이동시작 클라이언트 명령을 처리하는 클래스  
#. HeroMoveCommandHandler(LoginRequiredCommandHandler 상속) - 영웅이동 클라이언트 명령을 처리하는 클래스  
#. HeroMoveEndCommandHandler(LoginRequiredCommandHandler 상속) - 영웅이동종료 클라이언트 명령을 처리하는 클래스  

./Client/ServerEvent  
#. ServerEvent - 서버 이벤트 객체 생성 및 클라이언트 피어에게 서버 이벤트 객체 전달하는 클래스  

./Instance  
#. Unit - 영웅 및 몬스터의 기본이 되는 추상 클래스  

./Instance/Account  
#. Account - 사용자가 서버에 로그인하여 생성되는 사용자의 정보를 저장하고 있는 클래스  
#. AccountStatus - 계정 상태 열거자  

./Instance/Entrance  
#. EntranceParam - 장소입장에 대한 데이터를 저장하기 위한 추상 클래스  
#. HeroInitEnterParam - 영웅초기입장에 대한 데이터를 저장하기 위한 클래스  

