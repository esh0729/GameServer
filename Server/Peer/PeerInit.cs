using System;
using System.Net.Sockets;

namespace Server
{
	//=====================================================================================================================
	// PeerBase 객체에 서버 메인클래스 객체와 클라이언트 소켓 정보를 전달하기 위한 클래스
	//=====================================================================================================================
	public class PeerInit
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		// 서버 메인클래스 객체
		private ApplicationBase m_applicationBase = null;
		// 클라이언트 소켓
		private Socket m_socket = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		//=====================================================================================================================
		// 생성자
		//
		// applicationBase : 서버 메인클래스 객체
		// socket : 클라이언트 소켓
		//=====================================================================================================================
		public PeerInit(ApplicationBase applicationBase, Socket socket)
		{
			if (applicationBase == null)
				throw new ArgumentNullException("applicationBase");

			if (socket == null)
				throw new ArgumentNullException("socket");

			m_applicationBase = applicationBase;
			m_socket = socket;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		// 서버 메인클래스 객체
		public ApplicationBase applicationBase
		{
			get { return m_applicationBase; }
		}

		// 클라이언트 소켓
		public Socket socket
		{
			get { return m_socket; }
		}
	}
}
