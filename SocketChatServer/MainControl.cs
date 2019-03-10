using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketChatServer
{
    public delegate string ProcessingCallback(string input);

    public class SocketListener: IDisposable
    {
        private Socket _socket;
        private ProcessingCallback _processingCallback;
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public SocketListener(Func<string, string> callback)
        {
            _processingCallback = new ProcessingCallback(callback);
        }
        
        public async Task Run(int port, int backlogSize)
        {
            _socket = await EstablishEndpoint(port, backlogSize);
            var test = new ManualResetEvent(false);
            while (true)
            {
                allDone.Reset();
                Console.WriteLine("Waiting for a connection...");
                _socket.BeginAccept(new AsyncCallback(AcceptCallback), _socket);
                allDone.WaitOne();
            }
        }

        public void AcceptCallback(IAsyncResult asyncResult)
        {
            allDone.Set();
            var listener = (Socket)asyncResult.AsyncState;
            var handler = listener.EndAccept(asyncResult);

            // Create the state object.  
            var state = new StateObject
            {
                WorkSocket = handler
            };
            handler.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            var content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            var state = (StateObject)ar.AsyncState;
            var handler = state.WorkSocket;

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.Sb.Append(Encoding.ASCII.GetString(
                    state.Buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read   
                // more data.  
                content = state.Sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the   
                    // client. Display it on the console.  
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    // Echo the data back to the client. 
                    var res = _processingCallback(content);
                    Send(handler, res, SendCallback, state);
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var handler = ((StateObject)ar.AsyncState).WorkSocket;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket socket, string data, Action<IAsyncResult> sendCallback, StateObject stateObject)
        {
            // Convert the string data to byte data using ASCII encoding.  
            var byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            socket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(sendCallback), stateObject);
        }

        private Task<Socket> EstablishEndpoint(int port, int backLogSize)
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo?.AddressList?.FirstOrDefault() ??
                throw new InvalidOperationException("IP address not found");
            var localEndPoint = new IPEndPoint(ipAddress, port);
            var socket = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(localEndPoint);
                socket.Listen(backLogSize);
                return Task.FromResult(socket);
            }
            catch (Exception)
            {
                if (socket.IsBound)
                {
                    socket.Close();
                }
                throw;
            }
        }

        public void Dispose()
        {
            _socket?.Close();
            _socket?.Dispose();
        }
    }
}

