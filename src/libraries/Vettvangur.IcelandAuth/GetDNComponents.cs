using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vettvangur.IcelandAuth
{
    /// <summary>
    /// https://github.com/dotnet/corefx/blob/c539d6c627b169d45f0b4cf1826b560cd0862abe/src/System.DirectoryServices/src/System/DirectoryServices/ActiveDirectory/Utils.cs#L440-L449
    /// </summary>
    static class ADUtils
    {

        internal struct Component
        {
            public string Name;
            public string Value;
        }

        //
        // this method breaks up the string into tokens based on the delimiter
        // (escaped characters are those preceded by '\' or contained in quotes and
        // such characters are not considered for a match with the delimiter)
        //
        public static string[] Split(string distinguishedName, char delim)
        {
            bool inQuotedString = false;
            char curr;
            char quote = '\"';
            char escape = '\\';
            int nextTokenStart = 0;
            ArrayList resultList = new ArrayList();
            string[] results;

            // get the actual tokens
            for (int i = 0; i < distinguishedName.Length; i++)
            {
                curr = distinguishedName[i];

                if (curr == quote)
                {
                    inQuotedString = !inQuotedString;
                }
                else if (curr == escape)
                {
                    // skip the next character (if one exists)
                    if (i < (distinguishedName.Length - 1))
                    {
                        i++;
                    }
                }
                else if ((!inQuotedString) && (curr == delim))
                {
                    // we found an unqoted character that matches the delimiter
                    // split it at the delimiter (add the tokrn that ends at this delimiter)
                    resultList.Add(distinguishedName.Substring(nextTokenStart, i - nextTokenStart));
                    nextTokenStart = i + 1;
                }

                if (i == (distinguishedName.Length - 1))
                {
                    // we've reached the end 

                    // if we are still in quoted string, the format is invalid
                    if (inQuotedString)
                    {
                        throw new ArgumentException("Invalid DN format", nameof(distinguishedName));
                    }

                    // we need to end the last token
                    resultList.Add(distinguishedName.Substring(nextTokenStart, i - nextTokenStart + 1));
                }
            }

            results = new string[resultList.Count];
            for (int i = 0; i < resultList.Count; i++)
            {
                results[i] = (string)resultList[i];
            }

            return results;
        }


        //
        // Splits up a DN into it's components
        // e.g. cn=a,cn=b,dc=c,dc=d would be returned as 
        // a component array
        // components[0].name = cn
        // components[0].value = a
        // components[1].name = cn
        // components[1].value = b ... and so on
        // 
        internal static Component[] GetDNComponents(string distinguishedName)
        {
            Debug.Assert(distinguishedName != null, "Utils.GetDNComponents: distinguishedName is null");

            // First split by ','
            string[] components = Split(distinguishedName, ',');
            Component[] dnComponents = new Component[components.GetLength(0)];

            for (int i = 0; i < components.GetLength(0); i++)
            {
                // split each component by '='
                string[] subComponents = Split(components[i], '=');
                if (subComponents.GetLength(0) != 2)
                {
                    throw new ArgumentException("Invalid DN format", nameof(distinguishedName));
                }

                dnComponents[i].Name = subComponents[0].Trim();
                if (dnComponents[i].Name.Length == 0)
                {
                    throw new ArgumentException("Invalid DN format", nameof(distinguishedName));
                }

                dnComponents[i].Value = subComponents[1].Trim();
                if (dnComponents[i].Value.Length == 0)
                {
                    throw new ArgumentException("Invalid DN format", nameof(distinguishedName));
                }
            }
            return dnComponents;
        }
    }
}
