using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ElasticSearch.Web.Test.Models;
using Nest;
using Alvin.Data;
using Microsoft.EntityFrameworkCore;
using Alvin.Core.Domain.Catalog;
using System.Threading;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.Net.Http.Headers;
using BLun.ETagMiddleware;

namespace ElasticSearch.Web.Test.Controllers
{
    public class HomeController : Controller
    {
        private AnalysisDescriptor Analysis(AnalysisDescriptor analysis) => analysis
            .Tokenizers(tokenizers => tokenizers
                .Pattern("name-tokenizer", p => p.Pattern(@"\W+"))
            )
            .TokenFilters(tokenfilters => tokenfilters
                .WordDelimiter("name-words", w => w
                    .SplitOnCaseChange()
                    .PreserveOriginal()
                    .SplitOnNumerics()
                    .GenerateNumberParts(false)
                    .GenerateWordParts()
                )
            )
            .Analyzers(analyzers => analyzers
                .Custom("name-keyword", c => c
                    .Tokenizer("keyword")
                    .Filters("lowercase")
                )
               .Custom("html_stripper", cc => cc
                    .Filters("trim", "lowercase")
                    .CharFilters("html_strip")
                    .Tokenizer("name-tokenizer")
                )
                .Custom("name-analyzer", c => c
                .Filters("name-words", "lowercase")
                .Tokenizer("ik_max_word")
                )
            );

        private static TypeMappingDescriptor<ProductModel> MapProduct(TypeMappingDescriptor<ProductModel> map) => map
            .AutoMap()
            .Properties(ps => ps
                .Number(t => t
                    .Name(p => p.Id)
                    .Type(NumberType.Integer)
                )
                .Text(t => t
                    .Name(p => p.Name)
                    .Analyzer("ik_max_word")
                )
                .Text(t => t
                    .Name(p => p.ShortDescription)
                    .Analyzer("ik_max_word")
                )
                .Number(t => t
                    .Name(p => p.ReviewCount)
                    .Type(NumberType.Integer)
                )
                .Keyword(k => k
                    .Name(p => p.Tags)
                )
                .Completion(c => c
                    .Name(p => p.Suggest)
                )
            );

        private ProductModel ProductEntityToProductModel(Product product)
        {
            var model = new ProductModel();
            model.Id = product.Id;
            model.Name = product.Name;
            model.FullDescription = string.IsNullOrEmpty(product.FullDescription) ? "" : _htmlEncoder.Encode(product.FullDescription);
            model.ShortDescription = product.ShortDescription;
            model.Tags = product.ProductProductTagMappings.Any() ? product.ProductProductTagMappings.Select(s => s.ProductTag.Name).ToArray() : new List<string>().ToArray();
            model.ReviewCount = product.ProductReviews.Count();
            return model;
        }

        private ElasticClient GetClient()
        {
            var indexName = "productsearch";
            var _connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex(indexName)
                .DefaultMappingFor<ProductModel>(i => i
                    .TypeName("product")
                    .IndexName(indexName)
                );

            var client = new ElasticClient(_connectionSettings);
            return client;
        }

        HtmlEncoder _htmlEncoder;
        JavaScriptEncoder _javaScriptEncoder;
        UrlEncoder _urlEncoder;
        DbContextOptions<AlvinObjectContext> _dbContexOptions;
        public HomeController(HtmlEncoder htmlEncoder,
                             JavaScriptEncoder javascriptEncoder,
                             UrlEncoder urlEncoder, DbContextOptions<AlvinObjectContext> dbContexOptions)
        {
            _htmlEncoder = htmlEncoder;
            _javaScriptEncoder = javascriptEncoder;
            _urlEncoder = urlEncoder;
            _dbContexOptions = dbContexOptions;
        }
        public IActionResult Index()
        {
            var client = GetClient();
            //ik_smart ik_max_word
            var analyzeResponse = client.Analyze(a => a
                    .Analyzer("ik_max_word")
                    .Text("商品促销信息以商品详情页“促销”栏中的信息为准；商品的具体售价以订单结算页价格为准；如您发现活动商品售价或促销信息有异常，建议购买前先联系销售商咨询桂林山水甲天下刘德华MIIDDJ")
            );
            ViewBag.AnalyzeTokens = analyzeResponse.Tokens;
            return View();
        }

        // Can add on controller
        //[ETag(ETagAlgorithm = ETagAlgorithm.SHA521)]
        public IActionResult ReadDic()
        {
            var str= $"刘德华{Environment.NewLine}刘一华{Environment.NewLine}MI";
            //return Content(str, "text/plain;charset=utf-8");
            var entityTag = new EntityTagHeaderValue("\"CalculatedEtagValue\"");
            return File(Encoding.UTF8.GetBytes(str), "text/plain;charset=utf-8", "downloadName.txt", lastModified: DateTime.UtcNow.AddSeconds(-5), entityTag: entityTag);
        }


        public IActionResult CreateIndex()
        {
            var indexName = "productsearch";

            var _connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200"))
                 .DefaultIndex(indexName)
                 .DefaultMappingFor<ProductModel>(i => i
                     .TypeName("product")
                     .IndexName(indexName)
                 );

            var client = new ElasticClient(_connectionSettings);

            if (client.IndexExists(indexName).Exists)
                client.DeleteIndex(indexName);

            var createIndexResonse = client.CreateIndex(indexName, i => i
                  .Settings(s => s
                      .NumberOfShards(2)
                      .NumberOfReplicas(0)
                      .Analysis(Analysis)
                  )
                  .Mappings(m => m
                      .Map<ProductModel>(MapProduct)
                  )
             );
            if (!createIndexResonse.IsValid)
            {

            }
            //var connection = @"Data Source=.;Initial Catalog=NopCommerceNew;Integrated Security=False;Persist Security Info=False;User ID=sa;Password=123456";
            //var contextOptionsBuilder = new DbContextOptionsBuilder<AlvinObjectContext>().UseSqlServer(connection);
            //var context = new AlvinObjectContext(contextOptionsBuilder.Options);
            var context = new AlvinObjectContext(_dbContexOptions);
            Alvin.Core.Data.IRepository<Product> _productRepository = new EfRepository<Product>(context);

            var products = _productRepository.Table
                               //.Include(a => a.ProductProductTagMappings)
                               //    .ThenInclude(map => map.ProductTag)
                               //.Include(a => a.ProductReviews)
                               ;

            var lists = products.AsEnumerable().Select(product =>
            {
                var model = new ProductModel();
                model.Id = product.Id;
                model.Name = product.Name;
                model.FullDescription = string.IsNullOrEmpty(product.FullDescription) ? "" : _htmlEncoder.Encode(product.FullDescription);
                model.ShortDescription = product.ShortDescription;
                model.Tags = product.ProductProductTagMappings.Any() ? product.ProductProductTagMappings.Select(s => s.ProductTag.Name).ToArray() : new List<string>().ToArray();
                model.ReviewCount = product.ProductReviews.Count();
                model.Suggest = new CompletionField
                {
                    Input = new List<string>() { product.Name },
                    Weight = product.ProductReviews.Count()
                };
                return model;
            });

            //var lists = new List<ProductModel>();
            //foreach (var product in products)
            //{
            //    var model = new ProductModel();
            //    model.Id = product.Id;
            //    model.Name = product.Name;
            //    model.FullDescription = string.IsNullOrEmpty(product.FullDescription) ? "" : _htmlEncoder.Encode(product.FullDescription);
            //    model.ShortDescription = product.ShortDescription;
            //    model.Tags = product.ProductProductTagMappings.Any() ? product.ProductProductTagMappings.Select(s => s.ProductTag.Name).ToArray() : new List<string>().ToArray();
            //    model.ReviewCount = product.ProductReviews.Count();
            //    lists.Add(model);
            //}

            var waitHandle = new CountdownEvent(1);

            var bulkAll = client.BulkAll(lists, b => b
                .Index(indexName)
                .BackOffRetries(2)
                .BackOffTime("30s")
                .RefreshOnCompleted(true)
                .MaxDegreeOfParallelism(4)
                .Size(1000)
            );

            bulkAll.Subscribe(new BulkAllObserver(
                onNext: b => Console.Write("."),
                onError: e => throw e,
                onCompleted: () => waitHandle.Signal()
            ));

            waitHandle.Wait(TimeSpan.FromMinutes(30));
            return View();
        }

        public IActionResult Search(SearchForm form)
        {
            var indexName = "productsearch";

            var _connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200"))
                 .DefaultIndex(indexName)
                 .DefaultMappingFor<ProductModel>(i => i
                     .TypeName("product")
                     .IndexName(indexName)
                 );

            var client = new ElasticClient(_connectionSettings);
            int pageIndwx = (form.Page - 1) * form.PageSize;
            int pageSize = form.PageSize;
            var queryStr = string.IsNullOrEmpty(form.Query) ? "商品" : form.Query;
            var result = client.Search<ProductModel>(s => s
                .From(pageIndwx)
                .Size(pageSize)
                .Sort(sort =>
                {
                    return sort.Descending(p => p.ReviewCount);
                    //return sort.Descending(SortSpecialField.Score);
                })
                .Query(q => (q
                    .Match(m => m
                            .Field(p => p.Name.Suffix("keyword"))
                            .Boost(1000)
                            .Query(queryStr)
                            ) || q
                    .FunctionScore(fs => fs
                            .BoostMode(FunctionBoostMode.Multiply)
                            .ScoreMode(FunctionScoreMode.Sum)
                            .Functions(ff => ff
                                .FieldValueFactor(fvf => fvf
                                    .Field(p => p.ReviewCount)
                                    .Factor(0.0001)
                                    .Modifier(FieldValueFactorModifier.None)
                                )
                            )
                            .Query(query => query
                                     .MultiMatch(m => m
                                        .Fields(f => f
                                            .Field(p => p.Name, 1.5)
                                            .Field(p => p.ShortDescription, 0.8)
                                        )
                                        .Operator(Operator.Or)
                                        .Query(queryStr)
                                      )
                                )
                    ))
                )
                .Highlight(h => h
                    .PreTags("<span style=\"color: red\">")
                    .PostTags("</span>")
                    .Fields(ff => ff
                                .Field(p => p.Name)
                                .NumberOfFragments(2)
                                .FragmentSize(250)
                                .NoMatchSize(200),
                            ff => ff
                                .Field(f => f.ShortDescription)
                                .NumberOfFragments(2)
                                .FragmentSize(250)
                                .NoMatchSize(200)
                    )
                )
             );

            var model = new SearchViewModel
            {
                Hits = result.Hits,
                Total = result.Total,
                Form = form,
                TotalPages = (int)Math.Ceiling(result.Total / (double)pageSize),
                //Authors = authors
            };
            return View(model);
        }
        [HttpPost]
        public IActionResult Suggest([FromBody]SearchForm form)
        {
            var client = GetClient();
            var result = client.Search<ProductModel>(s => s
                //.Index<ProductModel>()
                .Source(sf => sf
                    .Includes(f => f
                        .Field(ff => ff.Id)
                        .Field(ff => ff.ReviewCount)
                        .Field(ff => ff.Name)
                        .Field(ff => ff.ShortDescription)
                    )
                )
                .Suggest(su => su
                    .Completion("package-suggestions", c => c
                        .Prefix(form.Query)
                        .Field(p => p.Suggest).Size(10)
                    )
                )
            );

            var suggestions = result.Suggest["package-suggestions"]
                .FirstOrDefault()
                .Options
                .Select(suggest => new
                {
                    id = suggest.Source.Id,
                    ReviewCount = suggest.Source.ReviewCount,
                    Name = !string.IsNullOrEmpty(suggest.Source.Name)
                        ? string.Concat(suggest.Source.Name.Take(200))
                        : string.Empty,
                    ShortDescription = suggest.Source.ShortDescription
                });

            return Json(suggestions);
        }
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
