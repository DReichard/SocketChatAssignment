using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketChatClient
{
    class ChatClient: IDisposable
    {
        private readonly SocketClient _socketClient;
        private string _userName;

        public ChatClient()
        {
            _socketClient = new SocketClient(4242);
        }
        public void Start()
        {
            _userName = GetUserName();
            while (true)
            {
                var message = Console.ReadLine();
                for (var i = 0; i < message.Length; i++) {
                    Console.Write("\b");
                }
                Console.Write("\b");
                _socketClient.SendSingle($"{_userName}: {message}").Wait();
            }
        }

        private string GetUserName()
        {
            Console.WriteLine("Enter Username:");
            return Console.ReadLine();
        }

        public void Dispose()
        {
            _socketClient.Dispose();
        }

    }


}
