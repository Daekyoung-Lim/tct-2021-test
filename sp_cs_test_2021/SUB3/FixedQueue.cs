using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SP_TEST
{
    class MessageEntity
    {
        public string msg;
        public string msgId;
        public string state;    //RECIVED(FAILED), SENT

        public MessageEntity(string msg, string msgId, string state)
        {
            this.msg = msg;
            this.msgId = msgId;
            this.state = state;
        }
    }

    class FixedQueue
    {
        static Mutex muQue = new Mutex();
        private string queId { get; set; }
        private int size { get; set; }
        private int pos { get; set; }
        private int cnt { get; set; }
        private int id { get; set; }
        private List<MessageEntity> listMsg;

        public FixedQueue(string queId, int size)
        {
            this.queId = queId;
            this.size = size;
            this.pos = 0;
            this.cnt = 0;
            this.id = 0;
            listMsg = new List<MessageEntity>();
        }

        public bool Enqueue(string msg)
        {
            bool bRet = false;
            string strId = "";

            if (cnt < size)
            {
                muQue.WaitOne();
                strId = queId + "_" + id.ToString();
                listMsg.Add(new MessageEntity(msg, strId, "RECEIVED"));
                id++;
                cnt++;
                muQue.ReleaseMutex();
                bRet =true;
            }

            /*
            Console.WriteLine("[Enqueue]size:{0}, pos:{1}, cnt:{2}, id:{3}, msg{4}, bRet{5}", 
                size, pos, cnt, strId, msg, bRet);
            */

            return bRet;
        }

        public MessageEntity Dequeue()
        {
            MessageEntity msgEnt = null;

            if (cnt > 0)
            {
                muQue.WaitOne();
                msgEnt = listMsg[pos];
                if (msgEnt.state == "RECEIVED")
                {
                    msgEnt.state = "SENT";
                    listMsg[pos] = msgEnt;
                    //update position
                    for (int idx = ++pos; idx < listMsg.Count; idx++)
                    {
                        if (listMsg[idx].state == "RECEIVED")
                        {
                            pos = idx;
                            break;
                        }
                    }
                    cnt--;  //update count
                } else
                {
                    //Console.WriteLine("[Dequeue]state mismatch in position:{0}!", pos);
                }
                muQue.ReleaseMutex();

                /*
                Console.WriteLine("[Dequeue]size:{0}, pos:{1}, cnt:{2}, id:{3}, msgEnt.msg:{4}, msgEnt.msgId:{5}",
                    size, pos, cnt, id, msgEnt.msg, msgEnt.msgId);
                */
            }

            //Console.WriteLine("[Dequeue]size:{0}, pos{1}, cnt{2}, id:{3}", size, pos, cnt, id);

            return msgEnt;
        }

        public bool Ack(string msgId)
        {
            bool ret;

            muQue.WaitOne();
            int idx = listMsg.FindIndex(x => x.msgId == msgId);
            if (idx >= 0)
            {
                listMsg.RemoveAt(idx);
                ret = true;
            } else
            {
                //Console.WriteLine("[Ack]fail to find message ID:{0}!", msgId);
                ret = false;
            }
            muQue.ReleaseMutex();

            /*
            Console.WriteLine("[Ack]size:{0}, pos:{1}, cnt:{2}, id:{3}, msgId:{4}, ret:{5}",
                size, pos, cnt, id, msgId, ret);
            */

            return ret;
        }

        public bool Fail(string msgId)
        {
            bool ret;

            muQue.WaitOne();
            int idx = listMsg.FindIndex(x => x.msgId == msgId);
            if (idx >= 0)
            {
                MessageEntity msgEnt = listMsg[idx];
                msgEnt.state = "RECEIVED";
                listMsg[idx] = msgEnt;
                cnt++;
                if (idx < pos)
                {
                    pos = idx;
                }
                ret = true;
            } else
            {
                //Console.WriteLine("[Fail]fail to find message ID!:{0}", msgId);
                ret = false;
            }
            muQue.ReleaseMutex();

            /*
            Console.WriteLine("[Fail]size:{0}, pos:{1}, cnt:{2}, id:{3}, msgId:{4}, ret:{5}",
                size, pos, cnt, id, msgId, ret);
            */
            return ret;
        }

    }
}
