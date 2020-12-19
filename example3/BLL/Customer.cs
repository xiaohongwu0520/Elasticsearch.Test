using System;
using Nest;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest.Resolvers;

namespace Elastic_CRUD.BLL
{
    /// <summary>
    /// BLL for Customer entity
    /// </summary>
    public class Customer
    {
        #region [[Properties]]

        /// <summary>
        /// Elastic client
        /// </summary>
        DAL.EsClient _EsClientDAL;


        #endregion

        #region [[Constructors]]

        /// <summary>
        /// Constructor
        /// </summary>
        public Customer()
        {
            _EsClientDAL = new DAL.EsClient();
        }

        #endregion

        #region [[Methods]]

        /// <summary>
        /// Inserting or Updating a doc
        /// </summary>
        /// <param name="customer"></param>
        public bool Index(DTO.Customer customer)
        {
            //TODO: Validation
            var response = _EsClientDAL.Current.Index(customer, c => c.Type(DTO.Constants.DEFAULT_INDEX_TYPE));
            if (response.Created == false && response.ServerError != null)
                throw new Exception(response.ServerError.Error);
            else
                return true;
        }

        /// <summary>
        /// Deleting a row
        /// </summary>
        /// <param name="customer"></param>
        public bool Delete(string id)
        {
            //TODO: Validation
            return _EsClientDAL.Current.Delete(new Nest.DeleteRequest(DTO.Constants.DEFAULT_INDEX, DTO.Constants.DEFAULT_INDEX_TYPE, id.Trim())).Found;
        }

        /// <summary>
        /// Querying by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<DTO.Customer> QueryById(string id)
        {
            QueryContainer queryById = new TermQuery() { Field = "_id", Value = id.Trim() };

            var hits = _EsClientDAL.Current
                                   .Search<DTO.Customer>(s => s.Query(q => q.MatchAll() && queryById))
                                   .Hits;

            List<DTO.Customer> typedList = hits.Select(hit => ConvertHitToCustumer(hit)).ToList();

            return typedList; 
        }

        /// <summary>
        /// Querying by all fields with 'AND' operator
        /// </summary>
        /// <param name="costumer"></param>
        /// <returns></returns>
        public List<DTO.Customer> QueryByAllFieldsUsingAnd(DTO.Customer costumer)
        {
           IQueryContainer query = CreateSimpleQueryUsingAnd(costumer);

            var hits = _EsClientDAL.Current
                                   .Search<DTO.Customer>(s => s.Query(query))
                                   .Hits;

            List<DTO.Customer> typedList = hits.Select(hit => ConvertHitToCustumer(hit)).ToList();

            return typedList;            
        }

        /// <summary>
        /// Querying by all fields with 'OR' operator
        /// </summary>
        /// <param name="costumer"></param>
        /// <returns></returns>
        public List<DTO.Customer> QueryByAllFieldsUsingOr(DTO.Customer costumer)
        {
            IQueryContainer query = CreateSimpleQueryUsingOr(costumer);

            var hits = _EsClientDAL.Current
                                   .Search<DTO.Customer>(s => s.Query(query))
                                   .Hits;

            List<DTO.Customer> typedList = hits.Select(hit => ConvertHitToCustumer(hit)).ToList();

            return typedList;            
        }

        /// <summary>
        /// Anonymous method to translate from a Hit to our customer DTO
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        private DTO.Customer ConvertHitToCustumer(IHit<DTO.Customer> hit)
        {
            Func<IHit<DTO.Customer>, DTO.Customer> func = (x) =>
            {
                hit.Source.Id = Convert.ToInt32(hit.Id);
                return hit.Source;
            };

            return func.Invoke(hit);

            #region Notes
            /*
             Its a necessary workaround to get the "_id" property that remains in a upper level.
             Take this json return as sample:             
             
            {
            "_index": "crud_sample",
            "_type": "Customer_Info",
            "_id": "4",                   <- ID of the row
            "_score": 1,
            "_source": {                  <- All other properties are in the "_source" level: 
               "age": 32,
               "birthday": "19830101",
               "enrollmentFee": 100.1,
               "hasChildren": false,
               "name": "Juan",
               "opinion": "¿Qué tal estás?"
            }
             
             */
            #endregion
        }

        /// <summary>
        /// Create a query using all fields with 'AND' operator
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        private IQueryContainer CreateSimpleQueryUsingAnd(DTO.Customer customer)
        {
            QueryContainer queryContainer = null;

            queryContainer &= new TermQuery() { Field = "_id", Value = customer.Id };

            queryContainer &= new TermQuery() { Field = "name", Value = customer.Name };

            queryContainer &= new TermQuery() { Field = "age", Value = customer.Age };

            queryContainer &= new TermQuery() { Field = "birthday", Value = customer.Birthday };   

            queryContainer &= new TermQuery() { Field = "hasChildren", Value = customer.HasChildren };

            queryContainer &= new TermQuery() { Field = "enrollmentFee", Value = customer.EnrollmentFee };

            return queryContainer;
        }

        /// <summary>
        /// Create a query using all fields with 'AND' operator
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        private IQueryContainer CreateSimpleQueryUsingOr(DTO.Customer customer)
        {
            QueryContainer queryContainer = null;

            queryContainer |= new TermQuery() { Field = "_id", Value = customer.Id };

            queryContainer |= new TermQuery() { Field = "name", Value = customer.Name };

            queryContainer |= new TermQuery() { Field = "age", Value = customer.Age };

            queryContainer |= new TermQuery() { Field = "birthday", Value = customer.Birthday };

            queryContainer |= new TermQuery() { Field = "hasChildren", Value = customer.HasChildren };

            queryContainer |= new TermQuery() { Field = "enrollmentFee", Value = customer.EnrollmentFee };

            return queryContainer;
        }

        /// <summary>
        /// Querying combining fields
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public List<DTO.Customer> QueryUsingCombinations(DTO.CombinedFilter filter)
        {
            //Build Elastic "Should" filtering object for "Ages":          
            FilterContainer[] agesFiltering = new FilterContainer[filter.Ages.Count];
            for (int i = 0; i < filter.Ages.Count; i++)
            {
                FilterDescriptor<DTO.Customer> clause = new FilterDescriptor<DTO.Customer>();
                agesFiltering[i] = clause.Term("age", int.Parse(filter.Ages[i]));                                
            }

            //Build Elastic "Must Not" filtering object for "Names":
            FilterContainer[] nameFiltering = new FilterContainer[filter.Names.Count];
            for (int i = 0; i < filter.Names.Count; i++)
            {
                FilterDescriptor<DTO.Customer> clause = new FilterDescriptor<DTO.Customer>();
                nameFiltering[i] = clause.Term("name", filter.Names[i]);
            }

            //Run the combined query:
            var hits = _EsClientDAL.Current.Search<DTO.Customer>(s => s
                                                                   .Query(q => q
                                                                       .Filtered(fq => fq
                                                                       .Query(qq => qq.MatchAll())
                                                                       .Filter(ff => ff
                                                                           .Bool(b => b
                                                                               .Must(m1 => m1.Term("hasChildren", filter.HasChildren))
                                                                               .MustNot(nameFiltering)
                                                                               .Should(agesFiltering)
                                                                           )
                                                                        )
                                                                     )
                                                                  )
                                                                ).Hits;

            //Translate the hits and return the list
            List<DTO.Customer> typedList = hits.Select(hit => ConvertHitToCustumer(hit)).ToList();
            return typedList;    
        }

        /// <summary>
        /// Querying using ranges
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public List<DTO.Customer> QueryUsingRanges(DTO.RangeFilter filter)
        {

            FilterContainer[] ranges = new FilterContainer[2];

            //Build Elastic range filtering object for "Enrollment Fee": 
            FilterDescriptor<DTO.Customer> clause1 = new FilterDescriptor<DTO.Customer>();
            ranges[0] = clause1.Range(r => r.OnField(f => 
                                                        f.EnrollmentFee).Greater(filter.EnrollmentFeeStart)
                                                                        .Lower(filter.EnrollmentFeeEnd));

            //Build Elastic range filtering object for "Birthday": 
            FilterDescriptor<DTO.Customer> clause2 = new FilterDescriptor<DTO.Customer>();
            ranges[1] = clause2.Range(r => r.OnField(f => f.Birthday)
                                            .Greater(filter.Birthday.ToString(DTO.Constants.BASIC_DATE)));

            //Run the combined query:
            var hits = _EsClientDAL.Current
                                    .Search<DTO.Customer>(s => s
                                                           .Query(q => q
                                                               .Filtered(fq => fq
                                                               .Query(qq => qq.MatchAll())
                                                               .Filter(ff => ff
                                                                   .Bool(b => b
                                                                       .Must(ranges)
                                                                   )
                                                                )
                                                             )
                                                          )
                                                        ).Hits;


            //Translate the hits and return the list
            List<DTO.Customer> typedList = hits.Select(hit => ConvertHitToCustumer(hit)).ToList();
            return typedList;
        }

        /// <summary>
        /// Getting basic aggregations 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Dictionary<string, double> GetAggregations(DTO.Aggregations filter)
        {
            Dictionary<string, double> list = new Dictionary<string, double>();
            string agg_nickname = "customer_agg";

            switch (filter.AggregationType)
            {
                case "Count":
                        {
                            ExecuteCountAggregation(filter, list, agg_nickname);                        
                            break;
                        }


                case "Avg":
                        {
                            ExecuteAvgAggregation(filter, list, agg_nickname);                            
                            break;
                        }


                case "Sum":
                        {
                            ExecuteSumAggregation(filter, list, agg_nickname);
                            break;
                        }

                case "Min":
                        {
                            ExecuteMinAggregation(filter, list, agg_nickname);
                            break;
                        }

                case "Max":
                        {
                            ExecuteMaxAggregation(filter, list, agg_nickname);
                            break;
                        }


                default:
                    break;
            }


            return list;
        }

        /// <summary>
        /// Get Sum Aggregation
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="list"></param>
        /// <param name="agg_nickname"></param>
        private void ExecuteSumAggregation(DTO.Aggregations filter, Dictionary<string, double> list, string agg_nickname)
        {
            var response = _EsClientDAL.Current.Search<DTO.Customer>(s => s
                                                                     .Aggregations(a => a
                                                                          .Sum(agg_nickname, st => st
                                                                              .Field(filter.Field)
                                                                                )
                                                                            )
                                                                      );

            list.Add(filter.Field + " Sum", response.Aggs.Sum(agg_nickname).Value.Value);
        }

        /// <summary>
        /// Get Avg Aggregation
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="list"></param>
        /// <param name="agg_nickname"></param>
        private void ExecuteAvgAggregation(DTO.Aggregations filter, Dictionary<string, double> list, string agg_nickname)
        {
            var response = _EsClientDAL.Current.Search<DTO.Customer>(s => s
                                                                     .Aggregations(a => a
                                                                          .Average(agg_nickname, st => st
                                                                              .Field(filter.Field)
                                                                                )
                                                                            )
                                                                      );

            list.Add(filter.Field + " Average", response.Aggs.Average(agg_nickname).Value.Value);
        }

        /// <summary>
        /// Get Count Aggregation
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="list"></param>
        /// <param name="agg_nickname"></param>
        private void ExecuteCountAggregation(DTO.Aggregations filter, Dictionary<string, double> list, string agg_nickname)
        {
            var response = _EsClientDAL.Current.Search<DTO.Customer>(s => s
                                                                     .Aggregations(a => a
                                                                          .Terms(agg_nickname, st => st
                                                                              .Field(filter.Field)
                                                                              .Size(int.MaxValue)
                                                                              .ExecutionHint(TermsAggregationExecutionHint.GlobalOrdinals)
                                                                                )
                                                                            )
                                                                      );

            foreach (var item in response.Aggs.Terms(agg_nickname).Items)
            {
                list.Add(item.Key, item.DocCount);
            }
        }

        /// <summary>
        /// Get Min Aggregation
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="list"></param>
        /// <param name="agg_nickname"></param>
        private void ExecuteMinAggregation(DTO.Aggregations filter, Dictionary<string, double> list, string agg_nickname)
        {
            var response = _EsClientDAL.Current.Search<DTO.Customer>(s => s
                                                                     .Aggregations(a => a
                                                                          .Min(agg_nickname, st => st
                                                                              .Field(filter.Field)
                                                                                )
                                                                            )
                                                                      );

            list.Add(filter.Field + " Min", response.Aggs.Sum(agg_nickname).Value.Value);
        }

        /// <summary>
        /// Get Max Aggregation
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="list"></param>
        /// <param name="agg_nickname"></param>
        private void ExecuteMaxAggregation(DTO.Aggregations filter, Dictionary<string, double> list, string agg_nickname)
        {
            var response = _EsClientDAL.Current.Search<DTO.Customer>(s => s
                                                                     .Aggregations(a => a
                                                                          .Max(agg_nickname, st => st
                                                                              .Field(filter.Field)
                                                                                )
                                                                            )
                                                                      );

            list.Add(filter.Field + " Max", response.Aggs.Sum(agg_nickname).Value.Value);
        }

        #endregion
    }
}
