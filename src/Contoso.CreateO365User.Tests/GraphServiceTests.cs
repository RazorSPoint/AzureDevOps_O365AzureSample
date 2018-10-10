using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Contoso.Common.Services;
using Contoso.Common;
using GG.FA.CreateO365User.Tests.Mockup;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GG.FA.CreateO365User.Tests
{
    [TestClass]
    public class GraphServiceTests
    {
        [TestMethod]
        public void GetAccessToken_NotEmptyOrNull()
        {
            var clientId = Properties.Common.Default.AppClientID;
            var clientSecret = Properties.Common.Default.AppClientSecret;
            var tenantId = Properties.Common.Default.TenantId;

            Task.Run(async () =>
            {
                var graphToken = await GraphService.GetAccessTokenWithClientSecret(clientId, clientSecret, tenantId);
                Assert.IsTrue(!string.IsNullOrEmpty(graphToken));
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void GetTestUserOnlyListItems(bool testUserOnly)
        {
            var clientId = Properties.Common.Default.AppClientID;
            var clientSecret = Properties.Common.Default.AppClientSecret;
            var tenantId = Properties.Common.Default.TenantId;

            // site: ***REMOVED***/sites/gut-goedelitz/UserAdminiatration
            var siteId = Properties.Common.Default.SiteIdUserList;
            // list: ***REMOVED***/sites/gut-goedelitz/UserAdminiatration/Lists/UserInventory
            var listId = Properties.Common.Default.ListIdUserList;

            Task.Run(async () =>
            {
                var graphToken = await GraphService.GetAccessTokenWithClientSecret(clientId, clientSecret, tenantId);
                
                var graphService = new GraphService(graphToken, new TestLogger());
                
                var users = await graphService.GetUserFromSpUserListAsync(siteId,listId, testUserOnly);

                var onlyOneTypeUser = users.Count(u =>
                {
                    var fields = u.Fields.AdditionalData;
                    return fields.ContainsKey("Testbenutzer") && (bool) fields["Testbenutzer"] != testUserOnly || !fields.ContainsKey("Testbenutzer");
                }) == 0;

                Assert.AreEqual(testUserOnly, onlyOneTypeUser);

            }).GetAwaiter().GetResult();
        }
    }
}
