using SocketListener;
using System;

namespace SocketChatApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new Host(Convert.ToInt32(args[0]), Convert.ToInt32(args[1])))
            {
                host.Start();
                Console.CancelKeyPress += delegate {
                    host.Stop();
                };
            }
        }
    }
}
