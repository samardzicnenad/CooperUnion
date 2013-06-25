/**********************************************************************
 * Created by : Nenad Samardzic
 * Date       : 04/16/2013
 * Description: The class represents model for multithreading operations.
 *              It comprises both "one-time" and "from the pool" thread usage.
 * Idea       : The model represents a pizza restaurant which serves guests on-site, but also does pizza delivery.
 *              Whenever a satisfying criteria is meet (randomly generated number is divisible by one of two key values) an action is triggered:
 *              1) If the random number is divisible by 11777, we consider that we have a guest on-site.
 *              New action item is added to our serving queue.
 *              Our waiter (a thread from the pool) informs us of the moments when the guest orders and when the guest leaves.
 *              2) If the random number is divisible by 11779, we consider that we have delivery request.
 *              We hire a delivery pizza boy ("one-time" used thread) which informs us of the moments when he starts and when he ends delivery.
 **********************************************************************/
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pizzeria {
    class Pizzeria {
        Queue<Action> orders = new Queue<Action>(); //action queue
        Thread[] _workerThreads; //Our waiters

        readonly object lockerQ = new object(); //locks the queue
        readonly object lockerC = new object(); //locks the served/delivered pizza counters
        readonly object lockerCh = new object(); //locks the current char
        readonly object lockerPN = new object(); //locks the pizza counter

        int pizzaNumber, servedPizzas, deliveredPizzas; //helper vars
        const int pizzaCapacity = 600, workingWaiters = 4; //program dimensions
        char startChar = 'A';

        //Constructor method which prints out the info segment and starts waiting threads
        public Pizzeria(int waiterCount) {
            _workerThreads = new Thread[waiterCount];
            this.pizzaNumber = 1;
            this.servedPizzas = 0;
            this.deliveredPizzas = 0;
            // Create and start a separate thread for each worker
            for (int i = 0; i < waiterCount; i++)
                (_workerThreads[i] = new Thread(TakeOrder)).Start();
            StartMessage();
        }

        //Waiters wait for the orders in the queue
        private void TakeOrder() {
            while (true) {
                Action item = null;
                lock (lockerQ) {
                    while (orders.Count == 0) Monitor.Wait(lockerQ);
                    item = orders.Dequeue();
                }
                if (item == null) return; //escape the while(true) loop
                item();
            }
        }

        //Guest serving method
        private void serveGuests(int pizzaNo, String waiter)
        {
            try //doesn't do much, but just to demonstrate that exception handling goes within threads
            {
                Console.WriteLine("Waiter Thread-{0} has served the guests that ordered pizza number {1}", waiter, pizzaNo);
                eatPizza("S");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
            }
        }

        //Pizza delivery method
        private void deliverPizza(int pizzaNo, String pizzaBoy)
        {
            try
            {
                eatPizza("D");
                Console.WriteLine("Pizza boy Thread-{0} delivered his pizza number {1}", pizzaBoy, pizzaNo);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
            }
        }

        //Counting served vs. delivered
        private void eatPizza(String s)
        {
            lock (lockerC)
            {
                if (s == "S")
                    servedPizzas++;
                else
                    deliveredPizzas++;
            }
        }

        //Prints message at the beginning
        private void StartMessage()
        {
            Console.WriteLine("Our pizzeria opens on {0} at {1}", DateTime.Now.ToString("MM/dd/yyyy"), DateTime.Now.ToString(("h:mm:ss tt")));
            Console.WriteLine("Our capacity for today is {0} pizzas.\nWelcome!", pizzaCapacity.ToString());
            Console.WriteLine(); Console.WriteLine();
        }

        //Shuts down all of the threads
        private void Shutdown()
        {
            foreach (Thread worker in _workerThreads)
                lock (lockerQ)
                {
                    orders.Enqueue(null); //Exit while(true) loop
                    Monitor.Pulse(lockerQ);
                }

            foreach (Thread worker in _workerThreads)
                worker.Join(); // Wait for the worker thread to finish

            EndMessage();
        }

        //Prints message at the end
        private void EndMessage()
        {
            System.Threading.Thread.Sleep(150);
            Console.WriteLine(); Console.WriteLine();
            Console.WriteLine("{0} pizzas were delivered and {1} pizzas were served today in our restaurant.", deliveredPizzas, servedPizzas);
            Console.WriteLine();
            Console.WriteLine("Our pizzeria closes on {0} at {1}", DateTime.Now.ToString("MM/dd/yyyy"), DateTime.Now.ToString(("h:mm:ss tt")));
        }
    
        //Helper method - increasing the probability that the thread name is unique
        //(First I went with GUID, but that one was not very nice to read)
        private char nextLetter(char c) {
            lock (lockerCh)
            {
                char next = Convert.ToChar((int)'A' + ((c - 'A') + 1) % 26);
                return next;
            }
        }

        //Helper method - increasing the pizza no.
        private void countPizza()
        {
            lock (lockerPN)
            {
                pizzaNumber += 1;
            }
        }
        
        //use a thread pool to serve the request
        private void Rand111751(int randomNumber)
        {
            string sName = startChar.ToString() + randomNumber.ToString();
            startChar = nextLetter(startChar);
            int nSend = pizzaNumber;
            Console.WriteLine("Waiter Thread-{0} received his order for the pizza number {1}", sName, pizzaNumber);
            orders.Enqueue(() => serveGuests(nSend, sName)); //It will be handled by the waiting thread
            lock (lockerQ) //give a signal that something is added to a queue
                Monitor.Pulse(lockerQ);
            countPizza();
        }

        //create a thread to serve the request
        private void Rand111773(int randomNumber)
        {
            string sName = startChar.ToString() + randomNumber.ToString();
            Console.WriteLine("Pizza boy Thread-{0} received his order for the pizza number {1}", sName, pizzaNumber);
            startChar = nextLetter(startChar);
            int nSend = pizzaNumber;
            Thread t = new Thread(() => deliverPizza(nSend, sName)); //Create one-time thread
            t.Start();
            countPizza();
        }

        static void Main() {
            Pizzeria myPizzeria = new Pizzeria(workingWaiters);
            Random rnd = new Random();

            while (myPizzeria.pizzaNumber <= pizzaCapacity)
            {
                int randomNumber = rnd.Next(100000000);
                if (randomNumber % 111751 == 0) //We have a guest in pizzeria. One of thread pool waiters will take care of the guest
                    myPizzeria.Rand111751(randomNumber);
                if (randomNumber % 111773 == 0) //We have a delivery order. Delivery company ("one-time" thread) will do a delivery
                    myPizzeria.Rand111773(randomNumber);
            }
            myPizzeria.Shutdown();
            Console.ReadLine(); //Just to wait with the visible results
        }
    }
}
