using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace Sandbox
{
    class Window
    {
        IntPtr window = IntPtr.Zero;
        bool running = false;

        public Window(int width, int height, string title)
        {
            if (SDL_Init(SDL_INIT_EVERYTHING) == 0)
            {
                window = SDL_CreateWindow(title, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, width, height, SDL_WindowFlags.SDL_WINDOW_HIDDEN);
                running = true;
            }
        }

        public void setTitle(string title)
        {
            SDL_SetWindowTitle(window, title);
        }

        public void show()
        {
            SDL_ShowWindow(window);
        }

        public bool isClosed()
        {
            return !running;
        }

        public void update()
        {
            SDL_Event e;
            SDL_PollEvent(out e);

            switch (e.type)
            {
                case SDL_EventType.SDL_QUIT: running = false; break;
                default: break;
            }
        }

        public void terminate()
        {
            SDL_DestroyWindow(window);
            Console.WriteLine("Window closed");
        }
    }
}
