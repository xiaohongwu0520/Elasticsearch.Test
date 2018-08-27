using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearch.Web.Test.Models
{
    public class ProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }

        public string[] Tags { get; set; }

        public int ReviewCount { get; set; }

        public PictureModel DefaultPictureModel { get; set; }

        public CompletionField Suggest { get; set; }
    }
}
