using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic_CRUD.DTO
{
    /// <summary>
    /// System constant values
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Elastic index name
        /// </summary>
        public const string DEFAULT_INDEX = "crud_sample";

        /// <summary>
        /// Elastic type of a given index
        /// </summary>
        public const string DEFAULT_INDEX_TYPE = "Customer_Info";

        /// <summary>
        /// Basic date format
        /// </summary>
        public const string BASIC_DATE = "yyyyMMdd";
    }
}
