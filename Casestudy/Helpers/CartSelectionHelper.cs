using Casestudy.DAL.DomainClasses;
using System.Text.Json.Serialization;

namespace Casestudy.Helpers
{
    public class CartSelectionHelper
    {
        public int Qty { get; set; }
        [JsonPropertyName("item")]
        public Product? Product { get; set; }
    }
}
