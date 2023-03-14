using OpenQA.Selenium;
using SeleniumUndetectedChromeDriver;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace OrderServiceBot
{
    public class BotScrapper : IBotScrapper, IDisposable
    {
        private UndetectedChromeDriver driver;

        public void Dispose()
        {
            driver.Close();
            driver.Quit();
            driver.Dispose();
        }

        public async void Init()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            driver = UndetectedChromeDriver.Create(driverExecutablePath: await new ChromeDriverInstaller().Auto());
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

            ProductData productData = new ProductData(title, category, price, ship);   

            return productData;
        }
    }

}
