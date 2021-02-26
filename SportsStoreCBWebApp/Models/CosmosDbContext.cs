using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SportsStoreCBWebApp.Models
{
  public class CosmosDbContext
  {
    private readonly ILogger<CosmosDbContext> _logger;
    private readonly string _databaseId;
    private readonly string _containerId;
    private readonly CosmosClient _cosmosClient;
    private readonly string _cosmosEndpoint;
    private readonly string _cosmosKey;
    public CosmosDbContext(IOptions<CosmosUtility> cosmosUtility, IConfiguration configuration, ILogger<CosmosDbContext> logger)
    {
      _cosmosEndpoint = cosmosUtility.Value.CosmosEndpoint;
      _cosmosKey = cosmosUtility.Value.CosmosKey;
      _logger = logger;
      _databaseId = configuration["CosmosConnectionString:DatabaseName"]; // "sportsStoreDb";
      _containerId = configuration["CosmosConnectionString:ContainerName"];  // "products";
      _cosmosClient = new CosmosClient(_cosmosEndpoint, _cosmosKey);
    }

    public async Task<bool> CreateDatabaseAsync()
    {
      //DatabaseResponse dbResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId, throughput: 400);
      DatabaseResponse dbResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId);
      if (dbResponse.StatusCode == System.Net.HttpStatusCode.Created)
      {
        _logger.LogInformation($"Database: '{_databaseId}' created successfully");
        return true;
      }
      return false;
    }

    public async Task<bool> CreateContainerAsync(string partitionKeyName)
    {
      ContainerProperties containerProperties = new ContainerProperties(_containerId, $"/{partitionKeyName}");
      ContainerResponse containerResponse = await _cosmosClient.GetDatabase(_databaseId).CreateContainerIfNotExistsAsync(containerProperties);
      if (containerResponse.StatusCode == System.Net.HttpStatusCode.Created)
      {
        _logger.LogInformation($"Container: '{_containerId}' with the PartitionKey: '/{partitionKeyName}' has been created successfully in the Database: '{_databaseId}'");
        return true;
      }
      return false;
    }
  }
}
