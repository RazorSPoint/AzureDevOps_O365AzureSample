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
			    // site: ***REMOVED***/sites/gut-goedelitz/UserAdminiatration
				return GetConfig("UserAdministrationGraphSiteId");
		    }
	    }

	    public static string UserAdminstrationSharePointListId
		{
		    get
		    {
			    // list: ***REMOVED***/sites/gut-goedelitz/UserAdminiatration/Lists/UserInventory
				return GetConfig("UserAdminstrationSharePointListId");
		    }
	    }

	    public static string SendPasswordQueueConnectionString
		{
		    get
		    {
			    return GetConfig("SendPasswordQueueConnectionString");
		    }
	    }

	    public static string AddUserToGroupQueueConnectionString
		{
		    get
		    {
			    return GetConfig("AddUserToGroupQueueConnectionString");
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

		private static string GetConfig(string configName)
	    {
			return System.Environment.GetEnvironmentVariable(configName,
				EnvironmentVariableTarget.Process);
		}

	}
}
