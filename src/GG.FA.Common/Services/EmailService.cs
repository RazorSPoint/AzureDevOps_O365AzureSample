﻿using System;
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
	    private readonly string _templatePath;

	    public EmailService(IGraphService graphService,string templatePath)
	    {
		    _graphService = graphService;
		    _templatePath = templatePath;
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
		    var pathToTemplates = Path.Combine(_templatePath, "WK_EmailTemplate.html");

			var templateString = GetMailTemplateByFile(pathToTemplates);

		    templateString = templateString
			    .Replace("#GGEmail#", email)
			    .Replace("#GGPassword#", password)
			    .Replace("#GGDisplayName#", displayName);

			return templateString;
	    }

	    public bool SendPasswordMailAsync(User user,User admin, string password)
	    {
			var subject = "Der Werteakademie Account ist eingerichtet";

		    var displayName = user.DisplayName;
			var emailString = user.UserPrincipalName;
		    var toMail = new[] { emailString + ";"+ displayName };
		    var ccMail = new[] { $"{admin.UserPrincipalName};{admin.DisplayName}" };

			var mailBody = GetUserPasswordMail(emailString, displayName, password);

		    SendMailAsync(admin.UserPrincipalName, toMail, ccMail, ccMail[0], subject, mailBody);

			return true;
	    }

    }
}