namespace OrderServiceBot
{
    public class ProductData
    {
        public string Catalog { get; set; }
        public string Product { get; set; }
        public string Price { get; set; }
        public string Ship { get; set; }

        public string Url { get; set; }

        public ProductData(string catalog, string product, string price, string ship, string url)
        {
            Catalog = catalog;
            Product = product;
            Price = price;
            Ship = ship;
            Url = url;
        }
    }
}
