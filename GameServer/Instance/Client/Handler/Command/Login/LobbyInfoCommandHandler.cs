using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ClientCommon;
using ServerFramework;

namespace GameServer
{
	public class LobbyInfoCommandHandler : LoginRequiredCommandHandler<LobbyInfoCommandBody, LobbyInfoResponseBody>
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private List<PDLobbyHero> m_heroes = new List<PDLobbyHero>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnLoginRequiredCommandHandle()
		{
			RunnableStandaloneWork(Process);
		}

		private void Process()
		{
			SqlConnection conn = null;

			try
			{
				conn = Util.OpenGameDBConnection();

				//
				// 영웅 목록
				//

				DataRowCollection drcHeroes = GameDBDoc.Heroes(conn, null, m_myAccount.id);

				foreach (DataRow drHero in drcHeroes)
				{
					PDLobbyHero hero = new PDLobbyHero();
					hero.heroId = DBUtil.ToGuid(drHero["heroId"]);
					hero.name = Convert.ToString(drHero["name"]);
					hero.characterId = Convert.ToInt32(drHero["characterId"]);

					m_heroes.Add(hero);
				}

				//
				//
				//

				Util.Close(ref conn);
			}
			finally
			{
				if (conn != null)
					Util.Close(ref conn);
			}
		}

		protected override void OnWorkSuccess()
		{
			base.OnWorkSuccess();

			ProcessCompleted();
		}

		private void ProcessCompleted()
		{
			//
			// 응답
			//

			LobbyInfoResponseBody resBody = new LobbyInfoResponseBody();
			resBody.heroes = m_heroes.ToArray();

			SendResponseOK(resBody);
		}
	}
}
