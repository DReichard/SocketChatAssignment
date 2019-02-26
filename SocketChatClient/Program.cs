using System;
using Common;
namespace SocketChatClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var mc = new SocketClient(4242))
                {
                    mc.Send("test").Wait();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
