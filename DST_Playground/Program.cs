using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DST_Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            int year = 2017;

            Console.WriteLine(string.Format("Displaying transition dates info about {0} year ...", year));
            Console.WriteLine("====================================================");

            DST.ConsoleTransitionTimes(year);

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();


            Console.WriteLine("");
            Console.WriteLine("Function GetNextTransition testing ...");
            Console.WriteLine("================================================");

            List<DateTime> testDates = new List<DateTime>();
            testDates.Add(new DateTime(2017, 1, 1)); // Date is 1 January 2017 so next transition date should be 26 March 2017 for CEST(Central Europe Standard Time)
            testDates.Add(new DateTime(2017, 3, 27)); // Date is 27 March 2017 so next transition date should be 29 March 2017 for CEST(Central Europe Standard Time)
            testDates.Add(new DateTime(2017, 10, 29)); // Date is 29 October 2017 so next transition date should be 29 October 2017 for CEST(Central Europe Standard Time)

            string systemTimeZoneId = "Central Europe Standard Time";
            foreach (DateTime dt in testDates)
            {
                DateTime? result = DST.GetNextTransition(dt, systemTimeZoneId);
                Console.WriteLine(string.Format("Next transition date from {0} is {1}", dt, result));
            }

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }
    }
}
