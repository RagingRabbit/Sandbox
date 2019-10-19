using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;

namespace Sandbox
{
    class Client
    {
        const int WIDTH = 800;
        const int HEIGHT = 600;
        const string TITLE = "";

        public Window window;
        public NetworkClient network;
        public int fps = 0;

        Level level;

        public Client()
        {
            window = new Window(WIDTH, HEIGHT, TITLE);

            network = new NetworkClient();

            window.setTitle("<" + network.username + "> | " + network.host);
            window.show();

            level = new Level();
        }

        public bool isRunning()
        {
            return !window.isClosed();
        }

        public void update(float dt)
        {
            network.update();
            window.update();
        }

        public void render()
        {
        }

        public void terminate()
        {
            network.terminate();
            window.terminate();
        }
    }
}
