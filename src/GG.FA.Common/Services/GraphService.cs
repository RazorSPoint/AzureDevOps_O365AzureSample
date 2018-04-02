using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using GG.FA.Common.Interfaces;
using GG.FA.Common.Utilities;
using Microsoft.Graph;

namespace GG.FA.Common.Services
{
    public class GraphService
    {
       
        /// <summary>
        /// logger interface used to log
        /// </summary>
        private readonly ILogger _log;

        /// <summary>
        /// Instance of the graph client
        /// </summary>
        private GraphServiceClient _graphClient = null;
 
        /// <summary>
        /// Gets the graph client instance
        /// </summary>
        public GraphServiceClient GraphClient => _graphClient;

        private const string Scope = "https://graph.microsoft.com/.default";

        /// <summary>
        /// Constructor for the graph client. Exepcts a token and a ILogger instance.
        /// </summary>
        /// <param name="graphToken">the token string for the graph client for authentification</param>
        /// <param name="log">the logger instance that will be used in the graph client</param>
        public GraphService(string graphToken, ILogger log)
        {
            _log = log;
            this.GetAuthenticatedClient(graphToken);
        }

        /// <summary>
        /// Get an access token for the given context and resourceId. An attempt is first made to 
        /// acquire the token silently. If that fails, then we try to acquire the token by prompting the user.
        /// </summary>
        /// <param name="graphToken">the token string for the graph client for authentification</param>
        /// <returns>returns a the graph client instance</returns>
        private GraphServiceClient GetAuthenticatedClient(string graphToken)
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

        /// <summary>
        /// Creates a O365 user asynchronously
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <see cref="https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/user"/>
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

        public async Task<bool> AddUserToGroupAsync(string userId, string groupId)
        {
            var userAdded = false;
         
            try
            {
                User userToAdd = new User { Id = userId };

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
        public async Task<IListItemsCollectionPage> GetUserFromSpUserListAsync(string siteId, string listId, bool testuserOnly)
        {
            var userItems = new ListItemsCollectionPage();

            try
            {
                var request = _graphClient.Sites[siteId].Lists[listId].Items.Request()
                    .Expand("fields")
                    .Select("id,createdBy")
                    .Top(5000);

                request.Headers.Add(new HeaderOption("Prefer", "HonorNonIndexedQueriesWarningMayFailRandomly"));

                if (testuserOnly)
                {
                    request = request.Filter("fields/Testbenutzer eq True");
                }
                else
                { 
                    request = request.Filter("fields/AccountStatus eq 'nicht angelegt'");
                }
                
                userItems = (ListItemsCollectionPage) await request.GetAsync();
            }

            catch (ServiceException e)
            {
                _log.Info("We could not get a users from the list: " + e.Error.Message);
            }

            return userItems;
        }

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

            return new User()
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
}
