using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIP_WebAPI.Models
{
    public class ResponseData
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string LabelName { get; set; }
        public string LabelId { get; set; }
        public bool IsProtected { get; set; }
        public string TargetUrl { get; set; }
    }
}