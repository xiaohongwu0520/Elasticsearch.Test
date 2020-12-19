using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Newtonsoft;
using Elasticsearch.Net;

namespace Elastic_CRUD.DAL
{
    /// <summary>
    /// Elastic client
    /// </summary>
    public class EsClient
    {
        #region [[Properties]]

        /// <summary>
        /// URI 
        /// </summary>
        private const string ES_URI = "http://localhost:9200";

        /// <summary>
        /// Elastic settings
        /// </summary>
        private ConnectionSettings _settings;

        /// <summary>
        /// Current instantiated client
        /// </summary>
        public ElasticClient Current { get; set; }

        #endregion

        #region [[Constructors]]

        /// <summary>
        /// Constructor
        /// </summary>
        public EsClient()
        {
            var node = new Uri(ES_URI);

            _settings = new ConnectionSettings(node);
            _settings.SetDefaultIndex(DTO.Constants.DEFAULT_INDEX);
            _settings.MapDefaultTypeNames(m => m.Add(typeof(DTO.Customer), DTO.Constants.DEFAULT_INDEX_TYPE));

            Current = new ElasticClient(_settings);
            Current.Map<DTO.Customer>(m => m.MapFromAttributes());
            
        }

        #endregion
    }
}
