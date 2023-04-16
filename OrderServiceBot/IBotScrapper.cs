

using OrderService.Core.RabbitMqDto;

namespace OrderServiceBot
{
    public interface IBotScrapper: IDisposable
    {
        public RabbitResponseProductData Scrape(RabbitRequestProductData rabbitProductRequest);
        public void Init();
    }
}
