﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;

namespace SP_TEST
{
    class Message
    {
        public String mMsgId { get; set; }
        public String mMsgContents { get; set; }
        private bool mFlagAvail;
        private int mFailCnt;
        private int mMaxFailCnt;
        private Timer mAckTimer;
        static System.Threading.Mutex muAvail = new System.Threading.Mutex();
        static System.Threading.Mutex muTimer = new System.Threading.Mutex();
        public CustomQueue mCustomQue;

        public Message(String msgId, String contents, bool avail, int maxFailCnt, CustomQueue customQue)
        {
            this.mMsgId = msgId;
            this.mMsgContents = contents;
            this.mFlagAvail = avail;
            this.mFailCnt = 0;
            this.mMaxFailCnt = maxFailCnt;
            this.mAckTimer = null;
            this.mCustomQue = customQue;
        }

        public void SetHandleTime(int procTimeOut)
        {
            muTimer.WaitOne();

            this.mAckTimer = new Timer(procTimeOut * 1000); //msec -> sec
            this.mAckTimer.Elapsed += OnTimedEvent;
            this.mAckTimer.AutoReset = false;
            this.mAckTimer.Enabled = true;

            muTimer.ReleaseMutex();
        }

        public void DeleteTimer()
        {
            muTimer.WaitOne();

            this.mAckTimer.Stop();
            this.mAckTimer.Dispose();
            this.mAckTimer = null;

            muTimer.ReleaseMutex();
        }

        public void SetFlagAvail(bool avail)
        {
            muAvail.WaitOne();

            if (avail)  //fail or timeout
            {
                //move to DLQ
                if (mFailCnt >= mMaxFailCnt)
                {
                    mFailCnt = 0;
                    this.mCustomQue.MoveToDlq(this);
                } else
                {
                    mFailCnt++;
                }
            }
            mFlagAvail = avail;

            muAvail.ReleaseMutex();
        }

        public bool GetFlagAvail()
        {
            return mFlagAvail;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            SetFlagAvail(true);
        }
    }

    class CustomQueue
    {
        List<Message> mListMsg;
        static System.Threading.Mutex muListMsg = new System.Threading.Mutex();
        int mQueueLength;
        private String mQueName;
        private int mAccCount;
        private int mProcTimeOut;
        private int mMaxFailCnt;
        private int mWaitTime;

        List<Message> mListMsgDlq;
        List<HttpListenerContext> listHttpContext;

        public CustomQueue(String name, int maxLen, int procTimeOut, int maxFailCnt, int waitTime)
        {
            mListMsg = new List<Message>();
            this.mQueueLength = maxLen;
            this.mQueName = name;
            this.mAccCount = 0;
            this.mProcTimeOut = procTimeOut;
            this.mMaxFailCnt = maxFailCnt;
            this.mWaitTime = waitTime;

            mListMsgDlq = new List<Message>();
            listHttpContext = new List<HttpListenerContext>();
        }

        public bool Enqueue(String message)
        {
            bool ret;

            if (mListMsg.Count < this.mQueueLength)
            {
                muListMsg.WaitOne();

                mListMsg.Add(CreateMsg(message));

                muListMsg.ReleaseMutex();

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
                msg = mListMsg.Find(x => x.GetFlagAvail() == true);
                if (msg != null)
                {
                    msg.SetFlagAvail(false);
                    if (mProcTimeOut != 0)
                    {
                        msg.SetHandleTime(mProcTimeOut);
                    }
                }
            }

            return msg;
        }

        private int CheckReadyCondition(HttpListenerContext httpContext)
        {
            //return false if available mesaage exist
            if ((mListMsg.Count > 0) && 
                (mListMsg.Find(x => x.GetFlagAvail() == true) != null) &&
                (listHttpContext.IndexOf(httpContext) == 0))
            {
                //Console.WriteLine("[CheckWaitCondition]: Ready");
                return 1;
            } else if ((mListMsgDlq.Count > 0) && 
                (mListMsgDlq.Find(x => x.GetFlagAvail() == true) != null) &&
                (listHttpContext.IndexOf(httpContext) == 0))
            {
                return 2;
            } else
            {
                //Console.WriteLine("[CheckWaitCondition]: Not ready");
                return 0;
            }
        }

        /*
        private int CheckReadyCondition(HttpListenerContext httpContext)
        {
            //return false if available mesaage exist
            if ((mListMsg.Count > 0) &&
                (mListMsg.Find(x => x.GetFlagAvail() == true) != null) &&
                (listHttpContext.IndexOf(httpContext) == 0))
            {
                //Console.WriteLine("[CheckWaitCondition]: Ready");
                return 1;
            }
            else
            {
                //Console.WriteLine("[CheckWaitCondition]: Not ready");
                return 0;
            }
        }
        */

        public Message DequeueWithWaitTime(HttpListenerContext httpContext)
        {
            if ((mWaitTime != 0) &&
                (mListMsg.Find(x => x.GetFlagAvail() == true) == null))
            {
                listHttpContext.Add(httpContext);
                int ret = 0;
                int startTick = Environment.TickCount;

                Console.WriteLine();
                Console.WriteLine("[startTick]:{0}", startTick);
                Console.WriteLine("[Environment.TickCount]:{0}", Environment.TickCount);
                Console.WriteLine("[TickCount - startTick]:{0}", Environment.TickCount - startTick);
                Console.WriteLine("[mWaitTime * 1000]:{0}", mWaitTime * 1000);

                while (((Environment.TickCount - startTick) < (mWaitTime * 1000)) &&
                       ((ret = CheckReadyCondition(httpContext)) == 0))
                {
                    //Console.WriteLine("[Environment.TickCount]:{0}", Environment.TickCount);
                    //Console.WriteLine("[TickCount - startTick]:{0}", Environment.TickCount - startTick);

                    System.Threading.Thread.Sleep(10);
                }

                Console.WriteLine();
                Console.WriteLine("[startTick]:{0}", startTick);
                Console.WriteLine("[Environment.TickCount]:{0}", Environment.TickCount);
                Console.WriteLine("[TickCount - startTick]:{0}", Environment.TickCount - startTick);
                Console.WriteLine("ret: {0}", ret);

                listHttpContext.Remove(httpContext);

                if (ret == 1)
                {
                    return Dequeue();
                } else if (ret == 2)
                {
                    return DequeueDlq();
                } else
                {
                    return null;
                }
            } else
            {
                return Dequeue();
            }
        }

        public void MoveToDlq(Message msg)
        {
            muListMsg.WaitOne();
            mListMsg.Remove(msg);
            muListMsg.ReleaseMutex();

            mListMsgDlq.Add(msg);
        }

        public Message DequeueDlq()
        {
            Message msg = null;

            if (mListMsgDlq.Count > 0)
            {
                msg = mListMsgDlq.Find(x => x.GetFlagAvail() == true);
                if (msg != null)
                {
                    mListMsgDlq.Remove(msg);
                }
            }

            return msg;
        }

        public void Ack(String msgId)
        {
            Message msg = mListMsg.Find(x => x.mMsgId == msgId);
            if (mProcTimeOut != 0)
            {
                msg.DeleteTimer();
            }

            muListMsg.WaitOne();
            mListMsg.Remove(msg);
            muListMsg.ReleaseMutex();
        }

        public void Fail(String msgId)
        {
            Message msg = mListMsg.Find(x => x.mMsgId == msgId);
            if (mProcTimeOut != 0)
            {
                msg.DeleteTimer();
            }
            msg.SetFlagAvail(true);
        }

        private Message CreateMsg(String message)
        {
            String msgId = mQueName + "_" + mAccCount.ToString();
            mAccCount++;
            Message msg = new Message(msgId, message, true, mMaxFailCnt, this);

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

        public bool createQueue(String name, int maxLen, int procTimeOut, int maxFailCnt, int waitTime)
        {
            bool ret;

            if (!mDicQue.ContainsKey(name))
            {
                mDicQue.Add(name, new CustomQueue(name, maxLen, procTimeOut, maxFailCnt, waitTime));
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
