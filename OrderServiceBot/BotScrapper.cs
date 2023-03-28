

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace OrderServiceBot
{
    public class BotScrapper : IBotScrapper
    {
        private WebDriver driver;

        public void Dispose()
        {
            driver.Quit();
            driver.Dispose();
        }

        public void Init()
        {
            ChromeOptions chromeOptions = new ChromeOptions();

            chromeOptions.AddArgument("--no-sandbox");

            driver = new RemoteWebDriver(new Uri("http://host.docker.internal:4444"), chromeOptions);
        }

        public ProductData Scrape(string url)
        {
            driver.Navigate().GoToUrl(url);

            var titleSelector = By.CssSelector("#LeftSummaryPanel h1 > span");
            var title = driver.FindElement(titleSelector).Text;

            var categorySelector = By.CssSelector("ul > li > a > span");
            var category = driver.FindElement(categorySelector).Text;

            var priceSelector = By.CssSelector("span[itemprop=\"price\"]");
            var price = driver.FindElement(priceSelector).Text;

            var shipSelector = By.CssSelector("div.ux-labels-values__values > div > div > span");
            var ship = driver.FindElement(shipSelector).Text;

            Console.WriteLine($"======\nCatalog: {category}\nProduct: {title}\nPrice: {price}\nShip:{ship}\n");

            ProductData productData = new ProductData(category, title, price, ship, url);   

            return productData;
        }
    }

}
