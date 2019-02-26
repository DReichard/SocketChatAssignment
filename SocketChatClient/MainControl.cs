using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketChatClient
{
    public class SocketClient : IDisposable
    {
        private Socket _socket;
        private Dictionary<StateObject, string> _responses;

        private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        public SocketClient(int port)
        {
            _responses = new Dictionary<StateObject, string>();
            _socket = EstablishEndpoint(port).Result;
        }

        public Task<string> Run(string message)
        {
            // Send test data to the remote device.
            var stateObj = new StateObject
            {
                WorkSocket = _socket
            };
            Utilities.Send(_socket, message + "<EOF>", SendCallback, stateObj);
            sendDone.WaitOne();

            // Receive the response from the remote device.  
            Receive(_socket, stateObj);
            receiveDone.WaitOne();

            // Write the response to the console. 
            var res = _responses[stateObj];
            Console.WriteLine("Response received : {0}", res);
            _responses.Remove(stateObj);
            return Task.FromResult(res);
        }

        private Task<Socket> EstablishEndpoint(int port)
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo?.AddressList?.FirstOrDefault() ??
                throw new InvalidOperationException("IP address not found");
            var remoteEndPoint = new IPEndPoint(ipAddress, port);
            var socket = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);
            try
            {
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
                // Retrieve the socket from the state object.  
                var client = ((StateObject)ar.AsyncState).WorkSocket;

                // Complete sending the data to the remote device.  
                var bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
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
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
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
                // Create the state object.  
                //StateObject state = new StateObject();
                state.WorkSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
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
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.WorkSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.Sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.Buffer, 0, state.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.Sb.Length > 1)
                    {
                        _responses.Add(state, state.Sb.ToString());
                    }
                    // Signal that all bytes have been received.  
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
