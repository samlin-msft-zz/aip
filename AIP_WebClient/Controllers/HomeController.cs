using AIP_WebClient.Utils;
using Microsoft.Identity.Client;
using Microsoft.InformationProtection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AIP_WebClient.Models;
using System.IO;
using Microsoft.InformationProtection.Protection;
using System.Net;
using System.Web.Http;
using Azure.Storage.Blobs;
using Newtonsoft.Json;
using System.Text;

namespace AIP_WebClient.Controllers
{
    public class HomeController : Controller
    {
        public string baseUri = ConfigurationManager.AppSettings["BaseEndpointUri"];
        public string getValuesEndpoint = ConfigurationManager.AppSettings["GetValuesEndpoint"];
        public string getLabelsEndpoint = ConfigurationManager.AppSettings["GetLabelsEndpoint"];
        public string setLabelEndpoint = ConfigurationManager.AppSettings["SetLabelEndpoint"];
        public string IsProtectedEndpoint = ConfigurationManager.AppSettings["IsProtectedEndpoint"];
        public string removeLabelEndpoint = ConfigurationManager.AppSettings["RemoveLabelEndpoint"];
        //public string labelId = ConfigurationManager.AppSettings["LabelId"];
        public string customLabelId = ConfigurationManager.AppSettings["CustomLabelId"];
        public string apiScope = ConfigurationManager.AppSettings["APIScope"];
        public string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

        #region Test
        public async Task<string> Test()
        {
            AuthenticationResult result = null;
            string responseContent = string.Empty;
            string[] scopes = { apiScope };

            IConfidentialClientApplication app = await MsalAppBuilder.BuildConfidentialClientApplication();
            var account = await app.GetAccountAsync(ClaimsPrincipal.Current.GetAccountId());

            try
            {
                result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync().ConfigureAwait(false);
            }
            catch (MsalUiRequiredException ex)
            {
                ViewBag.Relogin = "true";
                return ex.Message;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error has occurred. Details: " + ex.Message;
                return ex.Message;
            }

            if (result != null)
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(baseUri + getValuesEndpoint);
                responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
                ViewBag.Message = responseContent;
            }
            return responseContent;
        }
        #endregion

        [System.Web.Http.Authorize]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
 
        #region Get Labels
        public async Task<string> GetLabels()
        {
            AuthenticationResult result = null;
            string responseContent = string.Empty;
            string[] scopes = { apiScope };

            IConfidentialClientApplication app = await MsalAppBuilder.BuildConfidentialClientApplication();
            var account = await app.GetAccountAsync(ClaimsPrincipal.Current.GetAccountId());

            try
            {
                result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync().ConfigureAwait(false);
            }
            catch (MsalUiRequiredException ex)
            {
                ViewBag.Relogin = "true";
                return ex.Message;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error has occurred. Details: " + ex.Message;
                return ex.Message;
            }

            if (result != null)
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(baseUri + getLabelsEndpoint);
                responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
                ViewBag.Message = responseContent;
            }
            return responseContent;
        }
        #endregion

        #region Set Label
        public async Task<ActionResult> SetLabel(FormCollection fc)
        {
            string blobUrl = fc["blob"].ToString();
            string labelId = fc["labelId"].ToString();
            string fileName = blobUrl.Split('/').Last();

            //UserRights userRights = null;
            //bool isUserDefine = false;
            //string relativeFilePath = "/l1/l2/sample_en.docx";
            //string fileName = "sample_en.docx";
            //string justificationMessage = "Test";

            AuthenticationResult result = null;
            string responseContent = string.Empty;
            string[] scopes = { apiScope };

            IConfidentialClientApplication app = await MsalAppBuilder.BuildConfidentialClientApplication();
            var account = await app.GetAccountAsync(ClaimsPrincipal.Current.GetAccountId());

            try
            {
                result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync().ConfigureAwait(false);

                if (result != null)
                {
                    // Add them to a new List<UserRights>
                    //List<UserRights> allUserRights = new List<UserRights>()
                    //{
                    //    userRights
                    //};

                    PostData postData = new PostData()
                    {
                        blobUrl = blobUrl,
                        //justificationMessage = "Set label justification",
                        labelId = labelId,
                        //userRightsList = allUserRights
                    };

                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
                    HttpResponseMessage httpResponseMessage = await client.PostAsJsonAsync(baseUri + setLabelEndpoint, postData);
                    string responsedata = await httpResponseMessage.Content.ReadAsStringAsync();
                    return Content(responsedata);
                }
                else { return Content($"failed to get token for {account.Username}"); }
            }
            catch (Exception ex)
            {
                return Content($"failed to set label, {ex.Message}");
            }
        }
        #endregion

        #region Remove Label
        public async Task<ActionResult> RemoveLabel(FormCollection fc)
        {
            string blobUrl = fc["blob"].ToString();

            // Retrive blob from url
            //BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            //string containerName = "source";
            //BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            //BlobClient blob = containerClient.GetBlobClient(blobUrl);

            string fileName = blobUrl.Split('/').Last(); //"sample_en_labeled.docx";

            //UserRights userRights = null;
            //bool isUserDefine = false;
            //string relativeFilePath = "/l1/l2/sample_en_labeled.docx";
            //string fileName = "sample_en_labeled.docx";
            //string justificationMessage = "Remove label.";

            AuthenticationResult result = null;
            string responseContent = string.Empty;
            string[] scopes = { apiScope };

            IConfidentialClientApplication app = await MsalAppBuilder.BuildConfidentialClientApplication();
            var account = await app.GetAccountAsync(ClaimsPrincipal.Current.GetAccountId());

            try
            {
                result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync().ConfigureAwait(false);

                if (result != null)
                {
                    // Add them to a new List<UserRights>
                    //List<UserRights> allUserRights = new List<UserRights>()
                    //{
                    //    userRights
                    //};

                    PostData postData = new PostData()
                    {
                        blobUrl = blobUrl,
                        //relativeFilePath = relativeFilePath,
                        //fileName = fileName,
                        justificationMessage = "Remove label justification"
                        //isCustom = isUserDefine,
                        //labelId = this.labelId,
                        //userRightsList = allUserRights
                    };

                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
                    HttpResponseMessage httpResponseMessage = await client.PostAsJsonAsync(baseUri + removeLabelEndpoint, postData);
                    //if (httpResponseMessage.IsSuccessStatusCode)
                    //{
                        string responsedata =  await httpResponseMessage.Content.ReadAsStringAsync();
                        return Content(responsedata);
                    //}
                    //else
                    //{
                    //    string responsedata = await httpResponseMessage.Content.ReadAsStringAsync();
                    //    return Content(responsedata);
                    //}
                }
                else { return Content($"failed to get token for {account.Username}"); }
            }
            catch (Exception ex)
            {
                return Content($"failed to remove label, {ex.Message}");
            }
        }
        #endregion

        #region IsProtected
        public async Task<ActionResult> IsProtected(FormCollection fc)
        {
            string blobUrl = fc["blob"].ToString();

            // Retrive blob from url
            //BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            //string containerName = "source";
            //BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            //BlobClient blob = containerClient.GetBlobClient(blobUrl);

            string fileName = blobUrl.Split('/').Last(); //"sample_en_labeled.docx";

            //UserRights userRights = null;
            //bool isUserDefine = false;
            //string relativeFilePath = "/l1/l2/sample_en_labeled.docx";
            //string fileName = "sample_en_labeled.docx";
            //string justificationMessage = "Remove label.";

            //UserRights userRights = null;
            //bool isUserDefine = false;
            //string relativeFilePath = "/l1/l2/sample_en_labeled.docx";
            //string fileName = "sample_en_labeled.docx";
            //string justificationMessage = "Remove label.";

            AuthenticationResult result = null;
            string responseContent = string.Empty;
            string[] scopes = { apiScope };

            IConfidentialClientApplication app = await MsalAppBuilder.BuildConfidentialClientApplication();
            var account = await app.GetAccountAsync(ClaimsPrincipal.Current.GetAccountId());

            try
            {
                result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync().ConfigureAwait(false);

                if (result != null)
                {
                    //// Add them to a new List<UserRights>
                    //List<UserRights> allUserRights = new List<UserRights>()
                    //{
                    //    userRights
                    //};

                    PostData postData = new PostData()
                    {
                        blobUrl = blobUrl
                        //relativeFilePath = relativeFilePath,
                        //fileName = fileName,
                        //justificationMessage = "Remove label justification",
                        //isCustom = isUserDefine,
                        //labelId = this.labelId,
                        //userRightsList = allUserRights
                    };

                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
                    HttpResponseMessage httpResponseMessage = await client.PostAsJsonAsync(baseUri + IsProtectedEndpoint, postData);
                    string responsedata = await httpResponseMessage.Content.ReadAsStringAsync();
                    return Content(responsedata);
                }
                else { return Content($"failed to get token for {account.Username}"); }
            }
            catch (Exception ex)
            {
                return Content($"failed to check if the file is protected, {ex.Message}");
            }
        }
        #endregion

        /*
        #region Set Custom Permission
        public async Task<ActionResult> SetCustomPermission()
        {
            UserRights userRights;
            bool isUserDefine = true;
            string relativeFilePath = "/l1/l2/sample.docx";
            string fileName = "sample.docx";
            string justificationMessage = string.Empty;

            AuthenticationResult result = null;
            string responseContent = string.Empty;
            string[] scopes = { apiScope };

            IConfidentialClientApplication app = await MsalAppBuilder.BuildConfidentialClientApplication();
            var account = await app.GetAccountAsync(ClaimsPrincipal.Current.GetAccountId());

            try
            {
                result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync().ConfigureAwait(false);

                if (result != null)
                {
                    // Create a List<string> of the first set of permissions. 
                    List<string> users = new List<string>()
                    {
                        "w1@samlin.onmicrosoft.com",
                        "w2@samlin.onmicrosoft.com"
                    };

                    // Create a List<string> of the Rights the above users should have. 
                    List<string> rights = new List<string>()
                    {
                        Rights.View,
                        Rights.Edit
                    };

                    // Create a UserRights object containing the defined users and rights.
                    userRights = new UserRights(users, rights);

                    // Add them to a new List<UserRights>
                    List<UserRights> allUserRights = new List<UserRights>()
                    {
                        userRights
                    };

                    PostData postData = new PostData()
                    {
                        relativeFilePath = relativeFilePath,
                        fileName = fileName,
                        justificationMessage = justificationMessage,
                        isCustom = isUserDefine,
                        labelId = this.customLabelId,
                        userRightsList = allUserRights
                    };

                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
                    HttpResponseMessage httpResponseMessage = await client.PostAsJsonAsync(baseUri + setLabelEndpoint, postData);
                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync();
                        stream.Position = 0;
                        return File(stream, "application/octet-stream", fileName);
                    }
                    else
                    {
                        return Content($"failed to download file: {fileName}");
                    }
                }
                else { return Content($"failed to get token for {account.Username}"); }
            }
            catch (Exception ex)
            {
                return Content($"failed to set label, {ex.Message}");
            }
        }
        #endregion
        */

    }
}
