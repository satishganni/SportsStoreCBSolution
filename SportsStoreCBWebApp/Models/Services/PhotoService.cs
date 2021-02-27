using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SportsStoreCBWebApp.Models.Abstract;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace SportsStoreCBWebApp.Models.Services
{
  public class PhotoService : IPhotoService
  {
    private CloudStorageAccount _storageAccount;
    private readonly ILogger<PhotoService> _logger;
    private readonly CloudBlobClient _blobClient;
    public PhotoService(IOptions<StorageUtility> storageUtility, ILogger<PhotoService> logger)
    {
      _storageAccount = storageUtility.Value.StorageAccount;
      _logger = logger;
      _blobClient = _storageAccount.CreateCloudBlobClient();
    }

    public async Task<string> UploadPhotoAsync(string category, IFormFile photoToUpload)
    {
      if (photoToUpload == null || photoToUpload.Length == 0) return null;

      string cateogryLowerCase = category.ToLower().Trim();
      string fullPath = null;
      try
      {
        CloudBlobContainer blobContainer = _blobClient.GetContainerReference(cateogryLowerCase);

        if (await blobContainer.CreateIfNotExistsAsync())
        {
          await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
          _logger.LogInformation($"Successfully created Blob Storage Container '{blobContainer.Name}' and made it Public");
        }

        string imageName = $"productphoto{Guid.NewGuid().ToString()}{Path.GetExtension(photoToUpload.FileName.Substring(photoToUpload.FileName.LastIndexOf("/") + 1))}";

        // Upload image to blob storage
        CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(imageName);
        blockBlob.Properties.ContentType = photoToUpload.ContentType;
        await blockBlob.UploadFromStreamAsync(photoToUpload.OpenReadStream());

        fullPath = blockBlob.Uri.ToString();
        _logger.LogInformation($"Blob Service, PhotoService.UploadPhoto, imagePath='{fullPath}'");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error Uploading the photo blob to storage");
      }
      return fullPath;
    }

    private async Task<bool> DeleteEmptyContainerAsync(CloudBlobContainer blobContainer)
    {
      BlobContinuationToken ctoken = null;
      var result = await blobContainer.ListBlobsSegmentedAsync(ctoken);
      if (result.Results.Count() == 0)
      {
        _logger.LogInformation($"Blob Container '{blobContainer.Name}' is empty, it will be deleted");
        return await blobContainer.DeleteIfExistsAsync();
      }
      return false;
    }
    public async Task<bool> DeletePhotoAsync(string category, string photoUrl)
    {
      if (string.IsNullOrEmpty(photoUrl)) return true;
      
      string categoryLowerCase = category.ToLower().Trim();
      bool deletedFlag = false;
      try
      {
        CloudBlobContainer blobContainer = _blobClient.GetContainerReference(categoryLowerCase);

        if (blobContainer.Name == categoryLowerCase)
        {
          string blobName = photoUrl.Substring(photoUrl.LastIndexOf("/") + 1);
          CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobName);
          deletedFlag = await blockBlob.DeleteIfExistsAsync();
        }
        _logger.LogInformation($"Blob Service, PhotoService.DeletePhoto, deletedImagePath='{photoUrl}'");
        await DeleteEmptyContainerAsync(blobContainer);
        return deletedFlag;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error in deleting the photo blob from storage");
        throw;
      }
    }

    public async Task<string> CopyPhotoAsync(string oldCategory, string newCategory, string photoUrl)
    {
      if (string.IsNullOrEmpty(photoUrl)) return null;

      string blobName = photoUrl.Substring(photoUrl.LastIndexOf("/") + 1);
      string oldCategoryLowerCase = oldCategory.ToLower().Trim();
      string newCategoryLowerCase = newCategory.ToLower().Trim();
      CloudBlockBlob oldCloudBlockBlob = null;
      CloudBlockBlob newCloudBlockBlob = null;
      string newFullPath = null;
      bool deletedFlag;
      MemoryStream mStream = new MemoryStream();
      try
      {
        CloudBlobContainer oldBlobContainer = _blobClient.GetContainerReference(oldCategoryLowerCase);
        if (oldBlobContainer.Name == oldCategoryLowerCase)
        {
          oldCloudBlockBlob = oldBlobContainer.GetBlockBlobReference(blobName);
          await oldCloudBlockBlob.DownloadToStreamAsync(mStream);
        }

        CloudBlobContainer newBlobContainer = _blobClient.GetContainerReference(newCategoryLowerCase);
        if (await newBlobContainer.CreateIfNotExistsAsync())
        {
          await newBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
          _logger.LogInformation($"Successfully created Blob Storage Container '{newBlobContainer.Name}' and made it Public");
        }
        if (mStream.Length != 0)
        {
          newCloudBlockBlob = newBlobContainer.GetBlockBlobReference(blobName);
          newCloudBlockBlob.Properties.ContentType = oldCloudBlockBlob.Properties.ContentType;
          mStream.Position = 0;
          await newCloudBlockBlob.UploadFromStreamAsync(mStream);
          await mStream.FlushAsync();
          newFullPath = newCloudBlockBlob.Uri.ToString();
          _logger.LogInformation($"Blob Service, PhotoService.CopyPhotoAsync, imagePath='{newFullPath}'");
          deletedFlag = await oldCloudBlockBlob.DeleteIfExistsAsync();
          await DeleteEmptyContainerAsync(oldBlobContainer);
          _logger.LogInformation($"blob Service, PhotoService.DeletePhoto, deletedImagePath='{photoUrl}'");
        }
        return newFullPath;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error in copying the photo from one container to another container");
        throw;
      }
    }
  }
}
