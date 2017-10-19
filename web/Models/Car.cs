using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web.Models
{
    public class Car
    {
        [JsonProperty(PropertyName = "id")]
        public int Id
        { get; set; }

        [JsonProperty(PropertyName = "make")]
        public string Make
        { get; set; }

        [JsonProperty(PropertyName = "model")]
        public string Model
        { get; set; }


    }
}