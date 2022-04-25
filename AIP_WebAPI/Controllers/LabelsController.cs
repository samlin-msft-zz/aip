using AIP_WebAPI.Common;
using AIP_WebAPI.Models;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.InformationProtection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace AIP_WebAPI.Controllers
{
    public class LabelsController : ApiController
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientID"];
        private static string appName = ConfigurationManager.AppSettings["ApplicationName"];
        private static string appVersion = ConfigurationManager.AppSettings["ApplicationVersion"];
		private static string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
		private static string UseMI = ConfigurationManager.AppSettings["UseManagedIdentity"];

		private static FileApi fileApi = new FileApi(clientId, appName, appVersion, ClaimsPrincipal.Current);

        [Authorize]
		[HttpGet]
		public JsonResult<List<Models.Label>> GetLabels()
        {
            List<Models.Label> result = fileApi.ListAllLabels();
            return Json(result);
        }

        [Authorize]
        [HttpPost]
		public async Task<JsonResult<ResponseData>> SetLabel([FromBody] PostData data)
		{
			string[] blobstring = data.blobUrl.Split('/');
			string storageAccount = blobstring[2];
			string fileName = blobstring.Last();
			string containerName = blobstring[3];

			ResponseData responseData = new ResponseData();

			try
			{
				bool.TryParse(UseMI, out bool isUseMI);

				BlobServiceClient blobServiceClient = null;
				BlobContainerClient containerClient = null;
				BlobClient blob = null;

				if (isUseMI)
				{
					var cred = new ChainedTokenCredential(new ManagedIdentityCredential(), new AzureCliCredential());
					blobServiceClient = new BlobServiceClient(new Uri($"{storageAccount}"), cred);
				}
                else
				{
					blobServiceClient = new BlobServiceClient(connectionString);					
				}
				containerClient = blobServiceClient.GetBlobContainerClient(containerName);
				blob = containerClient.GetBlobClient(fileName);

				MemoryStream ms = new MemoryStream();
				var result = await blob.DownloadToAsync(ms);

				MemoryStream outputStream = new MemoryStream();

				bool isSuccess = fileApi.ApplyLabel(ms, outputStream, fileName, data.labelId, out string message);
				ms.Dispose();

				responseData.IsSuccess = isSuccess;
				responseData.Message = message;

				if (isSuccess)
				{
					BlobContainerClient targetContainerClient = blobServiceClient.GetBlobContainerClient("target");
					await targetContainerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.None);
					BlobClient targetBlob = targetContainerClient.GetBlobClient(fileName);
					outputStream.Position = 0;
					await targetBlob.UploadAsync(outputStream, true);					
					responseData.TargetUrl = HttpUtility.UrlDecode(targetBlob.Uri.ToString());
					outputStream.Dispose();
				}
			}
			catch (Exception ex)
			{
				responseData.Message = ex.Message;
			}
			return Json(responseData);
		}

		[Authorize]
		[HttpPost]
		public async Task<JsonResult<ResponseData>> RemoveLabel([FromBody] PostData data)
		{
			string[] blobstring = data.blobUrl.Split('/');
			string storageAccount = blobstring[2];
			string fileName = blobstring.Last();
			string containerName = blobstring[3];

			ResponseData responseData = new ResponseData();

			try
			{
				bool.TryParse(UseMI, out bool isUseMI);

				BlobServiceClient blobServiceClient = null;
				BlobContainerClient containerClient = null;
				BlobClient blob = null;

				if (isUseMI)
				{
					var cred = new ChainedTokenCredential(new ManagedIdentityCredential(), new AzureCliCredential());
					blobServiceClient = new BlobServiceClient(new Uri($"{storageAccount}"), cred);
				}
				else
				{
					blobServiceClient = new BlobServiceClient(connectionString);
				}
				containerClient = blobServiceClient.GetBlobContainerClient(containerName);
				blob = containerClient.GetBlobClient(fileName);

				MemoryStream ms = new MemoryStream();
				await blob.DownloadToAsync(ms);

				MemoryStream outputStream = new MemoryStream();

				bool isSuccess = fileApi.RemoveLabel(ms, outputStream, fileName, data.justificationMessage, out string message);

				responseData.IsSuccess = isSuccess;
				responseData.Message = message;

				if (isSuccess)
				{
					BlobContainerClient targetContainerClient = blobServiceClient.GetBlobContainerClient("target");
					await targetContainerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.None);
					BlobClient targetBlob = targetContainerClient.GetBlobClient(fileName);
					outputStream.Position = 0;
					await targetBlob.UploadAsync(outputStream, true);
					responseData.TargetUrl = HttpUtility.UrlDecode(targetBlob.Uri.ToString());
				}
			}
			catch (Exception ex)
			{
				responseData.Message = ex.Message;
			}
			return Json(responseData);
		}

		[Authorize]
		[HttpPost]
		public async Task<JsonResult<ResponseData>> GetFileLabel([FromBody] PostData data)
		{
			string[] blobstring = data.blobUrl.Split('/');
			string storageAccount = blobstring[2];
			string fileName = blobstring.Last();
			string containerName = blobstring[3];

			ResponseData responseData = new ResponseData();

			try
			{
				bool.TryParse(UseMI, out bool isUseMI);

				BlobServiceClient blobServiceClient = null;
				BlobContainerClient containerClient = null;
				BlobClient blob = null;

				if (isUseMI)
				{
					var cred = new ChainedTokenCredential(new ManagedIdentityCredential(), new AzureCliCredential());
					blobServiceClient = new BlobServiceClient(new Uri($"{storageAccount}"), cred);
				}
				else
				{
					blobServiceClient = new BlobServiceClient(connectionString);
				}
				containerClient = blobServiceClient.GetBlobContainerClient(containerName);
				blob = containerClient.GetBlobClient(fileName);

				MemoryStream ms = new MemoryStream();
				var result = await blob.DownloadToAsync(ms);

				MemoryStream outputStream = new MemoryStream();

				ContentLabel contentLabel = fileApi.GetFileLabel(ms, outputStream, fileName, out string message);
				responseData.IsSuccess = true;
				if(contentLabel != null)
                {
					responseData.LabelId = contentLabel.Label.Id;
					responseData.LabelName = contentLabel.Label.Name;
					responseData.IsProtected = contentLabel.IsProtectionAppliedFromLabel;
				}
			}
			catch (Exception ex)
			{
				responseData.Message = ex.Message;
				responseData.IsSuccess = false;
			}
			return Json(responseData);
		}
	}
}