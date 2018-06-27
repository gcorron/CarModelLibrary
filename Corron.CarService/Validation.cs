using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corron.CarService
{
    internal static class Validation
    {
        public static string ValidateCostString(string cost, out decimal dcost)
        {

            if (!decimal.TryParse(cost, out dcost))
                return "Non-numeric value.";


            int i = cost.IndexOf('.');
            if (i < 0)
                return ValidateCost(dcost);

            if (i == cost.Length - 3)
                return ValidateCost(dcost);
            else
            {
                dcost = 0;
                return "Invalid currency amount.";
            }
        }

        public static string ValidateCost(decimal cost)
        {
            if (cost < 0)
                return "Amount must not be negative.";
            else
                return null;
        }

        public static string FiftyNoBlanks(string Test)
        {
            if (String.IsNullOrWhiteSpace(Test))
                return "Field must not be blank.";
            else if (Test.Length > 50)
                return "Field is too long.";
            else
                return null;
        }
    }
}
