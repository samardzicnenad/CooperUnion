/**********************************************************************
 * Created by : Nenad Samardzic
 * Date       : 04/23/2013
 * Description: The class represents producer - consumer(s) model.
 *              It comprises both FIFO and LIFO data structures.
 * Idea       : The model is scalable in terms of 3 parameters:
 *              - total number of threads
 *              - the scope for generating random numbers
 *              - the number of push/pop actions to be performed
 *              Half of the threads are used to serve stack, and the other half to serve queue.
 *              When a random number is generated, depending if it is less/bigger then the the middle of the scope (value) it will:
 *              1) be pushed to the stack with the appropriate message.
 *              2) be pushed to the queue with the appropriate message.
 *              One of the related threads will pop it from the appropriate data structure and the message is printed.
 **********************************************************************/
using System;
using System.Threading;

namespace ProducerConsumers
{
    public class ProducerConsumers
    {
        static readonly object stackLocker = new object(); //stack locker
        static readonly object queueLocker = new object(); //queue locker
        Stack myStack;
        Queue myQueue;
        Thread[] _stackWorkers, _queueWorkers; //thread arrays
        static readonly int nNoThreads = 8, nRndScope = 20, nRndTotal = 222; //parameters

        //helper Node subclass
        private class Node
        {
            public object Data; //data held
            public Node Next; //next Node
            public int counter; //help counter
        
            public Node(object obj, int counter)
            {
                this.Data = obj;
                this.counter = counter;
            }
        }

        //Stack subclass
        private class Stack
        {
            private Node top;
            private int nCount;

            public Stack()
            {
                this.nCount = 0;
                this.top = null;
            }

            public void Push(object obj, int counter)
            {
                Node newNode = new Node(obj, counter);
                if (top == null)
                    top = newNode;
                else
                {
                    Node temp = top;
                    top = newNode;
                    top.Next = temp;
                }
                if (obj != null)
                {
                    Console.WriteLine(String.Format("{0}. number pushed to the stack - {1}", counter, obj.ToString()));
                    nCount++;
                }
            }
            public object Pop()
            {
                //if (this.isEmpty()) throw new Exception("Nothing to pop!");
                Node objToPop = top;
                top = top.Next;
                nCount--;
                if (objToPop.Data != null)
                    Console.WriteLine(String.Format("{0}. number popped from the stack - {1}", objToPop.counter, objToPop.Data.ToString()));
                return objToPop.Data;
            }
            public Boolean isEmpty()
            {
                return nCount == 0;
            }
        }

        //Queue subclass
        private class Queue
        {
            private Node top, bottom;
            private int nCount;

            public Queue()
            {
                this.nCount = 0;
                this.bottom = null;
                this.top = bottom;
            }

            public void Push(object obj, int counter)
            {
                Node newNode = new Node(obj, counter);
                if (bottom == null)
                {
                    bottom = newNode;
                    top = bottom;
                }
                else
                {
                    Node temp = top;
                    top = newNode;
                    temp.Next = top;
                }
                if (obj != null)
                {
                    Console.WriteLine(String.Format("{0}. number pushed to the queue - {1}", counter, obj.ToString()));
                    nCount++;
                }
            }
            public object Pop()
            {
                //if (this.isEmpty()) throw new Exception("Nothing to pop!");
                Node objToPop = bottom;
                bottom = bottom.Next;
                nCount--;
                if (objToPop.Data != null)
                    Console.WriteLine(String.Format("{0}. number popped from the queue - {1}", objToPop.counter, objToPop.Data.ToString()));
                return objToPop.Data;
            }
            public Boolean isEmpty()
            {
                return nCount == 0;
            }
        }

        //Constructor creates data structures and creates and starts thread pools
        public ProducerConsumers() {
            _stackWorkers = new Thread[nNoThreads/2];
            _queueWorkers = new Thread[nNoThreads/2];

            myStack = new Stack();
            myQueue = new Queue();

            for (int i = 0; i < nNoThreads/2; i++)
            {
                (_stackWorkers[i] = new Thread(popStack)).Start();
                (_queueWorkers[i] = new Thread(popQueue)).Start();
            }
        }

        public void pushStack(Object item, int counter)
        {
            lock (stackLocker)
            {
                this.myStack.Push(item, counter);
                Monitor.Pulse(stackLocker);
            }
        }
        public void pushQueue(Object item, int counter)
        {
            lock (queueLocker)
            {
                this.myQueue.Push(item, counter);
                Monitor.Pulse(queueLocker);
            }
        }
        void popStack() {
            while (true) {
                object item;
                lock (stackLocker) {
                    while (this.myStack.isEmpty()) Monitor.Wait(stackLocker);
                    item = this.myStack.Pop();
                    if (item == null) return;
                }
            }
        }
        void popQueue() {
            while (true) {
                object item;
                lock (queueLocker) {
                    while (this.myQueue.isEmpty()) Monitor.Wait(queueLocker);
                    item = this.myQueue.Pop();
                    if (item == null) return;
                }
            }
        }
        //Exit while(true) loops
        public void Shutdown() {
            foreach (Thread worker in _stackWorkers)
                lock (stackLocker)
                {
                    this.myStack.Push(null, -1);
                }
            foreach (Thread worker in _queueWorkers)
                lock (_queueWorkers)
                {
                    this.myQueue.Push(null, -1);
                }
            foreach (Thread worker in _stackWorkers)
                worker.Join();
            foreach (Thread worker in _queueWorkers)
                worker.Join();
        }

        public static void Main() {
            Random rnd = new Random();
            ProducerConsumers PC = new ProducerConsumers();
            int nCount=0;

            while (nCount<nRndTotal)
            {
                int randomNumber = rnd.Next(nRndScope)+1;
                if (randomNumber < nRndScope/2+1)
                {
                    PC.pushStack(randomNumber, ++nCount);
                }
                else {
                    PC.pushQueue(randomNumber, ++nCount);
                }
            }
            PC.Shutdown();
        }
    }
}
