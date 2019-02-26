using System;

namespace SocketChatClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var mc = new MainControl())
                {
                    mc.Run(4242).Wait();
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
