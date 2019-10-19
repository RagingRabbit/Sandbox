using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Sandbox
{
    class Packet : ISerializable
    {
        public int type;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }

    class ConnectPacket : Packet
    {

    }

    class DisconnectPacket : Packet
    {

    }
}
