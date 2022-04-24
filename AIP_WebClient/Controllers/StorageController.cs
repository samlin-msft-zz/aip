using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AIP_WebClient.Controllers
{
    public class StorageController : Controller
    {
        public string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

        // GET: Storage
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Upload()
        {
            var file = Request.Files["file"];

            //string path = @"D:\Temp\";
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            ////Create a unique name for the container
            string containerName = "source";

            //// Create the container and return a container client object
            //BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);

            // Create a local file in the ./data/ directory for uploading and downloading
            //string localPath = "./data/";
            //string fileName = "quickstart" + Guid.NewGuid().ToString() + ".txt";
            //string localFilePath = Path.Combine(localPath, fileName);

            //// Write text to the file
            //await File.WriteAllTextAsync(localFilePath, "Hello, World!");

            // Get a reference to a blob
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(file.FileName);
            string blobUri = HttpUtility.UrlDecode(blobClient.Uri.ToString());

            //Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            // Upload data from the local file
            var result = await blobClient.UploadAsync(file.InputStream, true);

            if (result == null)
            {
                ViewBag.UploadMessage = "Failed to upload file.";
            }
            else
            {                
                ViewBag.UploadMessage = String.Format("Got file {0} uploaded to {1}",
                    file.FileName, blobUri);
                // TODO: actually save the image to Azure blob storage
            }

            return View("index");
        }
    }
}