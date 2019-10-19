using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Steamworks;
using static Sandbox.MemoryUtils;
using static SDL2.SDL;
using static SDL2.SDL_net;

namespace Sandbox
{
    class NetworkClient
    {
        const int PORT = 4444;
        const int MAX_PACKET_SIZE = 8192;

        public Network net;
        public NetworkServer hostServer;
        public string host;
        public string username;

        public NetworkClient()
        {
            net = new Network(onPacketReceived);

            Console.WriteLine("Enter username:");
            username = Console.ReadLine();

            Console.WriteLine("Host? Y/N");
            bool isHost = Console.ReadLine().ToLower() == "y";
            if (isHost)
            {
                hostServer = new NetworkServer();
                net.connect("localhost", username);
                net.sendMessage("hello");
            }
            else
            {
                Console.WriteLine("Enter host address:");
                host = Console.ReadLine();
                net.connect(host, username);
            }
        }

        public void update()
        {
            if (hostServer != null) hostServer.update();
            net.update();
        }

        void onPacketReceived(string msg, long time, Connection connection)
        {
            Console.WriteLine("[CLIENT] Received message " + msg);
        }

        public void terminate()
        {
            net.terminate();
        }
    }
}
