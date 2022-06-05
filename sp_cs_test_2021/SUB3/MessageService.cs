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

        public bool handleMsgSvcCmd(String cmd, String queueName, out MessageEntity msgEnt, String param = null)
        {
            bool bRet = true;
            msgEnt = null;

            switch (cmd)
            {
                case "CREATE":
                    if (!dicMsgQueues.ContainsKey(queueName))
                    {
                        dicMsgQueues.Add(queueName, new FixedQueue(queueName, Int32.Parse(param)));
                        bRet = true;
                    }
                    else
                    {
                        Console.WriteLine("Queue Exist");
                        bRet = false;
                    }
                    break;
                case "SEND":
                    if (dicMsgQueues.ContainsKey(queueName))
                    {
                        bRet = dicMsgQueues[queueName].Enqueue(param);
                    }
                    break;
                case "RECEIVE":
                    if (dicMsgQueues.ContainsKey(queueName))
                    {
                        msgEnt = dicMsgQueues[queueName].Dequeue();
                        if (msgEnt != null)
                        {
                            //Console.WriteLine("[handleMsgSvcCmd/RECEIVE]msgEnt.msg:{0}, msgEntmsgId:{1}", msgEnt.msg, msgEnt.msgId);

                            bRet = true;
                        } else
                        {
                            //Console.WriteLine(msg);
                            bRet = false;
                        }
                    }
                    break;
                case "ACK":
                    if (dicMsgQueues.ContainsKey(queueName))
                    {
                        bRet = dicMsgQueues[queueName].Ack(param);
                    }
                    break;
                case "FAIL":
                    if (dicMsgQueues.ContainsKey(queueName))
                    {
                        bRet = dicMsgQueues[queueName].Fail(param);
                    }
                    break;
            }

            //Console.WriteLine("[handleMsgSvcCmd]cmd:{0}, queueName:{1}, param:{2}, bRet:{3}", cmd, queueName, param, bRet);

            return bRet;
        }
    }
}
