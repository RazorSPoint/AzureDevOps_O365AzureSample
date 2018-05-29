using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using GG.FA.Common.Interfaces;
using GG.FA.Common.Utilities;
using GG.FA.Model;
using Microsoft.Graph;

namespace GG.FA.Common.Services
{
    public class GraphService: IGraphService
    {
       
        /// <summary>   logger interface used to log. </summary>
        private readonly ILogger _log;

        /// <summary>   Instance of the graph client. </summary>
        private GraphServiceClient _graphClient = null;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the graph client instance. </summary>
        ///
        /// <value> The graph client. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public GraphServiceClient GraphClient => _graphClient;

        private const string Scope = "https://graph.microsoft.com/.default";

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor for the graph client. Exepcts a token and a ILogger instance. </summary>
        ///
        /// <remarks>   Sebastian Schütze, 07/04/2018. </remarks>
        ///
        /// <param name="graphToken">   the token string for the graph client for authentification. </param>
        /// <param name="log">          the logger instance that will be used in the graph client. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public GraphService(string graphToken, ILogger log)
        {
            _log = log;
            this.GetAuthenticatedClient(graphToken);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Get an access token for the given context and resourceId. An attempt is first made to acquire
        /// the token silently. If that fails, then we try to acquire the token by prompting the user.
        /// </summary>
        ///
        /// <remarks>   Sebastian Schütze, 07/04/2018. </remarks>
        ///
        /// <param name="graphToken">   the token string for the graph client for authentification. </param>
        ///
        /// <returns>   returns a the graph client instance. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public GraphServiceClient GetAuthenticatedClient(string graphToken)
        {
            if (GraphClient != null) return GraphClient;

            // Create Microsoft Graph client.
            try
            {
                _graphClient = new GraphServiceClient(
                    "https://graph.microsoft.com/v1.0",
                    new DelegateAuthenticationProvider(
                        async (requestMessage) =>
                        {
                            requestMessage.Headers.Authorization =
                                new AuthenticationHeaderValue("bearer", graphToken);
                        }));
                return GraphClient;
            }

            catch (Exception ex)
            {
                _log.Info("Could not create a graph client: " + ex.Message);
            }

            return GraphClient;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Creates a O365 user asynchronously. </summary>
        ///
        /// <remarks>   Sebastian Schütze, 07/04/2018. </remarks>
        ///
        /// <param name="user"> . </param>
        ///
        /// <returns>   An asynchronous result that yields the create user. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<string> CreateUserAsync(User user)
        {
            string createdUserId = null;

            try
            {
                //var organization = await GraphClient.Organization.Request().GetAsync();
                //var domain = organization.CurrentPage[0].VerifiedDomains.ElementAt(0).Name;

                var createdUser = await GraphClient.Users.Request().AddAsync(user);

                createdUserId = createdUser.Id;
                _log.Info("Created user: " + createdUserId);

            }
            catch (ServiceException e)
            {
                _log.Info("We could not create a user: " + e.Error.Message);
                return null;
            }

            return createdUserId;
        }

	    public async Task<User> GetUserAsync(string userPrincipalName)
	    {

		    var user = await GraphClient.Users[userPrincipalName].Request().GetAsync();

		    return user;
	    }

	    public async Task<string> ResetUserPasswordAsync(string userPrincipalName)
	    {
		    var newPassword = Security.ToInsecureString(Security.GetRandomPassword(8, true, true, true, true));

		    await GraphClient.Users[userPrincipalName].Request().UpdateAsync(new User()
		    {
			    PasswordProfile = new PasswordProfile()
			    {
				    Password = newPassword,
				    ForceChangePasswordNextSignIn = true
			    }
		    });
			
			return newPassword;
	    }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>   Adds a user to group asynchronously by 'groupId'. </summary>
		///
		/// <remarks>   Sebastian Schütze, 07/04/2018. </remarks>
		///
		/// <param name="userId">   Identifier for the user. </param>
		/// <param name="groupId">  Identifier for the group. </param>
		///
		/// <returns>   An asynchronous result that yields the add user to group. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public async Task<bool> AddUserToGroupAsync(string userId, string groupId)
        {
            var userAdded = false;
         
            try
            {
                var userToAdd = new User { Id = userId };

                await _graphClient.Groups[groupId].Members.References.Request().AddAsync(userToAdd);
                _log.Info("Added user " + userId + " to the group: " + groupId);
                userAdded = true;

            }

            catch (ServiceException e)
            {
                _log.Info("We could not add a user to the group: " + e.Error.Message);
                userAdded = false;
            }
            return userAdded;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets user from the custom SharePoint User List asynchronous. </summary>
        ///
        /// <remarks>   Sebastian Schütze, 07/04/2018. </remarks>
        ///
        /// <param name="siteId">       Identifier for the site. </param>
        /// <param name="listId">       Identifier for the list. </param>
        /// <param name="testuserOnly"> If true, test user only are returned. </param>
        ///
        /// <returns>   An asynchronous result that yields the user from SharePoint user list. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IEnumerable<ListItem>> GetUserFromSpUserListAsync(string siteId, string listId, bool testuserOnly)
        {
            IEnumerable<ListItem> userItems = new List<ListItem>();

            try
            {
                var request = _graphClient.Sites[siteId].Lists[listId].Items.Request()
                    .Expand("fields")
                    .Select("id,createdBy")
                    .Top(5000);

                request.Headers.Add(new HeaderOption("Prefer", "HonorNonIndexedQueriesWarningMayFailRandomly"));
                
                userItems = ((ListItemsCollectionPage) await request.GetAsync()).ToList();


                if (testuserOnly)
                {
                   userItems = userItems.Where(u =>
                   {
                       return
                           u.Fields.AdditionalData.ContainsKey("Testbenutzer")
                           && (bool) u.Fields.AdditionalData["Testbenutzer"];
                   });
                }
                
                userItems = userItems.Where(u => u.Fields.AdditionalData.ContainsKey("AccountStatus") && (string)u.Fields.AdditionalData["AccountStatus"] == "nicht angelegt");
            }

            catch (ServiceException e)
            {
                _log.Info("We could not get a users from the list: " + e.Error.Message);
            }

            return userItems;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Assign an E2 license to an O365 user by user id. </summary>
        ///
        /// <remarks>   Sebastian Schütze, 07/04/2018. </remarks>
        ///
        /// <param name="userId">   Identifier for the user. </param>
        ///
        /// <returns>   An asynchronous result that yields a User. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<User> AssignE2LicenseToUserById(string userId)
        {
            var skus = await GraphClient.SubscribedSkus.Request().GetAsync();
            var e2Sku = skus.FirstOrDefault(sku => sku.SkuPartNumber.Equals("STANDARDWOFFPACK"));

            var assignedLicense = new List<AssignedLicense>() { new AssignedLicense { SkuId = e2Sku.SkuId } };

            return await GraphClient.Users[userId].AssignLicense(assignedLicense, new Guid[0]).Request().PostAsync();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets access token with client secret. </summary>
        ///
        /// <remarks>   Sebastian Schütze, 07/04/2018. </remarks>
        ///
        /// <param name="clientId">     Identifier for the client. </param>
        /// <param name="clientSecret"> The client secret. </param>
        /// <param name="tenantId">     Identifier for the tenant. </param>
        ///
        /// <returns>   An asynchronous result that yields the access token with client secret. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static async Task<string> GetAccessTokenWithClientSecret(string clientId, string clientSecret, string tenantId)
        {
            // prepare request content
            var bodyContent = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("scope", Scope)
            };
         
            // use v2 of the graph client
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token")
            {
                Content = new FormUrlEncodedContent(bodyContent)
            };

            var httpAuthorize = new HttpClient();
            var response = await httpAuthorize.SendAsync(httpRequest);

            //convert the returned JSON to xml to be used with 
            // xpath to query without 3rd party solution
            var stringContent = await response.Content.ReadAsStringAsync();
            var jsonResponseContent = ConverterHelper.ConvertJsonStringToXElement(stringContent);
            var graphToken = jsonResponseContent.XPathSelectElement("//access_token");

            return graphToken.Value;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets ad user object from user list item. </summary>
        ///
        /// <remarks>   Sebastian Schütze, 07/04/2018. </remarks>
        ///
        /// <param name="listItem"> The list item. </param>
        ///
        /// <returns>   The ad user object from user list item. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static User GetAdUserObjectFromUserListItem(ListItem listItem)
        {
            var fields = listItem.Fields.AdditionalData;

            var title = fields.ContainsKey("Title") ? (string)fields["Title"] : "";
            var firstname = fields.ContainsKey("Vorname") ? (string)fields["Vorname"] : "";
            var lastname = fields.ContainsKey("Nachname") ? (string)fields["Nachname"] : "";
            var department = fields.ContainsKey("Abteilung") ? (string)fields["Abteilung"] : "";
            var segment = fields.ContainsKey("Segment") ? (string)fields["Segment"] : "";
            var state = fields.ContainsKey("Bundesland") ? (string)fields["Bundesland"] : "";
            var alternateEmail = fields.ContainsKey("E_x002d_Mail") ? (string)fields["E_x002d_Mail"] : "";
            var homePhone = fields.ContainsKey("Festnetz") ? (string)fields["Festnetz"] : "";
            var year = fields.ContainsKey("Jahrgang") ? (string)fields["Jahrgang"] : "";
            var city = fields.ContainsKey("Ort") ? (string)fields["Ort"] : "";
            var mobilePhone = fields.ContainsKey("Mobiltelefon") ? (string)fields["Mobiltelefon"] : "";
            var forwardEmail = fields.ContainsKey("WeiterleitungAktiv") && (bool)fields["WeiterleitungAktiv"];
            var wKBereich = fields.ContainsKey("WKBereich") ? (string)fields["WKBereich"] : "";

            var userPrincipalName = O365UserPropertyHelper.UserPrincipalName(firstname, lastname, department);
            var searchableDisplayName = O365UserPropertyHelper.GetSearchableDisplayName(title, firstname, lastname);
            var displayName = O365UserPropertyHelper.GetDisplayName(title, firstname, lastname);
          
            return new WkUser()
            {
                AccountEnabled = true,
                UserPrincipalName = userPrincipalName,
                MobilePhone = homePhone,
                BusinessPhones = new[] { mobilePhone },
                Department = department,
                PasswordPolicies = "DisablePasswordExpiration",
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = true,
                    Password = Security.ToInsecureString(Security.GetRandomPassword(8, true, true, true, true))
                },
                MailNickname = $"{firstname}{lastname[0].ToString().ToUpper()}",
                DisplayName = searchableDisplayName,
                City = city,
                State = state,
                UsageLocation = "DE"
            };
        }
    }

    public interface IGraphService
    {
	    GraphServiceClient GraphClient { get; }

		GraphServiceClient GetAuthenticatedClient(string graphToken);

        Task<string> CreateUserAsync(User user);
        
        Task<bool> AddUserToGroupAsync(string userId, string groupId);
        
        Task<IEnumerable<ListItem>> GetUserFromSpUserListAsync(string siteId, string listId,bool testuserOnly);
        
        Task<User> AssignE2LicenseToUserById(string userId);
    }
}
