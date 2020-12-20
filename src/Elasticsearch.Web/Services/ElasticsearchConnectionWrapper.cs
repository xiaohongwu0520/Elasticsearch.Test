using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elasticsearch.Web.Services
{
    public class ElasticsearchConnectionWrapper
    {
        private readonly object _lock = new object();
        private readonly Lazy<string> _connectionString;
        private volatile ElasticClient _connection;
        public ElasticsearchConnectionWrapper()
        {
            _connectionString = new Lazy<string>(GetConnectionString);
        }

        #region Utilities
        /// <summary>
        /// Get connection string to Redis cache from configuration
        /// </summary>
        /// <returns></returns>
        protected string GetConnectionString()
        {
            return "http://localhost:9200";
        }

        /// <summary>
        /// Get connection to Elasticsearch servers
        /// </summary>
        /// <returns></returns>
        protected ElasticClient GetConnection()
        {
            if (_connection != null)
                return _connection;

            lock (_lock)
            {
                if (_connection != null)
                    return _connection;

                _connection = new ElasticClient(new Uri(_connectionString.Value));
            }

            return _connection;
        }
        #endregion
    }
}
