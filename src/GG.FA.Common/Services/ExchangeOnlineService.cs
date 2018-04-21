using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GG.FA.Common.Services
{
    public class ExchangeOnlineService
    {
        private static readonly HttpClient Client = new HttpClient();

        private const string FunctionCode = "CfDJ8AAAAAAAAAAAAAAAAAAAAACglueNHq0WFGxOe4BPL8UVAF7ZYS4miVaocFsbwJchkUBdUAdtnt1RQypIwqLTmwThBZqS09Z1wDdpRXzJDoerF5G1Da31KsG0uqCwdji17GiTAzkk7XzzBAso9rET72EaNl5xF74x-s0YSxLxHxTtsJeLRm4Ryz7daHX7UE-ePw";

        private static readonly Uri AzureFunctionUrl = new Uri($"https://***REMOVED***.azurewebsites.net/api/O365NewUserOrchestration?code={FunctionCode}");

        public static async Task<bool> AddUserToGroupAsync(string userPrincipalName,string groupId)
        {
            var values = new Dictionary<string, string>
            {
                { "securityGroup", groupId },
                { "userPrincipalName", userPrincipalName }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await Client.PostAsync(AzureFunctionUrl.AbsoluteUri, content);

            var responseString = await response.Content.ReadAsStringAsync();

            return true;
        }

        public static async Task<bool> AddUserToWerteAkademieGroupAsync(string userPrincipalName)
        {
            var isUserAddedToGroup = await AddUserToGroupAsync(userPrincipalName, "d5a50ddc-739e-46ac-97ee-9569872ea644");

            return isUserAddedToGroup;
        }

    }
}
