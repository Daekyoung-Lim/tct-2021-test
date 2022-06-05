using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_TEST
{
    class MessageService
    {
        Dictionary<string, FixedQueue> dicMsgQueues;

        public MessageService()
        {
            dicMsgQueues = new Dictionary<string, FixedQueue>();
        }

        public void handleMsgSvcCmd()
        {
            while (true)
            {
                string[] cmdArr = Console.ReadLine().Split();

                switch (cmdArr[0])
                {
                    case "CREATE":
                        if (!dicMsgQueues.ContainsKey(cmdArr[1]))
                        {
                            dicMsgQueues.Add(cmdArr[1], new FixedQueue(Int32.Parse(cmdArr[2])));
                        } else
                        {
                            Console.WriteLine("Queue Exist");
                        }
                        break;
                    case "SEND":
                        if (dicMsgQueues.ContainsKey(cmdArr[1]))
                        {
                            if (!dicMsgQueues[cmdArr[1]].Enqueue(cmdArr[2]))
                            {
                                Console.WriteLine("Queue Full");
                            }
                        }
                        break;
                    case "RECEIVE":
                        if (dicMsgQueues.ContainsKey(cmdArr[1]))
                        {
                            string msg = dicMsgQueues[cmdArr[1]].Dequeue();
                            if (msg != null)
                            {
                                Console.WriteLine(msg);
                            }
                        }
                        break;
                }
            }
        }        
    }
}
