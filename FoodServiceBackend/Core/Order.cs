using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodServiceBackend.Core
{
    public class Order
    {
        public string Token { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string AdditionInformation { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
    }
}
