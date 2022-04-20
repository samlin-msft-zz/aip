using Microsoft.InformationProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIP_WebAPI.Models
{
    public class PostData
    {
        public string relativeFilePath { get; set; }
        public string fileName { get; set; }
        public string labelId { get; set; }
        public string justificationMessage { get; set; }
        public bool isCustom { get; set; }
        public List<UserRights> userRightsList { get; set; }
    }
}