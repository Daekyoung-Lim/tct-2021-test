using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_TEST
{
    class Message
    {
        public String mMsgId { get; set; }
        public String mMsgContents { get; set; }
        public bool mFlagAvail;

        public Message(String msgId, String contents, bool avail)
        {
            this.mMsgId = msgId;
            this.mMsgContents = contents;
            this.mFlagAvail = avail;
        }
    }

    class CustomQueue
    {
        List<Message> mListMsg;
        int mQueueLength;
        private String mQueName;
        private int mAccCount;

        public CustomQueue(String name, int maxLen)
        {
            mListMsg = new List<Message>();
            this.mQueueLength = maxLen;
            this.mQueName = name;
            this.mAccCount = 0;
        }

        public bool Enqueue(String message)
        {
            bool ret;

            if (mListMsg.Count < this.mQueueLength)
            {
                mListMsg.Add(CreateMsg(message));
                ret = true;
            } else
            {
                Console.WriteLine("Queue Full");

                ret = false;
            }

            return ret;
        }

        public Message Dequeue()
        {
            Message msg = null;

            if (mListMsg.Count > 0)
            {
                msg = mListMsg.Find(x => x.mFlagAvail == true);
                if (msg != null)
                {
                    msg.mFlagAvail = false;
                }
            }

            return msg;
        }

        public void Ack(String msgId)
        {
            Message msg = mListMsg.Find(x => x.mMsgId == msgId);
            mListMsg.Remove(msg);
        }

        public void Fail(String msgId)
        {
            Message msg = mListMsg.Find(x => x.mMsgId == msgId);
            msg.mFlagAvail = true;
        }

        private Message CreateMsg(String message)
        {
            String msgId = mQueName + "_" + mAccCount.ToString();
            mAccCount++;
            Message msg = new Message(msgId, message, true);

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

        public bool createQueue(string name, int maxLength)
        {
            bool ret;

            if (!mDicQue.ContainsKey(name))
            {
                mDicQue.Add(name, new CustomQueue(name, maxLength));
                ret = true;
            } else
            {
                Console.WriteLine("Queue Exist");
                ret = false;
            }

            return ret;
        }

        public CustomQueue findQueue(string name)
        {
            return mDicQue[name];
        }
    }
}
