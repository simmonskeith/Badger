using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger.Core
{
    public static class Verify
    {
        /// <summary>
        /// Compares the actual value to the expected value.
        /// </summary>
        /// <param name="actual">the string value to test</param>
        /// <param name="expected">the expected string value</param>
        /// <param name="comment">an optional comment</param>
        /// <returns>result of the comparison (true = values are equal)</returns>
        public static bool AreEqual(string actual, string expected, string comment="")
        {
            bool result = false;
            if (!string.IsNullOrEmpty(comment))
            {
                comment = ", " + comment;
            }
            if (actual.Equals(expected))
            {
                Log.Pass($"'{actual}' equals expected '{expected}'{comment}");
                result = true;
            }
            else
            {
                Log.Fail($"'{actual}' does not equal expected '{expected}'{comment}");
            }
            return result;
        }

        /// <summary>
        /// Compares the actual value to a regex pattern.
        /// </summary>
        /// <param name="actual">the string value to test</param>
        /// <param name="pattern">the regex expression</param>
        /// <param name="comment">an optional comment</param>
        /// <returns>result of the match (true = actual value matches the pattern)</returns>
        public static bool Matches(string actual, string pattern, string comment="")
        {
            bool result = false;
            if (!string.IsNullOrEmpty(comment))
            {
                comment = ", " + comment;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(actual, pattern))
            {
                Log.Pass($"'{actual}' matches pattern '{pattern}'{comment}");
                result = true;
            }
            else
            {
                Log.Fail($"'{actual}' does not match pattern '{pattern}'{comment}");
            }
            return result;
        }
        
        /// <summary>
        /// Compares value for truthiness.
        /// </summary>
        /// <param name="value">the value tested for truthiness</param>
        /// <param name="comment">an optional comment</param>
        /// <returns></returns>
        public static bool IsTrue(bool value, string comment="")
        {
            bool result = false;
            if (!string.IsNullOrEmpty(comment))
            {
                comment = ", " + comment;
            }
            string message = $"Actual: {value}, Expected: true{comment}";

            if (value == true)
            {
                Log.Pass(message);
                result = true;
            }
            else
            {
                Log.Fail(message);
            }
            return result;
        }

        /// <summary>
        /// Compares value for falseness.
        /// </summary>
        /// <param name="value">the value tested for falseness</param>
        /// <param name="comment">an optional comment</param>
        /// <returns></returns>
        public static bool IsFalse(bool value, string comment = "")
        {
            bool result = false;
            if (!string.IsNullOrEmpty(comment))
            {
                comment = ", " + comment;
            }
            string message = $"Actual: {value}, Expected: false{comment}";

            if (value == false)
            {
                Log.Pass(message);
                result = true;
            }
            else
            {
                Log.Fail(message);
            }
            return result;
        }
    }
}
