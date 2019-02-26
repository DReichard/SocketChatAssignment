using System;

namespace SocketChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var mc = new MainControl())
                {
                    mc.Run().Wait();
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
