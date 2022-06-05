using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_TEST
{
    class FixedQueue
    {
        private int size { get; set; }
        private Queue<string> queMsg;

        public FixedQueue(int size)
        {
            this.size = size;
            queMsg = new Queue<string>();
        }

        public bool Enqueue(string msg)
        {
            if (queMsg.Count < size)
            {
                queMsg.Enqueue(msg);
                return true;
            }
            else
            {
                return false;
            }
        }

        public string Dequeue()
        {
            if (queMsg.Count > 0)
            {
                return queMsg.Dequeue();
            }
            else
            {
                return null;
            }
        }
    }
}
