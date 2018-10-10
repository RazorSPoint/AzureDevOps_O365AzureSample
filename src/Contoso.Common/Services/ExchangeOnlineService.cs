using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Contoso.Common.Services
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

	}
}
