using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elasticsearch.Web.Entities
{
    /// <summary>
    /// Represents a product type
    /// </summary>
    public enum ProductType
    {
        /// <summary>
        /// Simple
        /// </summary>
        SimpleProduct = 5,

        /// <summary>
        /// Grouped (product with variants)
        /// </summary>
        GroupedProduct = 10,
    }
}
