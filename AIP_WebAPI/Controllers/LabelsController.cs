using AIP_WebAPI.Common;
using AIP_WebAPI.Models;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.InformationProtection;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Results;

namespace AIP_WebAPI.Controllers
{
    public class LabelsController : ApiController
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientID"];
        private static string appName = ConfigurationManager.AppSettings["ApplicationName"];
        private static string appVersion = ConfigurationManager.AppSettings["ApplicationVersion"];
		private static string shareName = ConfigurationManager.AppSettings["ShareName"];
		private static string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
		private FileApi fileApi;

        // GET api/<controller>
        [Authorize]
        public JsonResult<List<Models.Label>> Get()
        {
            fileApi = new FileApi(clientId, appName, appVersion, ClaimsPrincipal.Current);
            List<Models.Label> result = fileApi.ListAllLabels();
            return Json(result);
        }

		// POST api/values
		[Authorize]
		[HttpPost]
		public HttpResponseMessage Post([FromBody] PostData data)
		{

			fileApi = new FileApi(clientId, appName, appVersion, ClaimsPrincipal.Current);

			string dirName = data.relativeFilePath.ToLower().Replace(data.fileName.ToLower(), "");
			// Get a reference to the file
			ShareClient share = new ShareClient(connectionString, shareName);
			ShareDirectoryClient directory = share.GetDirectoryClient(dirName);
			ShareFileClient file = directory.GetFileClient(data.fileName);

			// Download the file
			ShareFileDownloadInfo download = file.Download();
			MemoryStream ms = new MemoryStream();
			download.Content.CopyTo(ms);

			MemoryStream outputStream = new MemoryStream();

			ProtectionDescriptor protectionDescriptor = new ProtectionDescriptor(data.userRightsList);

			string errMessage = string.Empty;
			bool result = fileApi.ApplyLabel(ms, outputStream, data.fileName, data.labelId, data.justificationMessage, data.isCustom, protectionDescriptor, out errMessage);
			if (result)
			{
				HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
				outputStream.Position = 0;
				httpResponseMessage.Content = new StreamContent(outputStream);
				httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
				httpResponseMessage.Content.Headers.ContentDisposition.FileName = data.fileName;
				httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

				return httpResponseMessage;
			}
			else
			{
				HttpError err = new HttpError(errMessage);
				return Request.CreateResponse(HttpStatusCode.BadRequest, err);
			}
		}
	}
}