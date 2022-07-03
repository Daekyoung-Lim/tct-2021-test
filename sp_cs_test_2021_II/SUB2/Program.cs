using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            MessageService msgSvc = new MessageService();
            CustomQueue que = null;

            while (true)
            {
                String cmdLine = Console.ReadLine();

                String[] arrCmd = cmdLine.Split(' ');

                switch (arrCmd[0])
                {
                    case "CREATE":
                        msgSvc.createQueue(arrCmd[1], Int32.Parse(arrCmd[2]));
                        break;
                    case "SEND":
                        que = msgSvc.findQueue(arrCmd[1]);
                        que.Enqueue(new Message(arrCmd[2]));
                        break;
                    case "RECEIVE":
                        que = msgSvc.findQueue(arrCmd[1]);
                        Message msg = que.Dequeue();
                        if (msg != null)
                        {
                            Console.WriteLine("{0}", msg.mMsgContents);
                        }
                        break;
                }
            }
        }
    }
}
