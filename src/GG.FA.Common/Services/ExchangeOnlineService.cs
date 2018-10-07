using GG.FA.Common.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GG.FA.Common.Services
{
    public class ExchangeOnlineService
    {
     
		private readonly QueueService _queueService;

	    public ExchangeOnlineService(QueueService queueService)
	    {
		    _queueService = queueService;
	    }

	    public async Task<bool> AddUserToGroupAsync(string userPrincipalName, string groupId)
	    {

		    var values = new JObject
		    {
			    ["securityGroup"] = groupId,
			    ["userPrincipalName"] = userPrincipalName
		    };

		    await _queueService.CreateMessageAsync(values.ToString(Formatting.None));

		    return true;
	    }

		public async Task<bool> AddUserToWerteAkademieGroupAsync(string userPrincipalName)
        {
            var isUserAddedToGroup = await AddUserToGroupAsync(userPrincipalName, "a7b56b16-9cc7-4386-8fce-a2830b5fe119");

            return isUserAddedToGroup;
        }

	    public async Task<bool> AddUserToWerteBeiraeteGroupAsync(string userPrincipalName)
	    {
		    var isUserAddedToGroup = await AddUserToGroupAsync(userPrincipalName, "d5a50ddc-739e-46ac-97ee-9569872ea644");

		    return isUserAddedToGroup;
	    }

	}
}
