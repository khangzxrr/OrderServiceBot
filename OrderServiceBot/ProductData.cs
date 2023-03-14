using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderServiceBot
{
    public class ProductData
    {
        public string title { get; }
        public string category { get; }
        public string price { get; }
        public string ship { get; }

        public ProductData(string title, string category, string price, string ship)
        {
            this.title = title;
            this.category = category;
            this.price = price;
            this.ship = ship;
        }
    }
}
