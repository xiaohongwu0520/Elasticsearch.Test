using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic_CRUD.DTO
{
    /// <summary>
    /// 
    /// </summary>
    public class Aggregations
    {
        #region [ Properties ]

        /// <summary>
        /// 
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AggregationType { get; set; }

        #endregion

    }
}
