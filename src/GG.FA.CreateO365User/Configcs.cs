using System;
using System.Collections.Generic;
using System.Text;

namespace GG.FA.CreateO365User
{
    class Configs
    {
	    public static string UserAdministrationGraphSiteId
	    {
		    get
		    {
				return GetConfig("UserAdministrationGraphSiteId");
		    }
	    }

	    public static string UserAdminstrationSharePointListId
		{
		    get
		    {
				return GetConfig("UserAdminstrationSharePointListId");
		    }
	    }

	    public static string SendPasswordQueueName
		{
		    get
		    {
			    return GetConfig("SendPasswordQueueName");
		    }
	    }

	    public static string AddToGroupUsersQueueName
		{
		    get
		    {
			    return GetConfig("AddToGroupUsersQueueName");
		    }
	    }

	    public static string O365AdminUser
		{
		    get
		    {
			    return GetConfig("O365AdminUser");
		    }
	    }

	    public static string O365AdminPassword
		{
		    get
		    {
			    return GetConfig("O365AdminPassword");
		    }
	    }

	    public static string QueueConnectionString
		{
		    get
		    {
			    return GetConfig("QueueConnectionString");
		    }
	    }

	    public static string DefaultO365UserLicense
		{
		    get
		    {
			    return GetConfig("DefaultO365UserLicense");
		    }
	    }

		private static string GetConfig(string configName)
	    {
			return System.Environment.GetEnvironmentVariable(configName,
				EnvironmentVariableTarget.Process);
		}

	}
}
