using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Xml.Serialization;

namespace GG.FA.Common.Utilities
{
    public class Security
    {
        private static readonly byte[] Entropy = System.Text.Encoding.Unicode.GetBytes("SomeSaltHereIsNotAPassword");

        /// <summary>
        /// Encrypts the string with an entropy (salt). Only user that has encrypted the string can decrypt it.
        /// </summary>
        /// <param name="input">SecureString to be encrypted</param>
        /// <returns>encrypted Base64 string</returns>
        /// <see cref="https://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file"/>
        public static string EncryptString(System.Security.SecureString input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
                Entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Decrypts the encrypted string with an entropy (salt). Only works for the same user that has encrypted the string.
        /// </summary>
        /// <param name="encryptedData">the encrypted string</param>
        /// <returns>the decrypted SecureString</returns>
        /// <see cref="https://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file"/>
        public static SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    Entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }

        /// <summary>
        /// Converts a string to a SecureString.
        /// </summary>
        /// <param name="input">the string to be converted</param>
        /// <returns>the converted SecureString</returns>
        /// <see cref="https://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file"/>
        public static SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
         
            return secure;
        }

        /// <summary>
        /// Converts SecureString to normale string
        /// </summary>
        /// <param name="input">the SecureString to convert</param>
        /// <returns>a converted string</returns>
        /// <see cref="https://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file"/>
        public static string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }

        /// <summary>
        /// Reads the password from the console in a more secure way without showing the password.
        /// </summary>
        /// <returns>Returns the typed password as string</returns>
        /// <see cref="https://stackoverflow.com/questions/3404421/password-masking-console-application"/>
        public static string ReadPasswordFromConsole(string consoleMessage)
        {
            var password = "";
            var key = new ConsoleKeyInfo();

            Console.Write(consoleMessage);

            do
            {
                key = Console.ReadKey(true);

                // Append password if not Backspace of Enter
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                // Delete last typed character if Backspace
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, (password.Length - 1));
                    Console.Write("\b \b");
                }



            }
            // stop when enter is used
            while (key.Key != ConsoleKey.Enter);

            return password;
        }

        /// <summary>
        /// Reads the password from the console in a more secure way without showing the password. Returns a secure string if needed.
        /// </summary>
        /// <returns>Returns the typed password as SecureString</returns>
        /// <see cref="https://stackoverflow.com/questions/3404421/password-masking-console-application"/>
        public static SecureString ReadSecurePasswordFromConsole(string consoleMessage)
        {
            return ToSecureString(ReadPasswordFromConsole(consoleMessage));
        }

        public static SecureString GetRandomPassword(int length, bool includeLowerCase = false, bool includeUpperCase = false,
            bool includeNumbers = false, bool includeSpecialChars = false)
        {
            if (length < 4)
            {
                throw new Exception("The minimum password length is 4");
            }

            if (!includeLowerCase &&
                !includeNumbers &&
                !includeSpecialChars &&
                !includeUpperCase)
            {
                throw new Exception("At least one set of included characters must be specified");
            }

            var charsToSkip = new List<char>()
            {
                'i',
                'l',
                'o',
                '1',
                '0',
                'I'
            };


            var uppercaseChars = new List<char>();
            for (var a = 65; a < 90; a++)
            {
                if (!charsToSkip.Contains((char) a))
                {
                    uppercaseChars.Add((char) a);
                }
            }

            var lowercaseChars = new List<char>();
            for (var a = 97; a < 122; a++)
            {
                if (!charsToSkip.Contains((char) a))
                {
                    lowercaseChars.Add((char) a);
                }
            }

            var digitCahrs = new List<char>();
            for (var a = 48; a < 57; a++)
            {
                if (!charsToSkip.Contains((char) a))
                {
                    digitCahrs.Add((char) a);
                }
            }

            var specialChars = new List<char>() {'=', '+', '_', '?', '!', '-', '#', '$', '*', '&', '@'};

            var templateLetters = "";

            templateLetters += includeLowerCase ? "L" : "";
            templateLetters += includeUpperCase ? "U" : "";
            templateLetters += includeNumbers ? "N" : "";
            templateLetters += includeSpecialChars ? "S" : "";

            var passwordTemplate = new List<string>();

            do
            {
                var randomTplLetter = new Random();
                passwordTemplate.Clear();
                for (var loop = 1; loop <= length; loop++)
                {
                    passwordTemplate.Add(
                        templateLetters.Substring(
                            randomTplLetter.Next(templateLetters.Length), 1
                        )
                    );
                }

            } while (!(
                (!includeLowerCase || passwordTemplate.Contains("L")) &&
                (!includeUpperCase || passwordTemplate.Contains("U")) &&
                (!includeNumbers || passwordTemplate.Contains("N")) &&
                (!includeSpecialChars || passwordTemplate.Contains("S"))
            ));

            var password = "";
            var randomChar = new Random();
            foreach (var ch in passwordTemplate)
            {
                switch (ch)
                {
                    case "L":
                        password += lowercaseChars[randomChar.Next(lowercaseChars.Count - 1)];
                        break;
                    case "U":
                        password += uppercaseChars[randomChar.Next(uppercaseChars.Count - 1)];
                        break;
                    case "N":
                        password += digitCahrs[randomChar.Next(digitCahrs.Count - 1)];
                        break;
                    case "S":
                        password += specialChars[randomChar.Next(specialChars.Count - 1)];
                        break;
                    default:
                        throw new Exception("Password template option not supported");
                }
            }

            return ToSecureString(password);

        }
    }
}
