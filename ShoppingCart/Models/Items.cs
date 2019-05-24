using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingCart.Models
{
    public class Item
    {
        public string name { get; set; }
        public Weight weight { get; set; }
        public double price { get; set; }
    }
    public class Weight
    {
        public string unitOfMeasure { get; set; }
        public int value { get; set; }
    }
}
