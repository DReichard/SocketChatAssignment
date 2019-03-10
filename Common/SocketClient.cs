using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public delegate string ProcessingCallback(string input);

    public class SocketClient : IDisposable
    {
        private ProcessingCallback _processingCallback;
        private Socket _socket;
        private readonly int _port;
        private Dictionary<StateObject, string> _responses;
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        public SocketClient(int port, Func<string, string> callback)
        {
            _processingCallback = new ProcessingCallback(callback);
            _responses = new Dictionary<StateObject, string>();
            _port = port;
        }

        public async Task<string> SendSingle(string message)
        {
            _socket = await EstablishEndpoint(_port);
            var stateObj = new StateObject
            {
                WorkSocket = _socket
            };
            sendDone.Reset();
            Send(_socket, message + "<EOF>", SendCallback, stateObj);
            sendDone.WaitOne();
            receiveDone.Reset();
            Receive(_socket, stateObj);
            receiveDone.WaitOne();
            var res = _responses[stateObj];

            Console.WriteLine(res.Replace("<EOF>", ""));

            _responses.Remove(stateObj);
            return res;
        }

        private void Send(Socket socket, string data, Action<IAsyncResult> sendCallback, StateObject stateObject)
        {
            var byteData = Encoding.ASCII.GetBytes(data);
            socket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(sendCallback), stateObject);
        }

        private Task<Socket> EstablishEndpoint(int port)
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo?.AddressList?.FirstOrDefault() ??
                throw new InvalidOperationException("IP address not found");
            var remoteEndPoint = new IPEndPoint(ipAddress, port);
            var socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                connectDone.Reset();
                socket.BeginConnect(remoteEndPoint,
                     new AsyncCallback(ConnectCallback), socket);
                connectDone.WaitOne();
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

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var client = ((StateObject)ar.AsyncState).WorkSocket;
                var bytesSent = client.EndSend(ar);
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Receive(Socket client, StateObject state)
        {
            try
            {
                state.WorkSocket = client;
                client.BeginReceive(state.Buffer, 0, state.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            { 
                var state = (StateObject)ar.AsyncState;
                var client = state.WorkSocket;
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                { 
                    state.Sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));
                    client.BeginReceive(state.Buffer, 0, state.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    if (state.Sb.Length > 1)
                    {
                        _responses.Add(state, state.Sb.ToString());
                    }
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Dispose()
        {
            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Close();
            _socket?.Dispose();
        }
    }
}
