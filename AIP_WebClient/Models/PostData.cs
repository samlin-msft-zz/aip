using Microsoft.InformationProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIP_WebClient.Models
{
    public class PostData
    {
        public string blobUrl { get; set; }
        //public string targetContainer { get; set; }
        //public bool isOverwrite { get; set; }
        //public string relativeFilePath { get; set; }
        //public string fileName { get; set; }
        public string labelId { get; set; }
        public string justificationMessage { get; set; }
        //public bool isCustom { get; set; }
        //public List<UserRights> userRightsList { get; set; }
    }
}