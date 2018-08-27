using Nest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearch.Web.Test.Models
{
    public class SearchViewModel
    {
        public SearchViewModel()
        {
            this.Hits = new ReadOnlyCollection<IHit<ProductModel>>(new List<IHit<ProductModel>>());

            
        }

        /// <summary>
		/// The current state of the form that was submitted
		/// </summary>
		public SearchForm Form { get; set; }
        /// <summary>
		/// The total number of matching results
		/// </summary>
		public long Total { get; set; }

        /// <summary>
        /// The total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// The current page of package results
        /// </summary>
        public IReadOnlyCollection<IHit<ProductModel>> Hits { get; set; }


    }
}
