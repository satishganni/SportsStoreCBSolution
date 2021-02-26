using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SportsStoreCBWebApp.Models.Entities
{
  public class Product
  {
    [JsonProperty("id")]
    public string ProductId { get; set; }
    [JsonProperty("productName")]
    [Required, StringLength(100)]
    public string ProductName { get; set; }
    [JsonProperty("price")]
    [Required]
    public decimal Price { get; set; }
    [JsonProperty("category")]
    [Required, StringLength(100)]
    public string Category { get; set; }
    [JsonProperty("description")]
    [Required, StringLength(250)]
    public string Description { get; set; }
    [JsonProperty("photoUrl")]
    public string PhotoUrl { get; set; }
  }
}
