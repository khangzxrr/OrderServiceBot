namespace OrderServiceBot
{
    public interface IBotScrapper: IDisposable
    {
        public ProductData Scrape(string url);
        public void Init();
    }
}
