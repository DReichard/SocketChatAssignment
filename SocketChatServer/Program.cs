using System;

namespace SocketChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var processor = new Processor();

                using (var mc = new SocketListener(processor.Process))
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
