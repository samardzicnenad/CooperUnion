/**********************************************************************
 * Created by : StackOverflow
 * Date       : -
 * Description: The class represents 2 element tuple
 * Idea       : MS .NET 3.5 doesn't support Tuple structure, so I used finished class
 * Parameters : -
 **********************************************************************/
namespace nsATMWebSvc
{
    public class Tuple<T1, T2>
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }

        internal Tuple(T1 first, T2 second)
        {
            Item1 = first;
            Item2 = second;
        }
    }

    public static class Tuple
    {
        public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second)
        {
            var tuple = new Tuple<T1, T2>(first, second);
            return tuple;
        }
    }
}