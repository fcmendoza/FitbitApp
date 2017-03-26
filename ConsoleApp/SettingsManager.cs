using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ConsoleApp
{
    public class SettingsManager
    {
        public SettingsEntity Retrieve()
        {
            // Parse the connection string and return a reference to the storage account.
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            var tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            var table = tableClient.GetTableReference("fitbitsettings");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            // Construct the query operation for all setting entities where PartitionKey="fitbitpk".
            // Create a retrieve operation that takes a settings entity.
            var retrieveOperation = TableOperation.Retrieve<SettingsEntity>("fitbitpk", "user1");

            // Execute the retrieve operation.
            var retrievedResult = table.Execute(retrieveOperation);

            var settings = (SettingsEntity)retrievedResult.Result;
            return settings;
        }

        public void UpdateSettings(string accessToken, string refreshToken)
        {
            // Parse the connection string and return a reference to the storage account.
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            var tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            var table = tableClient.GetTableReference("fitbitsettings");

            // Construct the query operation for all setting entities where PartitionKey="fitbitpk".
            // Create a retrieve operation that takes a settings entity.
            var retrieveOperation = TableOperation.Retrieve<SettingsEntity>("fitbitpk", "user1");

            // Execute the retrieve operation.
            var retrievedResult = table.Execute(retrieveOperation);

            //
            var settings = (SettingsEntity)retrievedResult.Result;

            // Update or create settings as necessary.
            if (settings != null)
            {
                // Update tokens
                settings.AccessToken = accessToken;
                settings.RefreshToken = refreshToken;

                // Create the Replace TableOperation.
                var updateOperation = TableOperation.Replace(settings);

                // Execute the operation.
                table.Execute(updateOperation);
            }
        }
    }

    public class SettingsEntity : TableEntity
    {
        public SettingsEntity(string partitionKeyName, string userId)
        {
            this.PartitionKey = partitionKeyName;
            this.RowKey = userId;
        }

        public SettingsEntity() { }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }
}