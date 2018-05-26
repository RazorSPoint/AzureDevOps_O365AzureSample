using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GG.FA.Common;
using GG.FA.Common.Services;
using GG.FA.CreateO365User.Tests.Mockup;
using GG.FA.Model;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GG.FA.CreateO365User.Tests
{
    [TestClass]
    public class EmailServiceTests
	{

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SendMailAsync_Successful(bool testUserOnly)
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

	            var currUserItems = await graphService.GetUserFromSpUserListAsync(siteId, listId, true);
				var emailService = new EmailService(graphService);
				
	            foreach (var currUserItem in currUserItems)
	            {
		            var user = GraphService.GetAdUserObjectFromUserListItem(currUserItem);

		            emailService.SendPasswordMailAsync((WkUser)user);
	            }

			}).GetAwaiter().GetResult();
        }
    }
}
