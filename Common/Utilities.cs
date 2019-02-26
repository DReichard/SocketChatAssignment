using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Utilities
    {

        public static void Send(Socket socket, string data, Action<IAsyncResult> sendCallback, StateObject stateObject)
        {
            // Convert the string data to byte data using ASCII encoding.  
            var byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            socket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(sendCallback), stateObject);
        }
    }
}
