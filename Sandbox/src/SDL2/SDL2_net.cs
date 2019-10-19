#region Using Statements
using System;
using System.Runtime.InteropServices;
#endregion

namespace SDL2
{
    public static class SDL_net
    {
        #region SDL2# Variables

        /* Used by DllImport to load the native library. */
        private const string nativeLibName = "SDL2_net";

        #endregion

        #region SDL_net.h

        [StructLayout(LayoutKind.Sequential)]
        public struct IPaddress
        {
            public UInt32 host;
            public UInt16 port;
        }

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDLNet_Init();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDLNet_Quit();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDLNet_ResolveHost(out IPaddress address, string host, UInt16 port);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDLNet_TCP_Open(ref IPaddress ip);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDLNet_TCP_Close(IntPtr sock);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDLNet_TCP_Accept(IntPtr server);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IPaddress SDLNet_TCP_GetPeerAddress(IntPtr sock);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDLNet_TCP_Send(IntPtr sock, IntPtr data, int len);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDLNet_TCP_Recv(IntPtr sock, IntPtr data, int maxlen);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDLNet_AllocSocketSet(int maxsockets);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDLNet_FreeSocketSet(IntPtr set);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDLNet_AddSocket(IntPtr set, IntPtr sock);

        public static int SDLNet_TCP_AddSocket(IntPtr set, IntPtr sock)
        {
            return SDLNet_AddSocket(set, sock);
        }

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDLNet_CheckSockets(IntPtr set, UInt32 timeout);

        public static bool SDLNet_SocketReady(IntPtr sock)
        {
            return (sock != IntPtr.Zero) && (Marshal.ReadInt32(sock) != 0);
        }

        #endregion
    }
}
