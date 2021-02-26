using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SportsStoreCBWebApp.Models.Abstract;
using SportsStoreCBWebApp.Models.Entities;

namespace SportsStoreCBWebApp.Models.Concrete
{
  public class CDbProductRepository : IProductRepository
  {
    private readonly ILogger<CDbProductRepository> _logger;
    private readonly string _cosmosEndpoint;
    private readonly string _cosmosKey;
    private readonly string _databaseId;
    private readonly string _containerId;
    private readonly CosmosClient _cosmosClient;
    private readonly Database _database;
    private readonly Container _container;
    private readonly IDistributedCache _distributedCache;
    private readonly IConfiguration _configuration;

    public CDbProductRepository(IOptions<CosmosUtility> cosmosUtility, IConfiguration configuration, ILogger<CDbProductRepository> logger, IDistributedCache distributedCache)
    {
      _logger = logger;
      _cosmosEndpoint = cosmosUtility.Value.CosmosEndpoint;
      _cosmosKey = cosmosUtility.Value.CosmosKey;
      _databaseId = configuration["CosmosConnectionString:DatabaseName"]; // "sportsStoreDb";
      _containerId = configuration["CosmosConnectionString:ContainerName"];  // "products";
      _cosmosClient = new CosmosClient(_cosmosEndpoint, _cosmosKey);
      _database = _cosmosClient.GetDatabase(_databaseId);
      _container = _database.GetContainer(_containerId);
      _distributedCache = distributedCache;
      _configuration = configuration;
    }
    public void ClearCache()
    {
      _distributedCache.RemoveAsync("productsList");
      _logger.LogInformation($"CDbProductRepository.ClearCache, 'productsList' Cache deleted");
    }

    public async Task<Product> CreateAsync(Product product)
    {
      return null;
    }

    public async Task<bool> DeleteAsync(string productId, string category)
    {
      return false;
    }

    private async Task<IEnumerable<Product>> QueryProducts(string queryText)
    {
      return null;
    }

    public async Task<Product> FindProductByIDAsync(string productId)
    {
      var queryText = $"SELECT * FROM p where p.id='{productId}'";
      var products = await QueryProducts(queryText);
      return products.First();
    }

    public async Task<List<Product>> FindProductsByCategoryAsync(string category)
    {
      var queryText = $"SELECT * FROM p where p.category='{category}'";
      var products = await QueryProducts(queryText);
      return products.ToList();
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {

      var queryText = $"SELECT * FROM p";
      var products = await QueryProducts(queryText);
      return products.ToList();
    }

    public async Task<Product> UpdateAsync(Product product)
    {
      var queryText = $"SELECT * FROM p where p.id='{product.ProductId}'";
      var products = await QueryProducts(queryText);
      var oldProduct = products.First();
      var deleteResult = await DeleteAsync(oldProduct.ProductId, oldProduct.Category);
      if(deleteResult)
      {
        _logger.LogInformation($"***Product updated the product with the ProductId: {oldProduct.ProductId}, OldCategory: {oldProduct.Category} and UpdatedCategory: {product.Category}***");
        return await CreateAsync(product);
      }
      _logger.LogInformation($"***Could not update the product with the ProductId: {oldProduct.ProductId} in the Category: {oldProduct.Category}***");
      return oldProduct;
    }
  }
}
