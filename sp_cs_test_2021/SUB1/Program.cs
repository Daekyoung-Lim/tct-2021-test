using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_TEST
{
    class Program
    {
        public Queue<string> queMsg = new Queue<string>();

        static void Main(string[] args)
        {
            Program prog = new Program();

            while (true)
            {
                string[] cmdArr = Console.ReadLine().Split();
                
                switch (cmdArr[0])
                {
                    case "SEND":
                        prog.queMsg.Enqueue(cmdArr[1]);
                        break;
                    case "RECEIVE":
                        if (prog.queMsg.Count > 0)
                        {
                            Console.WriteLine(prog.queMsg.Dequeue());
                        }
                        break;
                }
            }
        }
    }
}
