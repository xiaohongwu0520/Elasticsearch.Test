using Nest;
using System;

namespace Elasticsearch.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("people");

            var client = new ElasticClient(settings);

            var analyzeResponse = client.Analyze(a => a.Analyzer("standard").Text("F# is THE SUPERIOR language :)"));
            foreach (var analyzeToken in analyzeResponse.Tokens)
            {
                Console.WriteLine($"{analyzeToken.Token}");
            }

            var person = new Person
            {
                Id = 1,
                FirstName = "Martijn",
                LastName = "Laarman wu"
            };

            var indexResponse = client.IndexDocument(person);

            var person2 = new Person
            {
                Id = 2,
                FirstName = "Xiao ad a",
                LastName = "Hongwu ddd f hong"
            };

            var indexResponse2 = client.IndexDocument(person2);

            var person3 = new Person
            {
                Id = 3,
                FirstName = "Wang wo",
                LastName = "Baoyi bb b xiao"
            };

            var indexResponse3 = client.IndexDocument(person3);

            var person4 = new Person
            {
                Id = 4,
                FirstName = "Liu",
                LastName = "Chenxu Xiao"
            };

            var indexResponse4 = client.IndexDocument(person4);

            var person5 = new Person
            {
                Id = 5,
                FirstName = "Xiao",
                LastName = "Chenxu xu hong"
            };

            var indexResponse5 = client.IndexDocument(person5);

            //var query = new Nest.SearchDescriptor<Person>();
            //var ddd = query.Query(q => q.QueryString(t => t.Fields(f => f.Field(obj => obj.FirstName).Field(obj => obj.LastName)).Query("ao")));
            //var dddd = client.Search<Person>(s => query);
            var searchResponse = client.Search<Person>(s => s
                                                           .From(0)
                                                           .Size(10)
                                                           .Query(q => q
                                                                .Match(m => m
                                                                   .Field(f => f.LastName)
                                                                   .Query("xu").Fuzziness(Fuzziness.Auto)
                                                                )
                                                           )
                                                       );

            var people = searchResponse.Documents;

            var searchResponse2 = client.Search<Person>(s => s
    //.Size(0)
    .Query(q => q
         .Match(m => m
            .Field(f => f.FirstName)
            .Query("Xiao")
         )
    )
    .Aggregations(a => a
        .Terms("lastname", ta => ta
            .Field(f => f.LastName)
        )
    )
);

            var termsAggregation = searchResponse2.Aggregations.Terms("lastname");

            var searchRequest = new SearchRequest<Person>(Indices.All, Types.All)
            {
                From = 0,
                Size = 10,
                Query = new MatchQuery
                {
                    Field = Infer.Field<Person>(f => f.FirstName),
                    Query = "Martijn"
                }
            };

            var searchResponse3 = client.Search<Person>(searchRequest);

        }
    }
}
