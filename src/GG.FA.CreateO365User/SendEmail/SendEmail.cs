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
            TraceWriter log)
        {

            var azureFunctionsLogger = new AzureFunctionLogger(log);
            var graphService = new GraphService(graphToken, azureFunctionsLogger);

            var recipient = new Recipient()
            {
                EmailAddress = new EmailAddress()
                {
                    Address = "sebastian.schuetze@***REMOVED***",
                    Name = "Sebastian Schütze"
                }
            };

            var replyTo = new Recipient()
            {
                EmailAddress = new EmailAddress()
                {
                    Address = "sebastian.schuetze@***REMOVED***",
                    Name = "IT Administration"
                }
            };

            var body = new ItemBody()
            {
                Content = "TestBody",
                ContentType = BodyType.Html
            };


            var mail = new Message()
            {
                Subject = "",
                ToRecipients = new[] {recipient},
                CcRecipients = new[] {recipient},
                Body = body,
                From = recipient,
                ReplyTo = new[] {replyTo}
            };


            await graphService.GraphClient.Users["sebastian.schuetze@***REMOVED***"].SendMail(mail, true)
                .Request().PostAsync();
         
            return "true!";
        }


    }
}