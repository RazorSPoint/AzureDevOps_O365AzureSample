using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GG.FA.Common.Interfaces;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core.Utilities;
using ListItem = Microsoft.SharePoint.Client.ListItem;

namespace GG.FA.Common.Services
{
    public class SharePointService
	{
		/// <summary>   logger interface used to log. </summary>
		private readonly ILogger _log;


		/// <summary>	Context for the SharePoint client. </summary>
		private ClientContext _clientContext;

		/// <summary>	URL of the tenant. </summary>
		private string _tenantUrl;

		public SharePointService(string tenantUrl, string accessToken, ILogger log)
	    {
		    _log = log;
		    _tenantUrl = tenantUrl;

		    Task.Run(async () =>
		    {
				_clientContext = await this.GetClientContextWithAccessToken(tenantUrl,accessToken);
			}).GetAwaiter().GetResult();

		}

		private async Task<ClientContext> GetClientContextWithAccessToken(string tenantUrl, string accessToken)
		{
			string aadInstance = "https://login.microsoftonline.com";
			string tenant = "gutgoedelitz.onmicrosoft.com";
			string clientId = "***REMOVED***";
			string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
			string sharePointUrl = "***REMOVED***";

			var authContext = new AuthenticationContext(authority);

			var result = await authContext.AcquireTokenAsync(sharePointUrl, clientId, new Uri(sharePointUrl),
				new PlatformParameters());

			var ctx =
				new ClientContext(tenantUrl)
				{
					AuthenticationMode = ClientAuthenticationMode.Anonymous,
					FormDigestHandlingEnabled = false
				};

			ctx.ExecutingWebRequest +=
				delegate(object oSender, WebRequestEventArgs webRequestEventArgs)
				{
					webRequestEventArgs.WebRequestExecutor.RequestHeaders["Authorization"] =
						"Bearer " + result.AccessToken;
				};

			return ctx;
		}

		public async Task<IEnumerable<ListItem>> GetTemplateFromTemplateListByNameAsync(string templateName)
		{
			IEnumerable<ListItem> userItems = new List<ListItem>();

			try
			{
				var ctx = _clientContext;

				var stringQuery = CAML.ViewQuery(
					CAML.Where(
						CAML.Eq(
							CAML.FieldValue("Title","Text", templateName)
						)
					)
				);

				var camlQuery = new CamlQuery {ViewXml = stringQuery};

				var templateItems = ctx.Web.Lists.GetByTitle("WkBereich");//.GetItems(camlQuery);
				ctx.ExecuteQuery();

			/*	foreach (var templateItem in templateItems)
				{
					if (templateItem.AttachmentFiles.Any())
					{
						var test = "";
					}
				}*/

			}

			catch (ServiceException e)
			{
				_log.Info("We could not get templates from the list: " + e.Error.Message);
			}

			return userItems;
		}
	}
}
