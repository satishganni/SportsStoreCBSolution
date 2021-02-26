using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportsStoreCBWebApp.Models.Abstract;
using SportsStoreCBWebApp.Models.Entities;

namespace SportsStoreCBWebApp.Controllers
{
  public class ProductController : Controller
  {
    private readonly IProductRepository _productRepository;
    private readonly IPhotoService _photoService;

    public ProductController(IProductRepository productRepository, IPhotoService photoService)
    {
      _productRepository = productRepository;
      _photoService = photoService;
    }
    public async Task<IActionResult> List()
    {
      var productsList = await _productRepository.GetAllProductsAsync();
      return View(productsList);
    }

    public IActionResult Create() => View();
    
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(include: "ProductName, Description, Price, Category, PhotoUrl")] Product product, IFormFile photo)
    {
      if (ModelState.IsValid)
      {
        product.PhotoUrl = await _photoService.UploadPhotoAsync(product.Category, photo);
        var newProduct = _productRepository.CreateAsync(product);
        TempData["newproduct"] = $"New Product: '{product.ProductName}' in the Category: '{product.Category}' has been added successfully";
        return RedirectToAction("List");
      }
      return View(product);
    }

    public async Task<ActionResult> GetByCategory(string category)
    {
      ViewBag.category = category;
      var result = await _productRepository.FindProductsByCategoryAsync(category);
      return View(result);
    }

    public async Task<ActionResult> Edit(string productId)
    {
      var result = await _productRepository.FindProductByIDAsync(productId);
      return View(result);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(Product product, IFormFile photo)
    {
      var result = await _productRepository.FindProductByIDAsync(product.ProductId);
      if (result.Category == product.Category)
      {
        if (photo != null)
        {
          if (await _photoService.DeletePhotoAsync(product.Category, product.PhotoUrl))
          {
            product.PhotoUrl = await _photoService.UploadPhotoAsync(product.Category, photo);
          }
        }
      }
      else 
      {
        string newPhotoPath = await _photoService.CopyPhotoAsync(result.Category, product.Category, product.PhotoUrl);
        product.PhotoUrl = newPhotoPath;
      }
      await _productRepository.UpdateAsync(product);
      return RedirectToAction("List");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(string productId)
    {
      var result = await _productRepository.FindProductByIDAsync(productId);
      if (await _photoService.DeletePhotoAsync(result.Category, result.PhotoUrl))
      {
        await _productRepository.DeleteAsync(productId, result.Category);
      }
      return RedirectToAction("List");
    }

    public IActionResult ClearCache()
    {
      _productRepository.ClearCache();
      return RedirectToAction("List");
    }
  }
}
