using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GhostDrawProtobuf;
using GhostDrawServer.Servers;
using GhostDrawServer.Tools;

namespace GhostDrawServer.Controller
{
    class RoomController : BaseController
    {
        public RoomController()
        {
            requestCode = RequestCode.Room;
        }

        /// <summary>
        /// 配對
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack Pair(Server server, Client client, MainPack pack)
        {
            server.Pair(client, pack);
            return null;
        }

        /// <summary>
        /// 離開房間
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack ExitRoom(Server server, Client client, MainPack pack)
        {
            server.ExitRoom(client, pack);
            return null;
        }
    }
}
