using Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SocketChatClient
{
    public class ChatClient: IDisposable
    {
        private readonly SocketClient _socketClient;
        private string _userName;

        public ChatClient(int port, Func<string, string> callback)
        {
            _socketClient = new SocketClient(port, callback);
            Console.WriteLine($"Sending to {port}");
        }
        public void Start()
        {
            Task.Delay(1000).Wait();
            _userName = GetUserName();
            while (true)
            {
                var message = Console.ReadLine();
                var messageEncrypted = EncryptionProvider.Encrypt($"{_userName}: {message}<EOF>", "HardcodedKey");
                //message = Encoding.UTF8.GetString(messageEncrypted);
                ClearCurrentConsoleLine();

                _socketClient.SendSingle(messageEncrypted).Wait();
            }
        }

        private string GetUserName()
        {
            Console.WriteLine("Enter Username:");
            return Console.ReadLine();
        }

        private static void ClearCurrentConsoleLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            var currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public void Dispose()
        {
            _socketClient.Dispose();
        }

    }


}
