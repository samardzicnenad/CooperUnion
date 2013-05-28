/**********************************************************************
 * Created by : Nenad Samardzic
 * Date       : 04/25/2013
 * Description: The class represents solution of the dining philosophers problem.
 * Idea       : The model is created accordingly to Chandy / Misra solution (from wikipedia):
 *              In 1984, K. Mani Chandy and A. Aggarwal proposed a different solution to the dining philosophers problem to allow for arbitrary agents
 *              (numbered P1, ..., Pn) to contend for an arbitrary number of resources, unlike Dijkstra's solution. It is also completely distributed
 *              and requires no central authority after initialization. However, it violates the requirement that "the philosophers do not speak to
 *              each other" (due to the request messages).
 *              1) For every pair of philosophers contending for a resource, create a fork and give it to the philosopher with the lower ID. Each fork
 *              can either be dirty or clean. Initially, all forks are dirty.
 *              2) When a philosopher wants to use a set of resources (i.e. eat), he must obtain the forks from his contending neighbors. For all such
 *              forks he does not have, he sends a request message.
 *              3)When a philosopher with a fork receives a request message, he keeps the fork if it is clean, but gives it up when it is dirty.
 *              If he sends the fork over, he cleans the fork before doing so.
 *              4) After a philosopher is done eating, all his forks become dirty. If another philosopher had previously requested one of the forks,
 *              he cleans the fork and sends it.
 *              This solution also allows for a large degree of concurrency, and will solve an arbitrarily large problem.
 *              It also solves the starvation problem. The clean / dirty labels act as a way of giving preference to the most "starved" processes,
 *              and a disadvantage to processes that have just "eaten". One could compare their solution to one where philosophers are not allowed
 *              to eat twice in a row without letting others use the forks in between. Their solution is more flexible than that, but has an element
 *              tending in that direction.
 *              In their analysis they derive a system of preference levels from the distribution of the forks and their clean/dirty states.
 *              They show that this system may describe an acyclic graph, and if so, the operations in their protocol cannot turn that graph into
 *              a cyclic one. This guarantees that deadlock cannot occur. However, if the system is initialized to a perfectly symmetric state,
 *              like all philosophers holding their left side forks, then the graph is cyclic at the outset, and their solution cannot prevent
 *              a deadlock. Initializing the system so that philosophers with lower IDs have dirty forks ensures the graph is initially acyclic.
 * Parameters : nDimension - number of philosophers - theoretically unlimited, but here the maximum is 111 (that many named philosophers as given)
 *              turnsLeft - how many times will each philosopher eat
 **********************************************************************/
using System;
using System.Text;
using System.Threading;

namespace DiningPhilosophers
{
    public class DiningPhilosophers
    {
        static int nDimension = 5; //number of philosophers
        static String[] top111 = new String[] { "Thales of Miletus", "Pythagoras", "Confucius", "Heracleitus", "Parmenides", "Zeno of Elea", 
            "Socrates", "Sun Tzu", "Democritus", "Plato", "Aristotle", "Euclid", "Siddhārtha Gautama Buddha", "Mencius", "Confucius", "Zhuangzi",
            "Pyrrhon of Elis", "Epicurus", "Zeno of Citium", "Philo Judaeus", "Epictetus", "Marcus Aurelius", "Nagarjuna", "Plotinus", "Richard Rorty",
            "Sextus Empiricus", "Saint Augustine", "Hypatia", "Boethius", "Śankara", "al-Kindī", "Al-Fārābī", "Avicenna", "Rāmānuja", "Ibn Gabirol",
            "Erasmus Roterodamus", "Saint Anselm of Canterbury", "Galileo Galilei", "al-Ghazālī", "Peter Abelard", "Averroës", "Zhu Xi",
            "Moses Maimonides", "Ibn al-‘Arabī", "Shinran", "Saint Thomas Aquinas", "John Duns Scotus", "William of Ockham", "Niccolò Machiavelli",
            "Wang Yangming", "Francis Bacon", "Pierre de Fermat", "Thomas Hobbes", "René Descartes", "John Locke", "Benedict de Spinoza", "Voltaire",
            "Gottfried Wilhelm Leibniz", "Giambattista Vico", "George Berkeley", "Charles Montesquieu", "David Hume", "Jean-Jacques Rousseau",
            "Immanuel Kant", "Moses Mendelssohn", "marquis de Condorcet", "Jeremy Bentham", "Georg Wilhelm Friedrich Hegel", "Arthur Schopenhauer",
            "Auguste Comte", "John Stuart Mill", "Søren Kierkegaard", "Karl Marx", "Herbert Spencer", "Wilhelm Dilthey", "William James", "Saul Kripke",
            "Friedrich Nietzsche", "Friedrich Ludwig Gottlob Frege", "Edmund Husser", "Henri Bergson", "John Dewey", "Alfred North Whitehead",
            "Benedetto Croce", "Nishida Kitarō", "Bertrand Russell", "G.E. Moore", "Martin Buber", "Ludwig Wittgenstein", "Martin Heidegger",
            "Rudolf Carnap", "David Émile Durkheim", "Sir Karl Popper", "Theodor Wiesengrund Adorno", "Jean-Paul Sartre", "Hannah Arendt", "Noam Chomsky",
            "Simone de Beauvoir", "Willard Van Orman Quine", "Sir A.J. Ayer", "Wilfrid Sellars", "John Rawls", "Thomas S. Kuhn", "Michel Foucault",
            "Jürgen Habermas", "Sir Bernard Williams", "Jacques Derrida", "Robert Nozick", "David Kellogg Lewis", "Peter Singer", "Max Weber"};

        //Chopstick subclass
        public class Chopstick
        {
            public int nChopstickID;
            public bool bDirty = true; //the state of the chopstick
            public Philosopher pHasIt, pNeighborL, pNeighborR; //which philosopher holds it, who is left and who is right neighbor

            //constructor assigns ID to the chopstick
            public Chopstick(int nId)
            {
                nChopstickID = nId;
            }
            //return the other contender on the chopstick
            public Philosopher Contender(Philosopher pContender)
            {
                if (pNeighborL == pContender) return pNeighborR;
                else return pNeighborL;
            }
        }

        public class Philosopher
        {
            public int nMyID;
            public String sName;
            private int turnsLeft = 15; //turns to eat
            Chopstick cMyLeft, cMyRight, cTakeFromL, cTakeFromR, cGiveToL, cGiveToR;
            Action<Chopstick> Request;
            Action<Chopstick, Philosopher> Release;

            //constructor - assign ID, name, and left and right chopstick for the philosopher
            public Philosopher(int nId, String sPName, Chopstick cLeft, Chopstick cRight)
            {
                nMyID = nId;
                sName = sPName;
                cMyLeft = cLeft;
                cMyRight = cRight;
            }

            //define neighbors and give/take chopsticks actions on both sides
            public void Initialize()
            {
                Philosopher pContenderL = cMyLeft.Contender(this); //Philosopher on the left
                Philosopher pContenderR = cMyRight.Contender(this); //Philosopher on the right
                pContenderL.Request += GiveChopstick; //ask for and give both chopsticks
                pContenderR.Request += GiveChopstick;
                pContenderL.Release += TakeChopstick;
                pContenderR.Release += TakeChopstick;
            }

            //Which chopstick(s) does the philosopher have at the moment
            string HaveChopsticks()
            {
                if (cMyLeft.pHasIt == this)
                {
                    if (cMyRight.pHasIt == this) return "both";
                    else return "left";
                }
                if (cMyRight.pHasIt == this) return "right";
                return "none";
            }

            //Responds to a request for the chopstick
            void GiveChopstick(Chopstick chopstick)
            {
                if (chopstick.pHasIt != this) return; //doesn't have it
                if (chopstick.bDirty) //used - philosopher has eaten (or initial state) - will give it
                {
                    Console.WriteLine("{0} accepted request for chopstick number {1} from {2}", sName, chopstick.nChopstickID, chopstick.Contender(this).sName);
                    lock (chopstick)
                    {
                        chopstick.bDirty = false;
                        chopstick.pHasIt = null;
                    }
                    Release(chopstick, chopstick.Contender(this));
                }
                else //clean - decline, but will note the request
                {
                    Console.WriteLine("{0} declined request for chopstick number {1} from {2}", sName, chopstick.nChopstickID, chopstick.Contender(this).sName);
                    if (chopstick == cMyLeft) cGiveToL = chopstick; //note that it was asked for
                    else cGiveToR = chopstick;
                }
            }

            //Philosopher takes the chopstick
            void TakeChopstick(Chopstick chopstick, Philosopher pWho)
            {
                if (pWho != this) return;
                if (chopstick == cMyLeft) cTakeFromL = null; //have taken it
                else cTakeFromR = null;
                lock (chopstick)
                {
                    chopstick.pHasIt = this; //it's mine
                }
                Console.WriteLine("{0} takes chopstick number {1} from {2} and has {3} chopstick(s)", sName, chopstick.nChopstickID, chopstick.Contender(this).sName, HaveChopsticks());
            }

            public void Engage()
            {
                while (turnsLeft > 0)
                //while (true) //infinite model
                {
                    Console.WriteLine("{0} is thinking; chopstick(s) held: {1}", sName, HaveChopsticks());
                    while (HaveChopsticks() != "both") //ask for chopsticks until have both
                    {
                        Thread.Sleep(250);
                        if (cMyLeft.pHasIt != this && cTakeFromL != cMyLeft) //ask for the left chopstick
                        {
                            cTakeFromL = cMyLeft;
                            Console.WriteLine("{0} requests chopstick number {1} from {2}", sName, cMyLeft.nChopstickID, cMyLeft.Contender(this).sName);
                            Request(cMyLeft);
                        }
                        if (cMyRight.pHasIt != this && cTakeFromR != cMyRight) //ask for the right chopstick
                        {
                            cTakeFromR = cMyRight;
                            Console.WriteLine("{0} requests chopstick number {1} from {2}", sName, cMyRight.nChopstickID, cMyRight.Contender(this).sName);
                            Request(cMyRight);
                        }
                    }
                    turnsLeft--; //have both and will eat
                    Console.WriteLine("{0} is eating, {1} turn(s) left.", sName, turnsLeft);
                    //Console.WriteLine("{0} is eating", sName); //infinite model
                    cMyLeft.bDirty = true;
                    cMyRight.bDirty = true;

                    while (turnsLeft > 0 && cGiveToL == null && cGiveToR == null)
                    //while (cGiveToL == null && cGiveToR == null) //infinite model
                    {
                        lock (this)
                        {
                            Console.WriteLine("{0} is eating, {1} turn(s) left.", sName, --turnsLeft);
                            //Console.WriteLine("p{0} is eating", sName); //infinite model
                            Thread.Sleep(250);
                        }
                    }
                    if (turnsLeft == 0) //finished eating - if necessary offer chopsticks
                    {
                        if (cMyLeft.Contender(this).turnsLeft > 0)
                        {
                            cGiveToL = null;
                            Release(cMyLeft, cMyLeft.Contender(this));
                        }
                        if (cMyRight.Contender(this).turnsLeft > 0)
                        {
                            cGiveToR = null;
                            Release(cMyRight, cMyRight.Contender(this));
                        }
                    }
                    if (cGiveToL == cMyLeft) //earlier attempt to take a chopstick
                    {
                        Console.WriteLine("{0} accepts noted request for chopstick number {1} from {2}", sName, cMyLeft.nChopstickID, cMyLeft.Contender(this).sName);
                        cGiveToL = null;
                        Release(cMyLeft, cMyLeft.Contender(this));
                    }
                    if (cGiveToR == cMyRight) //earlier attempt to take a chopstick
                    {
                        Console.WriteLine("{0} accepts noted request for chopstick number {1} from {2}", sName, cMyRight.nChopstickID, cMyRight.Contender(this).sName);
                        cGiveToR = null;
                        Release(cMyRight, cMyRight.Contender(this));
                    }
                }
                Console.WriteLine("{0} is asleep. No more food for him.", sName);
            }
        }
        //helper function - determine what is left
        static int getLeft(int n)
        {
            return (n + 1) % nDimension;
        }
        //helper function - determine what is right
        static int getRight(int n)
        {
            return n % nDimension;
        }

        static void Main()
        {
            Philosopher[] philosophers = new Philosopher[nDimension];
            Chopstick[] chopsticks = new Chopstick[nDimension];
            Thread[] philosopherThread = new Thread[nDimension];

            String sTemp;
            Random rnd = new Random();
            int randomNumber;

            for (int i = 0; i < nDimension; i++) chopsticks[i] = new Chopstick(i);
            for (int i = 0; i < nDimension; i++)
            {
                sTemp = "";
                Chopstick cLeft = chopsticks[getLeft(i)];
                Chopstick cRight = chopsticks[getRight(i)];
                while (sTemp == "") //name all philosophers
                {
                    randomNumber = rnd.Next(111);
                    if (top111[randomNumber] != "")
                    {
                        sTemp = top111[randomNumber];
                        top111[randomNumber] = "";
                    }
                }
                philosophers[i] = new Philosopher(i, sTemp, cLeft, cRight);
                cLeft.pNeighborR = philosophers[i]; //right neighbor of the left chopstick is the philosopher himself
                cRight.pNeighborL = philosophers[i]; //left neighbor of the right chopstick is the philosopher himself
                philosopherThread[i] = new Thread(philosophers[i].Engage);
            }
            foreach (Chopstick chopstick in chopsticks)
            {
                if (chopstick.pNeighborL.nMyID > chopstick.pNeighborR.nMyID) chopstick.pHasIt = chopstick.pNeighborR; //Philosopher with smaller ID gets it
                else chopstick.pHasIt = chopstick.pNeighborL;
            }
            foreach (Philosopher philosopher in philosophers) philosopher.Initialize();
            for (int i = 0; i < nDimension; i++) philosopherThread[i].Start(); //start each thread
            for (int i = 0; i < nDimension; i++) philosopherThread[i].Join();
            Console.ReadLine();
        }
    }
}