using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using GG.FA.Common.Utilities;
using GG.FA.Model;
using Microsoft.Graph;

namespace GG.FA.Common.Services
{
    public class EmailService
    {
	    private readonly IGraphService _graphService;

	    public EmailService(IGraphService graphService)
	    {
		    _graphService = graphService;
	    }

	    public async void SendMailAsync(string sendByUserEmail, string[] toRecipientMails,string[] ccRecipientMails, string replyToMail, string subject, string bodyString)
	    {
			var toRecipients = toRecipientMails.Select(email => new Recipient()
		    {
				EmailAddress = new EmailAddress()
				{
					Address = email.Split(';')[0],
					Name = email.Split(';')[1]
				}
			});

		    var ccRecipients = ccRecipientMails.Select(email => new Recipient()
		    {
			    EmailAddress = new EmailAddress()
			    {
				    Address = email.Split(';')[0],
				    Name = email.Split(';')[1]
				}
		    });

			var replyTo = new Recipient()
		    {
			    EmailAddress = new EmailAddress()
			    {
				    Address = replyToMail.Split(';')[0],
				    Name = replyToMail.Split(';')[1]
				}
		    };

		    var body = new ItemBody()
		    {
			    Content = $@"{bodyString}",
			    ContentType = BodyType.Html
		    };


		    var mail = new Message()
		    {
			    Subject = subject,
			    ToRecipients = toRecipients,
			    CcRecipients = ccRecipients,
			    Body = body,
			    From = replyTo,
			    ReplyTo = new[] { replyTo }
		    };


		    await _graphService.GraphClient.Users[sendByUserEmail].SendMail(mail, true)
			    .Request().PostAsync();
		}

	    public string GetMailTemplateByFile(string filePath)
	    {
		    if (System.IO.File.Exists(filePath))
		    {
				return System.IO.File.ReadAllText(filePath);
		    }

			throw new FileNotFoundException("The file "+ filePath + " could not be found.");
		}

	    public string GetUserPasswordMail(string email, string displayName, string password)
	    {
		    var pathToTemplates = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates\\WK_EmailTemplate.html");

			var templateString = GetMailTemplateByFile(pathToTemplates);

		    templateString = templateString
			    .Replace("#GGEmail#", email)
			    .Replace("#GGPassword#", password)
			    .Replace("#GGDisplayName#", displayName);

			return templateString;
	    }

	    public bool SendPasswordMailAsync(WkUser user)
	    {
			var subject = "Der Werteakademie Account ist eingerichtet";

		    var displayName = user.DisplayName;
			var emailString = user.UserPrincipalName;
		    var toMail = new[] { emailString + ";"+ displayName };
		    var ccMail = new[] { "sebsatian.schuetze@***REMOVED***;Sebastian Schuetze" };

			var mailBody = GetUserPasswordMail(emailString, displayName, user.PasswordProfile.Password);

		    SendMailAsync(ccMail[0].Split(';')[0], toMail, ccMail, ccMail[0], subject, mailBody);

			return true;
	    }

    }
}
