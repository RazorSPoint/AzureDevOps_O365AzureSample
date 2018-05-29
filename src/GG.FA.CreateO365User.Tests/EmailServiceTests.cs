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
        public void SendMailAsync_Successful()
        {
            var clientId = Properties.Common.Default.AppClientID;
            var clientSecret = Properties.Common.Default.AppClientSecret;
            var tenantId = Properties.Common.Default.TenantId;

            Task.Run(async () =>
            {
                var graphToken = await GraphService.GetAccessTokenWithClientSecret(clientId, clientSecret, tenantId);
                var graphService = new GraphService(graphToken, new TestLogger());

	            var emailService = new EmailService(graphService, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates"));

	            var user = await graphService.GetUserAsync("sebastian.schuetze@***REMOVED***");
	            var admin = await graphService.GetUserAsync("sebastian.schuetze@***REMOVED***");

				var password = await graphService.ResetUserPasswordAsync("***REMOVED***");

	            emailService.SendPasswordMailAsync(user, admin, password);

			}).GetAwaiter().GetResult();
        }
    }
}
