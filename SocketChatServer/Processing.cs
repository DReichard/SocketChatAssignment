using System;
using System.Collections.Generic;
using System.Text;

namespace SocketChatServer
{
    class Processor
    {


        public Processor()
        {

        }

        public string Process(string input)
        {
            return $"{DateTime.Now} {input}";
        }
    }
}
