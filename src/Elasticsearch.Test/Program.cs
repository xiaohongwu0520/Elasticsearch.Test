using HtmlAgilityPack;
using Nest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Elasticsearch.Test
{

    public class ProductModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public string Publisher { get; set; }
    }


    class Program
    {
        private static IElasticClient GetClient()
        {
            var indexName = "productsearch";

            var _connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200"))
                 .DefaultIndex(indexName)
                 .DefaultMappingFor<ProductModel>(i => i
                     .IndexName(indexName)
                 )
                 //.EnableDebugMode()
                 .PrettyJson()
                 .RequestTimeout(TimeSpan.FromMinutes(2));

            var client = new ElasticClient(_connectionSettings);
            return client;
        }
        static void Main(string[] args)
        {
            var _client = GetClient();
            ////设置最大返回记录条数 默认：10000
            ////https://www.elastic.co/guide/en/elasticsearch/reference/current/index-modules.html#dynamic-index-settings
            //var updateIndexSettingsResponse = _client.Indices.UpdateSettings("productsearch", u => u
            //    .IndexSettings(di => di
            //        .Setting("index.max_result_window", 20000)
            //    )
            //);
            var searchResponse = _client.Search<ProductModel>(s => s
                .From(0)
                .Size(15000)
                .Query(q => q
                     .Match(m => m
                        .Field(f => f.Name)
                        .Query("水果")
                     )
                )
            );
            var productList = searchResponse.Documents;
            foreach (var item in productList)
            {
                Console.WriteLine($"{item.Id}-----{item.Name}");
            }
            Console.WriteLine("完成！！！！");
            Console.ReadLine();
            return;
            List<ProductModel> products = new List<ProductModel>();
            var keywords = new List<string>() { "java", "职业", "水果" };
            foreach (var keyword in keywords)
            {
                //var keyword = "java";
                var pageIndex = 1;
                var totalPage = 1;
                var IsCalculatedPage = false;
                do
                {
                    var url = $"https://search.jd.com/Search?keyword={keyword}&page={pageIndex}";
                    var web = new HtmlWeb();
                    web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.66 Safari/537.36";
                    var doc = web.Load(url);
                    var listNode = doc.GetElementbyId("J_goodsList");
                    var totalPageNode = doc.DocumentNode.SelectSingleNode("//span[@class='fp-text']");
                    if (totalPageNode != null)
                    {
                        if (!IsCalculatedPage)
                        {
                            var pageInfo = totalPageNode.InnerText;
                            if (!string.IsNullOrEmpty(pageInfo))
                            {
                                pageInfo = pageInfo.Replace("\t", "").Replace("\n", "");
                                totalPage = int.Parse(pageInfo.Substring(pageInfo.IndexOf("/") + 1));
                                IsCalculatedPage = true;
                            }
                        }
                    }
                    if (listNode != null)
                    {
                        var list = listNode.SelectNodes("./ul/li");
                        foreach (var li in list)
                        {
                            ProductModel product = new ProductModel();
                            product.Id = products.Count + 1;
                            var imgNode = li.SelectSingleNode("./div[@class='gl-i-wrap']//div[@class='p-img']/a/img");
                            if (imgNode != null)
                            {
                                var imageUrl = imgNode.GetAttributeValue("data-lazy-img", "");
                                product.ImageUrl = imageUrl;
                            }
                            var priceNode = li.SelectSingleNode("./div[@class='gl-i-wrap']//div[@class='p-price']/strong/i");
                            if (priceNode != null)
                            {
                                var price = priceNode.InnerText;
                                if (!string.IsNullOrEmpty(price))
                                {
                                    product.Price = decimal.Parse(price);
                                }
                            }
                            var nameNode = li.SelectSingleNode("./div[@class='gl-i-wrap']//div[contains(@class,'p-name')]/a/em");
                            if (nameNode != null)
                            {
                                var name = nameNode.InnerText;
                                product.Name = name;
                                Console.WriteLine(name);
                            }
                            var shopNode = li.SelectSingleNode("./div[@class='gl-i-wrap']/div[@class='p-shopnum']/a");
                            if (shopNode != null)
                            {
                                var publisher = shopNode.InnerText;
                                product.Publisher = publisher;
                            }
                            products.Add(product);
                        }
                    }
                    pageIndex++;
                } while (pageIndex <= totalPage);
            }

            var indexName = "productsearch";

            var _connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200"))
                 //.DefaultIndex(indexName)
                 .DefaultMappingFor<ProductModel>(i => i
                     .IndexName(indexName)
                 )
                 //.EnableDebugMode()
                 .PrettyJson()
                 .RequestTimeout(TimeSpan.FromMinutes(2));

            var client = new ElasticClient(_connectionSettings);

            if (client.Indices.Exists(indexName).Exists)
            {
                //client.Indices.Delete(indexName);
            }
            else
            {
                Dictionary<string, object> container = new Dictionary<string, object>();
                container.Add("index.max_result_window", 20000);
                //不存在索引创建
                IIndexState indexState = new IndexState
                {

                    Settings = new IndexSettings(container)
                    {
                        NumberOfReplicas = 0, //副本数量

                        NumberOfShards = 2 //分片数量
                    }
                };

                var createIndexResonse = client.Indices.Create(indexName, i => i
                                                                .InitializeUsing(indexState)
                                                                .Map<ProductModel>(p => p
                                                                .AutoMap()
                                                                .Properties(props => props
                                                                            .Number(t => t
                                                                                .Name(p => p.Id)
                                                                                .Type(NumberType.Integer)
                                                                            )
                                                                            .Keyword(t => t
                                                                                 .Name(p => p.ImageUrl)
                                                                            )
                                                                            .Text(t => t
                                                                                .Name(p => p.Name)
                                                                                .Analyzer("ik_max_word")
                                                                            )
                                                                            .Text(t => t
                                                                                .Name(p => p.Publisher)
                                                                                .Analyzer("ik_max_word")
                                                                            )
                                                                            .Number(t => t
                                                                                .Name(p => p.Price)
                                                                                .Type(NumberType.Float)
                                                                            )
                                                                            .Number(t => t
                                                                                .Name(p => p.OldPrice)
                                                                                .Type(NumberType.Float)
                                                                            )
                                                                        )
                                                                ));
                if (!createIndexResonse.IsValid)
                {

                }
            }

            var waitHandle = new CountdownEvent(1);

            var bulkAll = client.BulkAll(products, b => b.Index(indexName)
                 .BackOffRetries(2)
                 .BackOffTime("30s")
                 .RefreshOnCompleted(true)
                 .MaxDegreeOfParallelism(4)
                 .Size(100)
            );

            bulkAll.Subscribe(new BulkAllObserver(
               onNext: b => Console.Write("."),
                   onError: e => throw e,
                   onCompleted: () => waitHandle.Signal()
               ));

            waitHandle.Wait(TimeSpan.FromMinutes(30));

            //var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("people");

            //            var client = new ElasticClient(settings);

            //            var analyzeResponse = client.Indices.Analyze(a => a.Analyzer("standard").Text("F# is THE SUPERIOR language :)"));
            //            foreach (var analyzeToken in analyzeResponse.Tokens)
            //            {
            //                Console.WriteLine($"{analyzeToken.Token}");
            //            }

            //            var person = new Person
            //            {
            //                Id = 1,
            //                FirstName = "Martijn",
            //                LastName = "Laarman wu"
            //            };

            //            var indexResponse = client.IndexDocument(person);

            //            var person2 = new Person
            //            {
            //                Id = 2,
            //                FirstName = "Xiao ad a",
            //                LastName = "Hongwu ddd f hong"
            //            };

            //            var indexResponse2 = client.IndexDocument(person2);

            //            var person3 = new Person
            //            {
            //                Id = 3,
            //                FirstName = "Wang wo",
            //                LastName = "Baoyi bb b xiao"
            //            };

            //            var indexResponse3 = client.IndexDocument(person3);

            //            var person4 = new Person
            //            {
            //                Id = 4,
            //                FirstName = "Liu",
            //                LastName = "Chenxu Xiao"
            //            };

            //            var indexResponse4 = client.IndexDocument(person4);

            //            var person5 = new Person
            //            {
            //                Id = 5,
            //                FirstName = "Xiao",
            //                LastName = "Chenxu xu hong"
            //            };

            //            var indexResponse5 = client.IndexDocument(person5);

            //            //var query = new Nest.SearchDescriptor<Person>();
            //            //var ddd = query.Query(q => q.QueryString(t => t.Fields(f => f.Field(obj => obj.FirstName).Field(obj => obj.LastName)).Query("ao")));
            //            //var dddd = client.Search<Person>(s => query);
            //            var searchResponse = client.Search<Person>(s => s
            //                                                           .From(0)
            //                                                           .Size(10)
            //                                                           .Query(q => q
            //                                                                .Match(m => m
            //                                                                   .Field(f => f.LastName)
            //                                                                   .Query("xu").Fuzziness(Fuzziness.Auto)
            //                                                                )
            //                                                           )
            //                                                       );

            //            var people = searchResponse.Documents;

            //            var searchResponse2 = client.Search<Person>(s => s
            //    //.Size(0)
            //    .Query(q => q
            //         .Match(m => m
            //            .Field(f => f.FirstName)
            //            .Query("Xiao")
            //         )
            //    )
            //    .Aggregations(a => a
            //        .Terms("lastname", ta => ta
            //            .Field(f => f.LastName)
            //        )
            //    )
            //);

            //            var termsAggregation = searchResponse2.Aggregations.Terms("lastname");

            //            var searchRequest = new SearchRequest<Person>(Indices.All)
            //            {
            //                From = 0,
            //                Size = 10,
            //                Query = new MatchQuery
            //                {
            //                    Field = Infer.Field<Person>(f => f.FirstName),
            //                    Query = "Martijn"
            //                }
            //            };

            //            var searchResponse3 = client.Search<Person>(searchRequest);
            Console.WriteLine("");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("任务完成......");
            Console.WriteLine("----------------------------------------------");
            Console.ReadLine();
        }
    }
}
