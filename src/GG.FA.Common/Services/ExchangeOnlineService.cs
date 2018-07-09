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
	    private readonly string _exchangeAdmin;
	    private readonly SecureString _adminPassword;

	    public ExchangeOnlineService(QueueService queueService,string exchangeAdmin, SecureString adminPassword)
	    {
		    _queueService = queueService;
			_exchangeAdmin = exchangeAdmin;
			_adminPassword = adminPassword;
		}

        public async Task<bool> AddUserToGroupAsync(string userPrincipalName,string groupId,string userName, SecureString securePassword)
        {
		
	        JObject values = new JObject
	        {
		        ["securityGroup"] = groupId,
		        ["userPrincipalName"] = userPrincipalName,
		        ["adminUser"] = userName,
		        ["password"] = Security.ToInsecureString(securePassword)
			};

	        await _queueService.CreateMessageAsync(values.ToString(Formatting.None));

           // var content = new FormUrlEncodedContent(values);
	        //Client.Headers["content-type"] = "application/json";
	        //var reqString = Encoding.Default.GetBytes(JsonConvert.SerializeObject(values, Formatting.Indented));

	       // var resposnseByte = Client.UploadData(_queueService.AbsoluteUri, "post", reqString);
	       // var responseString = Encoding.Default.GetString(resposnseByte);
			
            return true;
        }

        public async Task<bool> AddUserToWerteAkademieGroupAsync(string userPrincipalName)
        {
            var isUserAddedToGroup = await AddUserToGroupAsync(userPrincipalName, "a7b56b16-9cc7-4386-8fce-a2830b5fe119", _exchangeAdmin, _adminPassword);

            return isUserAddedToGroup;
        }

	    public async Task<bool> AddUserToWerteBeiraeteGroupAsync(string userPrincipalName)
	    {
		    var isUserAddedToGroup = await AddUserToGroupAsync(userPrincipalName, "d5a50ddc-739e-46ac-97ee-9569872ea644", _exchangeAdmin, _adminPassword);

		    return isUserAddedToGroup;
	    }

	}
}
