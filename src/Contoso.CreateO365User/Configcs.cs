using System;

namespace Contoso.CreateO365User
{
    class Configs
    {
	    public static string UserAdministrationGraphSiteId => GetConfig("UserAdministrationGraphSiteId");

	    public static string UserAdminstrationSharePointListId => GetConfig("UserAdminstrationSharePointListId");

	    public static string SendPasswordQueueName => GetConfig("SendPasswordQueueName");

	    public static string AddToGroupUsersQueueName => GetConfig("AddToGroupUsersQueueName");

	    public static string QueueConnectionString => GetConfig("QueueConnectionString");

	    public static string DefaultO365UserLicense => GetConfig("DefaultO365UserLicense");

	    public static string DefaultExchangeGroupId => GetConfig("DefaultExchangeGroupId");

	    public static string UserEmailPasswordCopy => GetConfig("UserEmailPasswordCopy");

	    public static string UserEmailSender => GetConfig("UserEmailSender");
	    public static string DefaultExchangeOnlineLicense => GetConfig("DefaultExchangeOnlineLicense");

	    private static string GetConfig(string configName)
	    {
			return System.Environment.GetEnvironmentVariable(configName,
				EnvironmentVariableTarget.Process);
		}

	}
}
