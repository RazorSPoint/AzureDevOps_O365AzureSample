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
	    private readonly string _connectionString;
	    private readonly string _queueName;
	    private readonly CloudStorageAccount _storageAccount;

	    public QueueService(string connectionString, string queueName)
	    {
		    _queueName = queueName;
		    _connectionString = connectionString;
		    _storageAccount = CloudStorageAccount.Parse(_connectionString);
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
