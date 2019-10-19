using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Steamworks;

namespace Sandbox
{
    class Program
    {
        static long getNanos()
        {
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }

        static void Main(string[] args)
        {
            Client client = new Client();

            long lastTick = getNanos();
            long lastSecond = getNanos();
            long delta = 0;
            int frames = 0;

            while (client.isRunning())
            {
                long now = getNanos();
                delta = now - lastTick;
                lastTick = now;
                if (now - lastSecond >= 1e9)
                {
                    client.fps = frames;
                    frames = 0;
                    lastSecond = now;
                }

                client.update(delta / 1e9f);
                client.render();
                frames++;
            }

            client.terminate();
            Console.ReadKey();
        }
    }
}
