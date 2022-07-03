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
            MessageService msgSvc = new MessageService();

            ManageHttpServer mngHttpServer = new ManageHttpServer();
            Thread th = new Thread(mngHttpServer.DoHttpServer);

            th.Start(msgSvc);
            th.Join();
        }
    }
}
