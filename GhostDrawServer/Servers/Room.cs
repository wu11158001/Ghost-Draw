using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GhostDrawProtobuf;
using Google.Protobuf.Collections;

namespace GhostDrawServer.Servers
{
    class Room
    {
        private Server server;

        private const int jokerNum = 101;

        private int currActionUser;

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
            Console.WriteLine($"{client.UserInfo.NickName}:加入房間，遊戲開始。");

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

            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.ExitRoom;

            Broadcast(client, pack);
        }

        /// <summary>
        /// 判斷準備狀態
        /// </summary>
        public async void JudgeReadyState()
        {
            //所有玩家都已準備完成
            if (clientList.All(x => x.UserInfo.readyState))
            {
                SendInitUsersPoker();

                await Task.Delay(2000);

                SendRemovedPokers();

                await Task.Delay(1000);

                currActionUser = new Random().Next(0, clientList.Count);
                SendActionUser();
            }
        }

        /// <summary>
        /// 發送行動玩家
        /// </summary>
        private void SendActionUser()
        {
            currActionUser = currActionUser + 1 >= clientList.Count ? 0 : currActionUser + 1;

            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.ActionUser;

            GamePack gamePack = new GamePack();
            gamePack.ActionUserId = clientList[currActionUser].UserInfo.GoogleId;
            pack.GamePack = gamePack;

            Broadcast(null, pack);
        }

        /// <summary>
        /// 獲取對手Index
        /// </summary>
        /// <param name="localUser"></param>
        /// <returns></returns>
        private int GetMatchUserIndex(int localUser)
        {
            int match = localUser + 1;
            if (match >= clientList.Count)
            {
                match = 0;
            }

            return match;
        }

        /// <summary>
        /// 發送初始玩家手牌
        /// </summary>
        private void SendInitUsersPoker()
        {
            //52張牌
            List<int> pokerList = new List<int>();
            for (int i = 0; i < 52; i++)
            {
                pokerList.Add(i);
            }

            //玩家手牌
            int pokerCount = pokerList.Count / clientList.Count;
            for (int i = 0; i < clientList.Count; i++)
            {
                clientList[i].UserInfo.poker = new List<int>();
                for (int j = 0; j < pokerCount; j++)
                {
                    int index = new Random().Next(0, pokerList.Count);                    
                    clientList[i].UserInfo.poker.Add(pokerList[index]);
                    pokerList.RemoveAt(index);
                }
            }

            //設定鬼牌玩家
            int ghostUser = new Random().Next(0, clientList.Count);
            clientList[ghostUser].UserInfo.poker.Add(jokerNum);
            Shuffle(clientList[ghostUser].UserInfo.poker);

            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.InitGameInfo;
            GamePack gamePack = new GamePack();
            for (int i = 0; i < clientList.Count; i++)
            {
                UserInfoPack userInfoPack = new UserInfoPack();
                LoginPack loginPack = new LoginPack();
                loginPack.Googleid = clientList[i].UserInfo.GoogleId;
                loginPack.NickName = clientList[i].UserInfo.NickName;
                loginPack.ImgUrl = clientList[i].UserInfo.ImgUrl;
                userInfoPack.LoginPack = loginPack;

                pack.UserInfoPack.Add(userInfoPack);

                IntList intList = new IntList();
                intList.Values.AddRange(clientList[i].UserInfo.poker);
                gamePack.PokerDic.Add(clientList[i].UserInfo.GoogleId, intList);
            }
            pack.GamePack = gamePack;

            Console.WriteLine("發送遊戲初始訊息");
            Broadcast(null, pack);
        }

        /// <summary>
        /// 用戶洗牌
        /// </summary>
        /// <param name="client"></param>
        public void UserShuffle(Client client)
        {
            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.HandPokers;

            GamePack gamePack = new GamePack();
            Shuffle(client.UserInfo.poker);
            IntList intList = new IntList();
            intList.Values.AddRange(client.UserInfo.poker);
            gamePack.PokerDic.Add(client.UserInfo.GoogleId, intList);
            pack.GamePack = gamePack;

            Broadcast(null, pack);
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        /// <param name="pokers"></param>
        private void Shuffle(List<int> pokers)
        {
            Random rng = new Random();
            int n = pokers.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                int value = pokers[k];
                pokers[k] = pokers[n];
                pokers[n] = value;
            }
        }

        /// <summary>
        /// 發送移除相同牌後的手牌
        /// </summary>
        private void SendRemovedPokers()
        {
            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.HandPokers;

            GamePack gamePack = new GamePack();
            for (int i = 0; i < clientList.Count; i++)
            {
                clientList[i].UserInfo.poker = SamePokerRemove(clientList[i].UserInfo.poker);
                IntList intList = new IntList();
                intList.Values.AddRange(clientList[i].UserInfo.poker);
                gamePack.PokerDic.Add(clientList[i].UserInfo.GoogleId, intList);
            }
            pack.GamePack = gamePack;

            Broadcast(null, pack);
        }

        /// <summary>
        /// 移除相同的牌
        /// </summary>
        /// <param name="pokers"></param>
        private List<int> SamePokerRemove(List<int> pokers)
        {
            Dictionary<int, int> countDic = new Dictionary<int, int>();
            foreach (var poker in pokers)
            {
                int key = poker != jokerNum ? poker % 13 : jokerNum;
                if (countDic.ContainsKey(key))
                {
                    countDic[key]++;
                }
                else
                {
                    countDic.Add(key, 1);
                }
            }

            List<int> resut = new List<int>();
            foreach (var poker in pokers)
            {
                int key = poker != jokerNum ? poker % 13 : jokerNum;
                if (countDic.ContainsKey(key))
                {
                    if (countDic[key] == 3)
                    {
                        countDic[key] = 2;
                        resut.Add(poker);
                    }
                    else if (countDic[key] == 1)
                    {
                        resut.Add(poker);
                    }
                }
            }

            return resut;
        }

        /// <summary>
        /// 抽牌
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        public async void DrawCard(Client client, MainPack pack)
        {
            int drawUser = 0;
            for (int i = 0; i < clientList.Count; i++)
            {
                if (client != clientList[i])
                {
                    drawUser = i;
                    break;
                }
            }

            int draw = clientList[drawUser].UserInfo.poker[pack.GamePack.SelectPockerIndex];
            clientList[drawUser].UserInfo.poker.RemoveAt(pack.GamePack.SelectPockerIndex);
            clientList[GetMatchUserIndex(drawUser)].UserInfo.poker.Add(draw);

            pack.GamePack.DrawPoker = draw;
            pack.GamePack.ActionUserId = client.UserInfo.GoogleId;
            Broadcast(null, pack);

            await Task.Delay(2800);

            SendRemovedPokers();

            await Task.Delay(1000);

            //判斷贏家
            for (int i = 0; i < clientList.Count; i++)
            {
                if (clientList[i].UserInfo.poker.Count == 0)
                {
                    pack = new MainPack();
                    pack.ActionCode = ActionCode.GameResult;

                    GamePack gamePack = new GamePack();
                    gamePack.WinnerId = clientList[i].UserInfo.GoogleId;
                    pack.GamePack = gamePack;

                    Broadcast(null, pack);

                    Console.WriteLine($"遊戲結束，關閉房間!");
                    server.RemoveRoom(this);
                    return;
                }
            }

            SendActionUser();
        }
    }
}
