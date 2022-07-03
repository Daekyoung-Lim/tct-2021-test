using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SP_TEST
{
    class ManageHttpServer
    {
        HttpListener listener = new HttpListener();
        MessageService msgSvc = null;

        public void DoHttpServer(Object obj)
        {
            Console.WriteLine("Http Server Thread Start");

            msgSvc = (MessageService)obj;

            listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8080/");
            listener.Start();

            try
            {
                while (true)
                {
                    var context = listener.GetContext();

                    Thread thContext = new Thread(DoContext);
                    thContext.Start(context);
                }
            }
            catch (HttpListenerException e)
            {
                Console.WriteLine("Close HTTP Server");
            }
        }

        private void DoContext(Object obj)
        {
            HttpListenerContext context = (HttpListenerContext)obj;
            JObject resJson = new JObject();

            Console.WriteLine(context.Request.Url);

            string[] words = context.Request.Url.LocalPath.Split('/');
            string command = words[1];
            string queName = null;
            CustomQueue que = null;
            string msgId = null;

            if (context.Request.HttpMethod == "GET")
            {
                Message msg = null;

                switch (command)
                {
                    case "RECEIVE":
                        queName = words[2];
                        que = msgSvc.findQueue(queName);
                        msg = que.Dequeue();
                        if (msg != null)
                        {
                            resJson["Result"] = "Ok";
                            resJson["MessageID"] = msg.mMsgId;
                            resJson["Message"] = msg.mMsgContents;
                        } else
                        {
                            resJson["Result"] = "No Message";
                        }
                        break;
                    case "DLQ":
                        queName = words[2];
                        que = msgSvc.findQueue(queName);
                        msg = que.DequeueDlq();
                        if (msg != null)
                        {
                            resJson["Result"] = "Ok";
                            resJson["MessageID"] = msg.mMsgId;
                            resJson["Message"] = msg.mMsgContents;
                        }
                        else
                        {
                            resJson["Result"] = "No Message";
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (context.Request.HttpMethod == "POST")
            {
                JObject jsonBody = null;
                
                using (StreamReader sr = new StreamReader(context.Request.InputStream))
                {
                    string body = sr.ReadToEnd();
                    if (body != "null")
                        jsonBody = JObject.Parse(body);
                }

                switch (command)
                {
                    case "CREATE":
                        queName = words[2];
                        string queSize = jsonBody["QueueSize"].ToString();
                        int procTimeOut = jsonBody["ProcessTimeout"].ToObject<int>();
                        int maxFailCnt = jsonBody["MaxFailCount"].ToObject<int>();
                        int waitTime = jsonBody["WaitTime"].ToObject<int>();
                        if (msgSvc.createQueue(queName, Int32.Parse(queSize), procTimeOut, maxFailCnt, waitTime))
                        {
                            resJson["Result"] = "Ok";
                        } else
                        {
                            resJson["Result"] = "Queue Exist";
                        }
                        break;
                    case "SEND":
                        queName = words[2];
                        string message = jsonBody["Message"].ToString();
                        que = msgSvc.findQueue(queName);
                        if (que.Enqueue(message))
                        {
                            resJson["Result"] = "Ok";
                        } else
                        {
                            resJson["Result"] = "Queue Full";
                        }
                        break;
                    case "ACK":
                        queName = words[2];
                        msgId = words[3];
                        que = msgSvc.findQueue(queName);
                        que.Ack(msgId);
                        resJson["Result"] = "Ok";
                        break;
                    case "FAIL":
                        queName = words[2];
                        msgId = words[3];
                        que = msgSvc.findQueue(queName);
                        que.Fail(msgId);
                        resJson["Result"] = "Ok";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                return;
            }

            SendData(context, resJson);
        }

        private void SendData(HttpListenerContext context, JObject resJson)
        {
            byte[] data = Encoding.UTF8.GetBytes(resJson.ToString());
            context.Response.OutputStream.Write(data, 0, data.Length);
            context.Response.StatusCode = 200;
            context.Response.Close();
        }
    }
}
