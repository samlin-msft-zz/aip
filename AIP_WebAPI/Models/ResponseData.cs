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
        public string TargetUrl { get; set; }
    }
}