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
      if (string.IsNullOrEmpty(product.ProductId))
      {
        product.ProductId = Guid.NewGuid().ToString();
      }
      ItemResponse<Product> productResponse = await _container.CreateItemAsync<Product>(product);
      if (productResponse.Resource != null)
      {
        _logger.LogInformation($"Product with the ProductId: {productResponse.Resource.ProductId} of the Category: {productResponse.Resource.Category}, has been created successfully");
        return productResponse.Resource;
      }
      _logger.LogInformation($"***Product with the ProductId: {productResponse.Resource.ProductId} of the Category: {productResponse.Resource.Category}, could not be created***");
      return null;
    }

    public async Task<bool> DeleteAsync(string productId, string category)
    {
      ItemResponse<Product> productResponse = await _container.DeleteItemAsync<Product>(productId, new PartitionKey(category));
      if (productResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
      {
        _logger.LogInformation($"Product with ProductId: {productId} and Category: {category}, has been deleted successfully");
        return true;
      }
      _logger.LogInformation($"***Product with ProductId: {productId} and Category: {category}, could not be deleted***");
      return false;
    }

    private async Task<IEnumerable<Product>> QueryProducts(string queryText)
    {
      FeedIterator<Product> feedIterator = _container.GetItemQueryIterator<Product>(queryText);
      while (feedIterator.HasMoreResults)
      {
        FeedResponse<Product> products = await feedIterator.ReadNextAsync();
        _logger.LogInformation($"Query: {queryText} returned - '{products.Resource.Count()}' number of product/s");
        return products.Resource;
      }
      _logger.LogInformation($"Query: {queryText} returned - null");
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

      #region Without Caching
      //var queryText = $"SELECT * FROM p";
      //var products = await QueryProducts(queryText);
      //return products.ToList();
      #endregion
      // Will enable/use Redis Cache
      try
      {
        List<Product> productsList = null;
        if (_configuration["EnableRedisCaching"] == "true")
        {
          var cachedProductsList = await _distributedCache.GetStringAsync("productsList");
          if (!string.IsNullOrEmpty(cachedProductsList))
          {
            productsList = JsonConvert.DeserializeObject<List<Product>>(cachedProductsList);
            _logger.LogInformation($"CDbProductRepository.GetAllProductsAsync, ProductsList read from Cache");
          }
          else
          {
            var queryText = $"SELECT * FROM p";
            var products = await QueryProducts(queryText);
            productsList = products.ToList();
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions();
            entryOptions.SetAbsoluteExpiration(new TimeSpan(0, 3, 0));
            await _distributedCache.SetStringAsync("productsList", JsonConvert.SerializeObject(productsList), entryOptions);
            _logger.LogInformation($"CDbProductRepository.GetAllProductsAsync, ProductsList is Cached");
          }
        }
        else 
        {
          var queryText = $"SELECT * FROM p";
          var products = await QueryProducts(queryText);
          productsList = products.ToList();
        }
        return productsList;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error in CDbProductRepository.GetAllProductsAsync");
        throw;
      }
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
