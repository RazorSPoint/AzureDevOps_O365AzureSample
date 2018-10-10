using System;
using System.Reflection;
using Contoso.Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GG.FA.CreateO365User.Tests
{
    [TestClass]
    public class O365UserPropertyHelperTests
    {
        [TestMethod]
        [DataRow("Prof.", "Sebastian", "Schütze")]
        [DataRow("", "Sebastian", "Müller")]
        [DataRow("", "", "")]
        public void GetDisplayName_Multiple(string title, string firstname, string lastname)
        {
            var displayname = O365UserPropertyHelper.GetDisplayName(title, firstname, lastname);

            Assert.AreEqual($"{title} {firstname} {lastname}", displayname);
        }

        [TestMethod]
        [DataRow("Prof.", "Sebastian", "Schütze")]
        [DataRow("", "Sebastian", "Müller")]
        [DataRow("", "", "")]
        public void GetSearchableDisplayName_Multiple(string title, string firstname, string lastname)
        {
            var displayname = O365UserPropertyHelper.GetSearchableDisplayName(title, firstname, lastname);

            Assert.AreEqual($"{lastname}, {title} {firstname}", displayname);
        }

        [TestMethod]
        [DataRow("Sebastian", "Schütze", "***REMOVED***")]
        [DataRow("Sebastian", "Müller", "***REMOVED***")]
        [DataRow("$&§/U6u3265z", "%§&/$", "***REMOVED***")]
        [DataRow("", "", "")]
        public void GetUserPrincipalName_Multiple(string firstname, string lastname, string domain)
        {
            var userPrincipalName = O365UserPropertyHelper.UserPrincipalName(firstname, lastname, domain);

            var secureFirstname = O365UserPropertyHelper.GetReplacedSpecialChars(firstname, false);
            var secureLastname = O365UserPropertyHelper.GetReplacedSpecialChars(lastname, false);

            Assert.AreEqual($"{secureFirstname}.{secureLastname}@{domain}", userPrincipalName);
        }

        [TestMethod]
        [DataRow("Schütze", "Schuetze")]
        [DataRow("ÄöÜß", "AeoeUess")]
        public void ReplaceSpecialChars_Multiple(string stringChars, string expectedStringChars)
        {
            var secureStringChars = O365UserPropertyHelper.GetReplacedSpecialChars(stringChars, false);

            Assert.AreEqual(expectedStringChars, secureStringChars);
        }
    }
}
