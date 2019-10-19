using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    class NetworkServer
    {
        Network net;

        public NetworkServer()
        {
            net = new Network(onPacketReceived);
            net.host(10, onClientConnect);
        }

        void onClientConnect(Connection connection)
        {
            Console.WriteLine("[SERVER] Client " + connection.id + " from " + Connection.ToString(connection.getAddress()) + " connected");
        }

        void onPacketReceived(string msg, long time, Connection client)
        {
            Console.WriteLine("[SERVER] Received message " + msg);
            net.sendMessage("world", client);
        }

        public void update()
        {
            net.update();
        }

        public void terminate()
        {
            net.terminate();
        }
    }
}
