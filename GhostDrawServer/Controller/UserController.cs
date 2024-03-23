using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GhostDrawProtobuf;
using GhostDrawServer.Servers;

namespace GhostDrawServer.Controller
{
    class UserController : BaseController
    {
        private string[] userDataNames = { "googleid", };
        string tableName = "userdata";

        public UserController()
        {
            requestCode = RequestCode.User;
        }

        /// <summary>
        /// 設定用戶訊息
        /// </summary>
        private void SetUserInfo(Client client, MainPack pack)
        {
            client.UserInfo.GoogleId = pack.LoginPack.Googleid;
            client.UserInfo.NickName = pack.LoginPack.NickName;
            client.UserInfo.ImgUrl = pack.LoginPack.ImgUrl;
        }

        /// <summary>
        /// 註冊
        /// </summary>
        /// <returns></returns>
        public MainPack Logon(Server servers, Client client, MainPack pack)
        {
            if (client.GetMySql.CheckData(client.GetMySqlConnection, tableName, new string[] { "googleid" }, new string[] { pack.LoginPack.Googleid }))
            {
                pack.ReturnCode = ReturnCode.Duplicated;
                return pack;
            }
            else
            {
                string[] InsertNames = new string[] { "googleid",};
                string[] values = new string[]
                {
                    pack.LoginPack.Googleid,    //Google帳號ID
                };

                if (client.GetMySql.InsertData(client.GetMySqlConnection, tableName, userDataNames, values))
                {
                    pack.ReturnCode = ReturnCode.Succeed;
                    SetUserInfo(client, pack);

                    Console.WriteLine($"{pack.LoginPack.Googleid}: 註冊成功!");
                }
                else
                {
                    pack.ReturnCode = ReturnCode.Fail;
                    Console.WriteLine($"{pack.LoginPack.Googleid}: 註冊失敗!!!");
                }

                return pack;
            }
        }

        /// <summary>
        /// 登入
        /// </summary>
        /// <returns></returns>
        public MainPack Login(Server server, Client client, MainPack pack)
        {
            string[] searchNames = new string[] { "googleid" };
            string[] dataValues = new string[] { pack.LoginPack.Googleid };
            if (server.GetClientList.Any(list => list.UserInfo.GoogleId == pack.LoginPack.Googleid))
            {
                pack.ReturnCode = ReturnCode.Duplicated;
                Console.WriteLine(pack.LoginPack.Googleid + ": 重複登入!!!");
                return pack;
            }

            if (client.GetMySql.CheckData(client.GetMySqlConnection, tableName, searchNames, dataValues))
            {
                //登入
                Dictionary<string, string> dataDic = client.GetMySql.GetData(client.GetMySqlConnection, tableName, "googleid", pack.LoginPack.Googleid, userDataNames);
                pack.ReturnCode = ReturnCode.Succeed;

                SetUserInfo(client, pack);
                Console.WriteLine($"{pack.LoginPack.Googleid}: 登入!");
            }
            else
            {
                return Logon(server, client, pack);
            }

            return pack;
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        public MainPack Logout(Server server, Client client, MainPack pack)
        {
            Console.WriteLine(client.UserInfo.NickName + ": 用戶登出");
            server.RemoveClient(client);

            pack.ReturnCode = ReturnCode.Succeed;
            return pack;
        }
    }
}
