using System;

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
                    mc.Run("test").Wait();
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
