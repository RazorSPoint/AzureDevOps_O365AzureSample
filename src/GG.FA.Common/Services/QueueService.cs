using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GG.FA.Common.Utilities;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace GG.FA.Common.Services
{
    public class QueueService
    {
	    private readonly string _connectionStringName;
	    private readonly string _queueName;
	    private readonly CloudStorageAccount _storageAccount;

	    public QueueService(string connectionStringNameName, string queueName)
	    {
		    _queueName = queueName;
		    _connectionStringName = connectionStringNameName;
		    _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(_connectionStringName));
	    }

	    public Task CreateMessageAsync(string message)
	    {
		    var queueClient = _storageAccount.CreateCloudQueueClient();
		    var targetQueue = queueClient.GetQueueReference(_queueName);

		    var queueMessage = new CloudQueueMessage(message);
		    return targetQueue.AddMessageAsync(queueMessage);
	    }

	    public Task CreateEncryptedMessageAsync(string message)
	    {
		    var encryptedMessage = Security.EncryptString(Security.ToSecureString(message));
		    return CreateMessageAsync(encryptedMessage);

	    }
    }
}
