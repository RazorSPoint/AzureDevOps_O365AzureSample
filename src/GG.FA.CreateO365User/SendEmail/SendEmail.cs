using System.Threading.Tasks;
using GG.FA.Common.Services;
using GG.FA.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Graph;

namespace GG.FA.CreateO365User.SendEmail
{
    public static class SendEmail
    {
        [FunctionName("SendEmail")]
        public static async Task<string> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = null
            )]
            HttpRequest req,
            [Token(
                Identity = TokenIdentityMode.ClientCredentials,
                IdentityProvider = "AAD",
                Resource = "https://graph.microsoft.com"
            )]
            string graphToken,
            TraceWriter log, 
            ExecutionContext context)
        {

            var azureFunctionsLogger = new AzureFunctionLogger(log);
            var graphService = new GraphService(graphToken, azureFunctionsLogger);

	        var emailService = new EmailService(graphService);
			
	        var emailString = "sebastian.schuetze@***REMOVED***";
	        var displayName = "Sebastian Schütze";
			var toMail = new[] { emailString + ";Sebastian Schütze" };
	        var body = emailService.GetMailTemplateByFile($@"{context.FunctionDirectory}\Templates\WK_EmailTemplate.html")
		        .Replace("#GGEmail#", emailString)
				.Replace("#GGPassword#", "MyPassword")
				.Replace("#GGDisplayName#", displayName);

	        var subject = "Test Subject";

			emailService.SendMailAsync(emailString, toMail,toMail, emailString, subject, body);
			
            return "Mails was sent!";
        }


    }
}