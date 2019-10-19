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
    public delegate void onPacketReceived_t(string msg, long time, Connection connection);
    public delegate void onClientConnect_t(Connection connection);

    public class Connection
    {
        public int id;
        public IntPtr socket;
        public long lastPing;
        public string name;

        public Connection(IntPtr socket, long lastPing, int id)
        {
            this.socket = socket;
            this.lastPing = lastPing;
            this.id = id;
        }

        public IPaddress getAddress()
        {
            return SDLNet_TCP_GetPeerAddress(socket);
        }

        public static string ToString(IPaddress ip)
        {
            return ip.port + ":"
                + (ip.host & 0xFF000000 >> 24) + "."
                + (ip.host & 0x00FF0000 >> 16) + "."
                + (ip.host & 0x0000FF00 >> 8) + "."
                + (ip.host & 0x000000FF);
        }
    }

    public class Network
    {
        const int PORT = 4444;
        const int MAX_PACKET_SIZE = 8192;
        const int TIMEOUT_DURATION = 5000;

        public IntPtr socket;
        public IntPtr socketset;

        bool server;
        IntPtr packetData;
        byte[] packetBuffer;
        int bufferPosition;
        long lastTick1;

        onPacketReceived_t onPacketReceived;

        // Client
        Connection serverConnection;

        // Server
        int uniqueId;
        int maxClients;
        List<Connection> clients;
        onClientConnect_t onClientConnect;

        public Network(onPacketReceived_t onPacketReceived)
        {
            this.onPacketReceived = onPacketReceived;

            clients = new List<Connection>();
        }

        public void connect(string host, string username)
        {
            this.server = false;

            SDLNet_Init();
            IPaddress hostAddr;
            if (SDLNet_ResolveHost(out hostAddr, host, PORT) == -1)
            {
                Console.WriteLine("[CLIENT] Unresolved hostname " + host);
                return;
            }
            socket = SDLNet_TCP_Open(ref hostAddr);
            if (socket == IntPtr.Zero)
            {
                Console.WriteLine("[CLIENT] Unable to connect to " + host);
                return;
            }
            socketset = SDLNet_AllocSocketSet(1);
            SDLNet_TCP_AddSocket(socketset, socket);
            packetData = Marshal.AllocHGlobal(MAX_PACKET_SIZE);
            packetBuffer = new byte[MAX_PACKET_SIZE];
            serverConnection = new Connection(socket, 0, 0);
            Console.WriteLine("[CLIENT] Successfully connected to " + host);
        }

        public void host(int maxClients, onClientConnect_t onClientConnect)
        {
            this.maxClients = maxClients;
            this.onClientConnect = onClientConnect;
            this.server = true;

            SDLNet_Init();
            IPaddress hostAddr;
            SDLNet_ResolveHost(out hostAddr, null, PORT);
            socketset = SDLNet_AllocSocketSet(maxClients);
            socket = SDLNet_TCP_Open(ref hostAddr);
            if (socket == IntPtr.Zero)
            {
                Console.WriteLine("[SERVER] Unable to start server");
                return;
            }
            packetData = Marshal.AllocHGlobal(MAX_PACKET_SIZE);
            packetBuffer = new byte[MAX_PACKET_SIZE];
            Console.WriteLine("[SERVER] Successfully hosted server on " + Connection.ToString(hostAddr));
        }

        public void sendMessage(string msg, IntPtr socket)
        {
            byte[] packetData = Encoding.ASCII.GetBytes(msg);
            IntPtr packetDataPtr = Marshal.AllocHGlobal(packetData.Length);
            Marshal.Copy(packetData, 0, packetDataPtr, packetData.Length);
            SDLNet_TCP_Send(socket, packetDataPtr, packetData.Length);
            Marshal.FreeHGlobal(packetDataPtr);
        }

        public void sendMessage(string msg)
        {
            sendMessage(msg, socket);
        }

        public void sendMessage(string msg, Connection connection)
        {
            sendMessage(msg, connection.socket);
        }

        public void broadcastMessage(string msg)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                sendMessage(msg, clients[i]);
            }
        }

        /*
        public void sendPacket<T>(T packet)
        {
            byte[] packetData = toByteArray<T>(packet);
            IntPtr packetDataPtr = Marshal.AllocHGlobal(packetData.Length);
            Marshal.Copy(packetData, 0, packetDataPtr, packetData.Length);
            SDLNet_TCP_Send(socket, packetDataPtr, packetData.Length);
            Marshal.FreeHGlobal(packetDataPtr);
        }
        */

        public void update()
        {
            if (server) updateServer();
            else updateClient();
        }

        void updateServer()
        {
            // Check for incoming connections
            IntPtr connectingClient = SDLNet_TCP_Accept(socket);
            if (connectingClient != IntPtr.Zero)
            {
                if (clients.Count < maxClients)
                {
                    SDLNet_TCP_AddSocket(socketset, connectingClient);
                    int connectionId = uniqueId++;
                    clients.Add(new Connection(connectingClient, getNanos(), connectionId));
                    onClientConnect(clients[clients.Count - 1]);
                }
            }

            // Check for packets
            if (SDLNet_CheckSockets(socketset, 0) > 0)
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    if (SDLNet_SocketReady(clients[i].socket))
                    {
                        long time = getNanos();
                        int length = SDLNet_TCP_Recv(clients[i].socket, packetData, MAX_PACKET_SIZE);
                        if (length > 0)
                        {
                            clients[i].lastPing = time;
                            Marshal.Copy(packetData, packetBuffer, 0, length);

                            byte[] packetBytes = new byte[length];
                            Array.Copy(packetBuffer, 0, packetBytes, 0, length);
                            onPacketReceived(Encoding.ASCII.GetString(packetBytes), time, clients[i]);
                        }
                    }
                }
            }

            // Disconnect timeouted connectionss
            for (int i = 0; i < clients.Count; i++)
            {
                long now = getNanos();
                if ((now - clients[i].lastPing) / 1000000 > TIMEOUT_DURATION)
                {
                    Console.WriteLine("[SERVER] Client " + clients[i].id + " at " + Connection.ToString(clients[i].getAddress()) + " timeouted after " + TIMEOUT_DURATION + "ms");
                    disconnectClient(clients[i]);
                    i--;
                }
            }
        }

        void updateClient()
        {
            long now = getNanos();
            if (now - lastTick1 >= 1e9)
            {
                tick1();
                lastTick1 = now;
            }

            // Check for packets
            if (SDLNet_CheckSockets(socketset, 0) > 0 && SDLNet_SocketReady(socket))
            {
                long time = getNanos();
                int length = SDLNet_TCP_Recv(socket, packetData, MAX_PACKET_SIZE);
                if (length > 0)
                {
                    Marshal.Copy(packetData, packetBuffer, 0, length);

                    byte[] packetBytes = new byte[length];
                    Array.Copy(packetBuffer, 0, packetBytes, 0, length);
                    onPacketReceived(Encoding.ASCII.GetString(packetBytes), time, serverConnection);
                }
                /*
                int length = SDLNet_TCP_Recv(socket, new IntPtr(packetData.ToInt32() + bufferPosition), MAX_PACKET_SIZE);
                if (length > 0)
                {
                    bufferPosition += length;
                    int nextPacketSize = Marshal.ReadInt32(packetData) + 4;
                    while (bufferPosition >= nextPacketSize)
                    {
                        int size = Marshal.ReadInt32(packetData);
                        Marshal.Copy(packetData, packetBuffer, 0, size);

                        byte[] packetBytes = new byte[size];
                        Array.Copy(packetBuffer, 0, packetBytes, 0, size);
                        onPacketReceived(Encoding.ASCII.GetString(packetBytes), time, serverConnection);

                        CopyMemory(packetData, new IntPtr(packetData.ToInt32() + nextPacketSize), (uint)(bufferPosition - nextPacketSize));
                        bufferPosition -= nextPacketSize;

                        nextPacketSize = Marshal.ReadInt32(packetData) + 4;
                    }
                }
                */
            }
        }

        void disconnectClient(Connection connection)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].socket == connection.socket)
                {
                    IntPtr socket = clients[i].socket;
                    SDLNet_TCP_Close(socket);
                    clients.RemoveAt(i);
                    break;
                }
            }
        }

        long getNanos()
        {
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }

        void tick1()
        {
            // TODO ping
        }

        /*
    void onPacketReceived(byte[] data, int length, long time)
    {
        Packet packet = fromByteArray<Packet>(packetBytes);

        int packetType = packet.type;
        Console.WriteLine("Received packet with type " + packetType);
    }
        */

        public void terminate()
        {
            //sendPacket(new DisconnectPacket());

            Marshal.FreeHGlobal(packetData);
            SDLNet_TCP_Close(socket);
            SDLNet_FreeSocketSet(socketset);
            SDLNet_Quit();
        }
    }
}
