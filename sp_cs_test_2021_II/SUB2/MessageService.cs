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
        int mQueueLength;

        public CustomQueue(int maxLen)
        {
            mListMsg = new List<Message>();
            this.mQueueLength = maxLen;
        }

        public void Enqueue(Message msg)
        {
            if (mListMsg.Count < this.mQueueLength)
            {
                mListMsg.Add(msg);
            } else
            {
                Console.WriteLine("Queue Full");
            }
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

        public void createQueue(string name, int maxLength)
        {
            if (!mDicQue.ContainsKey(name))
            {
                mDicQue.Add(name, new CustomQueue(maxLength));
            } else
            {
                Console.WriteLine("Queue Exist");
            }
        }

        public CustomQueue findQueue(string name)
        {
            return mDicQue[name];
        }
    }
}
