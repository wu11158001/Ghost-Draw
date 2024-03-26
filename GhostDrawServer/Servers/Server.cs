using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using GhostDrawProtobuf;
using GhostDrawServer.Controller;

namespace GhostDrawServer.Servers
{
    class Server
    {
        private Socket socket = null;
        private ControllerManager controllerManager;

        //存放所有連接的客戶端
        private List<Client> clientList = new List<Client>();
        public List<Client> GetClientList { get { return clientList; } }

        //存放所有房間
        private List<Room> roomList = new List<Room>();
        public int GetRoomCount { get { return roomList.Count; } }

        public Server(int port)
        {
            controllerManager = new ControllerManager(this);

            //Socket初始化
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //綁定
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            //監聽
            socket.Listen(0);
            //開始接收
            StartAccect();
        }

        /// <summary>
        /// 開始接收
        /// </summary>
        void StartAccect()
        {
            socket.BeginAccept(AccectCallBack, null);
        }
        /// <summary>
        /// 接收CallBack
        /// </summary>
        void AccectCallBack(IAsyncResult iar)
        {
            Console.WriteLine("添加客戶");
            Socket client = socket.EndAccept(iar);
            clientList.Add(new Client(client, this));
            StartAccect();
        }

        /// <summary>
        /// 處理請求
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="client"></param>
        public void HandleRequest(MainPack pack, Client client)
        {
            controllerManager.HandleRequest(pack, client);
        }

        /// <summary>
        /// 移除客戶端
        /// </summary>
        /// <param name="client"></param>
        public void RemoveClient(Client client)
        {
            clientList.Remove(client);
        }

        /// <summary>
        /// 配對
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public void Pair(Client client, MainPack pack)
        {
            foreach (Room r in roomList)
            {
                if (r.GetRoomInfo.CurrPeople < 2 && r.GetRoomInfo.RoomState == RoomState.Wait)
                {                  
                    //配對成功                    
                    r.GetRoomInfo.RoomState = RoomState.InProgress;
                    r.Join(client);                 
                    return;
                }
            }

            //無法配對創建房間
            RoomPack roomPack = new RoomPack();
            Room room = new Room(this, client, roomPack);
            roomList.Add(room);
            Console.WriteLine($"{client.UserInfo.NickName}:等待配對...");
        }

        /// <summary>
        /// 離開房間
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public void ExitRoom(Client client, MainPack pack)
        {
            if (client == null) return;

            if (client.CurrRoom == null)
            {
                Console.WriteLine($"{client.UserInfo.NickName}:離開房間錯誤!!!");
                return;
            }

            client.CurrRoom.Exit(this, client);
        }

        /// <summary>
        /// 移除房間
        /// </summary>
        /// <param name="room"></param>
        public void RemoveRoom(Room room)
        {
            roomList.Remove(room);
        }
    }
}
