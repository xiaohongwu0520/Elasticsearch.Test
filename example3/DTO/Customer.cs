using Nest;
using System;

namespace Elastic_CRUD.DTO
{
    /// <summary>
    /// Customer entity
    /// </summary>
    [ElasticType(Name = "Customer_Info")]
    public class Customer
    {
        #region [[Properties]]

        /// <summary>
        /// _id field
        /// </summary>
        [ElasticProperty(Name="_id", NumericType = NumberType.Long)]
        public int Id { get; set; }

        /// <summary>
        /// name field
        /// </summary>
        [ElasticProperty(Name = "name", Index = FieldIndexOption.NotAnalyzed)]
        public string Name { get; set; }

        /// <summary>
        /// age field
        /// </summary>
        [ElasticProperty(Name = "age", NumericType = NumberType.Integer)]
        public int Age { get; set; }

        /// <summary>
        /// birthday field
        /// </summary>
        [ElasticProperty(Name = "birthday", Type = FieldType.Date, DateFormat = "basic_date")]
        public string Birthday { get; set; }

        /// <summary>
        /// haschildren field
        /// </summary>
        [ElasticProperty(Name = "hasChildren")]
        public bool HasChildren { get; set; }

        /// <summary>
        /// enrollmentFee field
        /// </summary>
        [ElasticProperty(Name = "enrollmentFee", NumericType = NumberType.Double)]
        public double EnrollmentFee { get; set; }

        /// <summary>
        /// opnion field
        /// </summary>
        [ElasticProperty(Name = "opinion", Index = FieldIndexOption.NotAnalyzed)]
        public string Opinion { get; set; }

        #endregion
    }
}

