using System;
using System.Text;
using Contoso.Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GG.FA.CreateO365User.Tests
{
    [TestClass]
    public class SecurityTests
    {
        [TestMethod]
        [DataRow(8,true, false, false, true)]
        [DataRow(8, false,true,false,false)]
        [DataRow(8, false, false,true,false)]
        [DataRow(8, false, false,false,true)]
        public void GetRandomPassword_Length8(int length, bool includeLowerCase, bool includeUpperCase,
            bool includeNumbers, bool includeSpecialChars)
        {
            var expectedLenth = length;
            var password = Security.GetRandomPassword(length, includeLowerCase, includeUpperCase, includeNumbers, includeSpecialChars);

            Assert.AreEqual(expectedLenth,password.Length);
        }

        [TestMethod]
        public void GetRandomPassword_WithLowerUpperNumberAndSpecialChars()
        {
            const int expectedLenth = 8;
            var password = Security.GetRandomPassword(8,true,true,true,true);

            Assert.AreEqual(expectedLenth, password.Length);
        }
    }
}
