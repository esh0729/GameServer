./  
#. RequestType - 클라이언트 요청에 대한 타입(Server.OperationRequest.operationcode 에 사용)  
#. SFPeerImpl(Server.PeerBase 상속) - 클라이언트 요청의 처리에 대한 분류와 클라이언트 송신에대한 응답 또는 이벤트 객체를 생성하는 추상 클래스  
#. SFRandom - 난수를 생성하는 클래스  

./Parameter  
#. CommandParameter - 명령(응답이 필요한 요청) 및 응답의 데이터를 저장하는 컬렉션의 세부 내용 열거형  
#. ClientEventParameter - 클라이언트 이벤트(응답이 필요 없는 요청)의 데이터를 저장하는 컬렉션의 세부 내용 열거형  
#. ServerEventParameter - 서버 이벤트(클라이언트의 요청 없이 서버에서 송신하는 메세지)의 데이터를 저장하는 컬렉션의 세부 내용 열거형  

./Work  
#. ISFWork - 특정작업을 실행하는 함수의 구현을 필요로 하는 클래스의 인터페이스  
#. SFAction(ISFWork 상속) - 특정 시점에 실행을 위한 함수 대리자를 가지고 있는 클래스(제네릭 매개변수 0개 ~ 3개 까지 지원되는 클래스 구현)  
#. SFWorker - 특정 작업을 큐잉하여 순서대로 실행을 시키는 클래스  
#. SFMultiWorker - 많은 작업을 큐잉하여 실행시키는 경우 사용하는 클래스  
./Work/Sql  
#. SFSqlWork - MS-SQL 데이터베이스에 대한 Sql(쿼리 또는 저장프로시저) 작업을 실행하는 클래스
