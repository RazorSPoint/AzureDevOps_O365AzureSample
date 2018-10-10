using System;
using System.Collections.Generic;
using System.Text;

namespace GG.FA.Common.Utilities
{
    public static class O365UserPropertyHelper
    {
        public static string GetReplacedSpecialChars(string stringToReplace, bool toLowerCase)
        {
            var specialChars = new string[]
            {
                "Ä", "Ö", "Ü", "ä", "ö", "ü", "ß", "é", "è", "á", "à", " "
            };
            var replaceChars = new string[]
            {
                "Ae", "Oe", "Ue", "ae", "oe", "ue", "ss", "e", "e", "a", "a", ""
            };


            var tmpString = stringToReplace;
            for (var i = 0; i < specialChars.Length; i++)
            {
                tmpString = tmpString.Replace(specialChars[i], replaceChars[i]);
            }

            if (toLowerCase)
            {
                tmpString = tmpString.ToLower();
            }

            return tmpString;

        }

        public static string GetDisplayName(string title, string firstName, string lastName)
        {
            return $"{title} {firstName} {lastName}";
        }

        public static string GetSearchableDisplayName(string title, string firstName, string lastName)
        {
            return $"{lastName}, {title} {firstName}";
        }

        public static string UserPrincipalName(string firstName, string lastName, string domain)
        {
            var secureFirstName = GetReplacedSpecialChars(firstName, false);
            var secureLastName = GetReplacedSpecialChars(lastName, false);

            return $"{secureFirstName}.{secureLastName}@{domain}";
        }
    }
}
