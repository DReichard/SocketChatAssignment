using System;
using System.Net.Sockets;
using System.Text;

namespace Common
{
    public class StateObject
    {
        public Socket WorkSocket { get; set; }
        public int BufferSize { get; set; } 
        public byte[] Buffer { get; set; }
        public StringBuilder Sb { get; set; }

        public StateObject(int bufferSize = 1024)
        {
            BufferSize = bufferSize;
            Buffer = new byte[BufferSize];
            Sb = new StringBuilder();
        }
    }
}
