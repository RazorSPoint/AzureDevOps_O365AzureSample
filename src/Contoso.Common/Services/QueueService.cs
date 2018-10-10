using System.Threading.Tasks;
using Contoso.Common.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Contoso.Common.Services
{
    public class QueueService
    {
	    private readonly string _connectionString;
	    private readonly string _queueName;
	    private readonly CloudStorageAccount _storageAccount;
		private CloudQueueClient _queueClient;
		private CloudQueue _targetQueue;

		public QueueService(string connectionString, string queueName)
	    {
		    _queueName = queueName;
		    _connectionString = connectionString;
		    _storageAccount = CloudStorageAccount.Parse(_connectionString);
			_queueClient = _storageAccount.CreateCloudQueueClient();
			_targetQueue = _queueClient.GetQueueReference(_queueName);			
		}

		public Task CreateQueueIfNotExistsAsync()
		{
			return _targetQueue.CreateIfNotExistsAsync();
		}
			   
	    public async Task CreateMessageAsync(string message)
	    {
		    var queueMessage = new CloudQueueMessage(message);

			await CreateQueueIfNotExistsAsync();
			await _targetQueue.AddMessageAsync(queueMessage);
		}

	    public async Task CreateEncryptedMessageAsync(string message)
	    {
		    var encryptedMessage = Security.EncryptString(Security.ToSecureString(message));

			await CreateQueueIfNotExistsAsync();
			await CreateMessageAsync(encryptedMessage);
	    }
    }
}
