using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SP_TEST
{
    class MessageHttpServer
    {
        public HttpListener listener;
        MessageService msgSvc;

        public MessageHttpServer()
        {
            msgSvc = new MessageService();
        }

        public void doHttpServer()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8080/");
            listener.Start();

            try
            {
                while (true)
                {
                    var context = listener.GetContext();

                    Thread thConext = new Thread(doContext);
                    thConext.Start(context);
                }
            } catch (HttpListenerException e)
            {
            }
        }

        private void doContext(Object obj)
        {
            HttpListenerContext context = (HttpListenerContext)obj;
            JObject resJson = new JObject();
            bool bRetMsgSvc;
            MessageEntity msgEnt;

            Console.WriteLine("[doContext]url:{0}", context.Request.Url);

            String[] words = context.Request.Url.LocalPath.Split('/');
            String cmd = words[1];
            String queName = words[2];
            
            //Console.WriteLine("[doContext]cmd:{0}, queName:{1}", cmd, queName);

            if (context.Request.HttpMethod == "GET")
            {
                bRetMsgSvc = msgSvc.handleMsgSvcCmd(cmd, queName, out msgEnt);
                if (bRetMsgSvc)
                {
                    resJson["Result"] = "Ok";
                    resJson["MessageID"] = msgEnt.msgId;
                    resJson["Message"] = msgEnt.msg;
                }
                else
                {
                    resJson["Result"] = "No Message";
                }
            }
            else if (context.Request.HttpMethod == "POST")
            {
                JObject jsonBody = null;

                using (StreamReader sr = new StreamReader(context.Request.InputStream))
                {
                    string body = sr.ReadToEnd();
                    if (body != "null")
                    {
                        jsonBody = JObject.Parse(body);
                    }
                }

                switch (cmd)
                {
                    case "CREATE":
                        string queSize = jsonBody["QueueSize"].ToString();

                        //Console.WriteLine("[doContext/POST/CREATE]queSize:{0}", queSize);

                        bRetMsgSvc = msgSvc.handleMsgSvcCmd(cmd, queName, out msgEnt, queSize);
                        if (bRetMsgSvc)
                        {
                            resJson["Result"] = "Ok";
                        } else
                        {
                            resJson["Result"] = "Queue Exist";
                        }
                        break;
                    case "SEND":
                        string msg = jsonBody["Message"].ToString();

                        //Console.WriteLine("[doContext/POST/SEND]msg{0}", msg);

                        bRetMsgSvc = msgSvc.handleMsgSvcCmd(cmd, queName, out msgEnt, msg);
                        if (bRetMsgSvc)
                        {
                            resJson["Result"] = "Ok";
                        }
                        else
                        {
                            resJson["Result"] = "Queue Full";
                        }
                        break;
                    case "ACK":

                        //Console.WriteLine("[doContext/POST/ACK]msgId{0}", words[3]);

                        bRetMsgSvc = msgSvc.handleMsgSvcCmd(cmd, queName, out msgEnt, words[3]);
                        if (bRetMsgSvc)
                        {
                            resJson["Result"] = "Ok";
                        }
                        else
                        {
                            Console.WriteLine("[doContext]Ack error!");
                        }
                        break;
                    case "FAIL":

                        //Console.WriteLine("[doContext/POST/FAIL]msgId:{0}", words[3]);

                        bRetMsgSvc = msgSvc.handleMsgSvcCmd(cmd, queName, out msgEnt, words[3]);
                        if (bRetMsgSvc)
                        {
                            resJson["Result"] = "Ok";
                        }
                        else
                        {
                            Console.WriteLine("[doContext]Fail error!");
                        }
                        break;
                }
            }

            sendData(context, resJson);

            //Console.WriteLine();
        }

        private void sendData(HttpListenerContext context, JObject resJson)
        {
            byte[] data = Encoding.UTF8.GetBytes(resJson.ToString());
            context.Response.OutputStream.Write(data, 0, data.Length);
            context.Response.StatusCode = 200;
            context.Response.Close();
        }
    }
}
