using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_TEST
{
    class Message
    {
        public String mMsgContents { get; set; }

        public Message(String contents)
        {
            this.mMsgContents = contents;
        }
    }

    class CustomQueue
    {
        List<Message> mListMsg;

        public CustomQueue()
        {
            mListMsg = new List<Message>();
        }

        public void Enqueue(Message msg)
        {
            mListMsg.Add(msg);
        }

        public Message Dequeue()
        {
            Message msg = null;
            
            if (mListMsg.Count > 0)
            {
                msg = mListMsg[0];
                mListMsg.RemoveAt(0);
            }

            return msg;
        }
    }


    class MessageService
    {
        Dictionary<String, CustomQueue> mDicQue;

        public MessageService()
        {
            mDicQue = new Dictionary<string, CustomQueue>();
        }

        public void createQueue(string name)
        {
            if (!mDicQue.ContainsKey(name))
            {
                mDicQue.Add(name, new CustomQueue());
            }
        }

        public CustomQueue findQueue(string name)
        {
            return mDicQue[name];
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            MessageService msgSvc = new MessageService();
            msgSvc.createQueue("DEFAULT");
            CustomQueue que = msgSvc.findQueue("DEFAULT");

            while (true)
            {
                String cmdLine = Console.ReadLine();

                String[] arrCmd = cmdLine.Split(' ');

                switch(arrCmd[0])
                {
                    case "SEND":
                        que.Enqueue(new Message(arrCmd[1]));
                        break;
                    case "RECEIVE":
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
