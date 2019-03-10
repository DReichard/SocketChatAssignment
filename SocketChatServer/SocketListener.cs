using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketListener
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
        
        public async Task Run(int port, int backlogSize, CancellationToken token)
        {
            _socket = await EstablishEndpoint(port, backlogSize);
            Console.WriteLine($"Listening at {port}");
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Dispose();
                    return;
                }
                allDone.Reset();
                _socket.BeginAccept(new AsyncCallback(AcceptCallback), _socket);
                allDone.WaitOne();
            }
        }

        public void AcceptCallback(IAsyncResult asyncResult)
        {
            allDone.Set();
            var listener = (Socket)asyncResult.AsyncState;
            var handler = listener.EndAccept(asyncResult);
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
            var state = (StateObject)ar.AsyncState;
            var handler = state.WorkSocket;
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.Sb.Append(Encoding.ASCII.GetString(
                    state.Buffer, 0, bytesRead));
                content = state.Sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    Console.WriteLine($"{DateTime.Now.TimeOfDay} {content.Replace("<EOF>", "")}");
                    var res = _processingCallback(content);
                    Send(handler, res, SendCallback, state);
                }
                else
                { 
                    handler.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var handler = ((StateObject)ar.AsyncState).WorkSocket;
                var bytesSent = handler.EndSend(ar);

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
            var byteData = Encoding.ASCII.GetBytes(data);
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

