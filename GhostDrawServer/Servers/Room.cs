using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GhostDrawProtobuf;

namespace GhostDrawServer.Servers
{
    class Room
    {
        private Server server;

        //房間內所有客戶端
        private List<Client> clientList;

        public Room(Server server, Client client, RoomPack pack)
        {
            this.server = server;
            roomInfo = pack;
            clientList = new List<Client>();
            clientList.Add(client);
            client.CurrRoom = this;
        }

        //房間訊息
        private RoomPack roomInfo;
        public RoomPack GetRoomInfo
        {
            get
            {
                roomInfo.CurrPeople = clientList.Count;
                return roomInfo;
            }
        }

        /// <summary>
        /// 廣播
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        public void Broadcast(Client client, MainPack pack)
        {
            pack.SendModeCode = SendModeCode.RoomBroadcast;
            foreach (Client c in clientList)
            {
                //排除的cliemt
                if (c.Equals(client)) continue;

                c.Send(pack);
            }
        }

        /// <summary>
        /// 添加客戶端
        /// </summary>
        /// <param name="client"></param>
        public void Join(Client client)
        { 
            clientList.Add(client);
            client.CurrRoom = this;

            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.StartGame;

            Broadcast(null, pack);
        }

        /// <summary>
        /// 離開房間
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        public void Exit(Server server, Client client)
        {
            Console.WriteLine($"{client.UserInfo.NickName}:離開房間");

            clientList.Remove(client);

            //關閉房間
            if (clientList.Count == 0)
            {
                Console.WriteLine($"{client.UserInfo.NickName}:關閉房間!");
                server.RemoveRoom(this);
                return;
            }

            client.CurrRoom = null;

            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.ExitRoom;
            pack.SendModeCode = SendModeCode.RoomBroadcast;

            Broadcast(client, pack);
        }
    }
}
