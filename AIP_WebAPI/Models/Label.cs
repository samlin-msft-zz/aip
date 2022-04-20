using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIP_WebAPI.Models
{
    public class Label
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Label> Children { get; set; }
        public int Sensitivity { get; set; }
        public string Description { get; set; }
    }
}