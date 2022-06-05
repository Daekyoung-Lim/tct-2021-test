using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SP_TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            MessageHttpServer msgHttpServer = new MessageHttpServer();
            Thread msgHttpServThread = new Thread(msgHttpServer.doHttpServer);
            msgHttpServThread.Start();

            msgHttpServThread.Join();
        }
    }
}
