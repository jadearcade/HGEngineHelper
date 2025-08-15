using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HgEngineCsvConverter.Code
{
    public static class Extensions
    {
        public static int ToInt(this string input)
        {
            if (int.TryParse(input, out int result))
            {
                return result;
            }
            // Handle cases where conversion fails, e.g., return 0 or throw an exception
            return 0; // Or throw new FormatException("Input string was not in a correct format.");
        }

        public static int? ToIntNullable(this string input)
        {
            if (int.TryParse(input, out int result))
            {
                return result;
            }
            // Handle cases where conversion fails, e.g., return 0 or throw an exception
            return null; // Or throw new FormatException("Input string was not in a correct format.");
        }
    }

    public class BoolResultWithMessage
    {
        public bool successful = false;
        public string message = "";
        public BoolResultWithMessage()
        {

        }
        public BoolResultWithMessage(bool s, string m)
        {
            successful = s; message = m;
        }
    }
}
