using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using MySql.Data.MySqlClient;
using GhostDrawProtobuf;
using GhostDrawServer.SqlData;
using GhostDrawServer.Tools;

namespace GhostDrawServer.Servers
{
    class Client
    {
        private const string connStr = "database=ghostdraw;Data Source=localhost;user=root;password=@Wu19918001;pooling=false;charset=utf8;port=3306";

        private Socket socket;
        private Server server;
        private Message message;

        private MySqlConnection mySqlConnection;
        public MySqlConnection GetMySqlConnection { get { return mySqlConnection; } }

        private MySqlManager mySqlManager;
        public MySqlManager GetMySql { get { return mySqlManager; } }

        public Room CurrRoom { get; set; }

        public class UserInfoData
        {
            public string GoogleId { get; set; }            //Google帳號ID
            public string NickName { get; set; }            //暱稱
            public string ImgUrl { get; set; }              //頭像Url

            public bool readyState { get; set; }            //遊戲準備狀態
            public List<int> poker { get; set; }            //手牌
        }
        public UserInfoData UserInfo { get; set; }

        public Client(Socket socket, Server server)
        {
            message = new Message();
            UserInfo = new UserInfoData();
            mySqlManager = new MySqlManager();

            mySqlConnection = new MySqlConnection(connStr);
            mySqlConnection.Open();

            this.server = server;
            this.socket = socket;

            //開始接收消息
            StartReceive();
        }

        /// <summary>
        /// 開始接收消息
        /// </summary>
        void StartReceive()
        {
            socket.BeginReceive(message.GetBuffer, message.GetStartIndex, message.GetRemSize, SocketFlags.None, ReceiveCallBack, null);
        }

        /// <summary>
        /// 接收消息CallBack
        /// </summary>
        void ReceiveCallBack(IAsyncResult iar)
        {
            try
            {
                if (socket == null || !socket.Connected) return;

                int len = socket.EndReceive(iar);
                if (len == 0)
                {
                    //關閉連接
                    Close();
                    return;
                }

                //解析Buffer
                message.ReadBuffer(len, HandleRequest);
                //再次開始接收消息
                StartReceive();
            }
            catch (Exception)
            {
                //關閉連接
                Close();
            }
        }

        /// <summary>
        /// 發送消息
        /// </summary>
        /// <param name="pack"></param>
        public void Send(MainPack pack)
        {
            Console.WriteLine($"給 {this.UserInfo.NickName}: 發送消息:{pack.ActionCode}");

            socket.Send(Message.PackData(pack));
        }

        /// <summary>
        /// 解析消息回調方法
        /// </summary>
        void HandleRequest(MainPack pack)
        {
            server.HandleRequest(pack, this);
        }

        /// <summary>
        /// 關閉連接
        /// </summary>
        void Close()
        {
            if (CurrRoom != null)
            {
                CurrRoom.Exit(server, this);
            }

            server.RemoveClient(this);
            socket.Close();
            mySqlConnection.Close();
            Console.WriteLine(this.UserInfo.NickName + ": 已斷開連接");
        }
    }
}
