using System;
using GhostDrawServer.Servers;

namespace GhostDrawServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(8888);
            Console.WriteLine("Bowling 服務端啟動..");
            Console.Read();
        }
    }
}
