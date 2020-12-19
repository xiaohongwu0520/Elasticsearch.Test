using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic_CRUD.DTO
{
    /// <summary>
    /// Represents a sample of a range filter
    /// </summary>
    public class RangeFilter
    {
        #region [ Properties ]

        /// <summary>
        /// Starting range
        /// </summary>
        public double EnrollmentFeeStart { get; set; }

        /// <summary>
        /// Ending range
        /// </summary>
        public double EnrollmentFeeEnd { get; set; }

        /// <summary>
        /// Customer birthday
        /// </summary>
        public DateTime Birthday { get; set; }

        #endregion
    }
}
