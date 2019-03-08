using System;
using System.Threading;
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
                    while (true)
                    {
                        try
                        {
                            mc.SendSingle("Test B").Wait();
                            Thread.Sleep(1000);
                        } 
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
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
