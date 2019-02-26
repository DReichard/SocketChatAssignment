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
                    mc.Run(4242, 50000).Wait();
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
