using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GhostDrawProtobuf;
using GhostDrawServer.Servers;

namespace GhostDrawServer.Controller
{
    class GameController : BaseController
    {
        public GameController()
        {
            requestCode = RequestCode.Game;
        }

        /// <summary>
        /// 用戶準備狀態完成
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack ReadyOk(Server server, Client client, MainPack pack)
        {
            client.UserInfo.readyState = true;
            client.CurrRoom.JudgeReadyState();
            return null;
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack Shuffle(Server server, Client client, MainPack pack)
        {
            client.CurrRoom.UserShuffle(client);
            return null;
        }

        /// <summary>
        /// 選牌
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack SelectPoker(Server server, Client client, MainPack pack)
        {
            client.CurrRoom.Broadcast(client, pack);
            return null;
        }

        /// <summary>
        /// 抽牌
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack DrawCard(Server server, Client client, MainPack pack)
        {
            client.CurrRoom.DrawCard(client, pack);
            return null;
        }
    }
}
