using System.Collections.Generic;
using System.Threading.Tasks;

using SportsStoreCBWebApp.Models.Entities;

namespace SportsStoreCBWebApp.Models.Abstract
{
  public interface IProductRepository
  {
    Task<List<Product>> GetAllProductsAsync();

    Task<Product> FindProductByIDAsync(string productId);

    Task<List<Product>> FindProductsByCategoryAsync(string category);

    Task<Product> CreateAsync(Product product);

    Task<Product> UpdateAsync(Product product);

    Task<bool> DeleteAsync(string productId, string category);
    void ClearCache();
  }
}
