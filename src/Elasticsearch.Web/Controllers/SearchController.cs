using Elasticsearch.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elasticsearch.Web.Controllers
{
    public class SearchController : Controller
    {
        private IElasticClient GetClient()
        {
            var indexName = "productsearch";

            var _connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200"))
                 .DefaultIndex(indexName)
                 //.DefaultMappingFor<ProductModel>(i => i
                 //    .IndexName(indexName)
                 //)
                 //.EnableDebugMode()
                 .PrettyJson()
                 .RequestTimeout(TimeSpan.FromMinutes(2));

            var client = new ElasticClient(_connectionSettings);
            return client;
        }
        public IActionResult Index()
        {
           
            return View();
        }
    }
}
