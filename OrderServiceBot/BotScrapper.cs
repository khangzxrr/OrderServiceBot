

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using OrderService.Core.RabbitMqDto;
using System.Text.RegularExpressions;

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
            Thread.Sleep(3000); //waiting for chrome worker to connect selenium hub

            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.PageLoadStrategy = PageLoadStrategy.Eager;

            chromeOptions.AddArgument("--no-sandbox");

            string hostname = $"http://{Environment.GetEnvironmentVariable("HOSTNAME")}:4444";

            driver = new RemoteWebDriver(new Uri(hostname), chromeOptions);
        }

        public RabbitResponseProductData Scrape(RabbitRequestProductData rabbitProductRequest)
        {
            driver.Navigate().GoToUrl(rabbitProductRequest.productUrl);

            var titleSelector = By.CssSelector("#LeftSummaryPanel h1 > span");
            var title = driver.FindElement(titleSelector).Text;

            var categorySelector = By.CssSelector("ul > li > a > span");
            var category = driver.FindElement(categorySelector).Text;

            var priceSelector = By.CssSelector("span[itemprop=\"price\"]");
            var priceString = driver.FindElement(priceSelector).Text;

            Regex priceRegex = new Regex("-?\\d+(?:\\.\\d+)?", RegexOptions.IgnoreCase);

            var price = double.Parse(priceRegex.Match(priceString).Value);


            var imageSelector = By.CssSelector(".ux-image-carousel > div > img");
            var imageUrl = driver.FindElement(imageSelector).GetAttribute("src");

            var tabShipSelector = By.CssSelector("#TABS_SPR");
            driver.FindElement(tabShipSelector).Click();

            Thread.Sleep(200);

            var countrySelector = By.CssSelector("#shCountry");
            var countrySelectElement = driver.FindElement(countrySelector);
            var countrySelectorElement = new SelectElement(countrySelectElement);
            countrySelectorElement.SelectByValue("1");

            var zipCodeSelector = By.CssSelector("#shZipCode");
            var zipCodeElements = driver.FindElements(zipCodeSelector);
            if (zipCodeElements.Count > 0)
            {
                zipCodeElements[0].SendKeys("34787");
            }

            var submitShipSelector = By.CssSelector(".ux-shipping-calculator__getRates > button");
            driver.FindElement(submitShipSelector).Click();

            Thread.Sleep(200);

           var shipCostSelector = By.CssSelector(".ux-table-section-with-hints--shippingTable > tbody:nth-child(2) > tr:nth-child(1) > td:nth-child(1) > span:nth-child(1)");
            var shipCostString = driver.FindElement(shipCostSelector).Text;

            double shipCost = 0;

            if (!shipCostString.Contains("Free"))
            {
                //US $14.99 (approx 348,604.65 VND)
                var splitBySpaceShipCost = shipCostString.Split(" ")[1].Replace("$", "").Trim();
                shipCost = double.Parse(splitBySpaceShipCost);
            }


            string pattern = "ddd, MMM d";

            var estimateFirstShipDateSelector = By.CssSelector($".ux-table-section-with-hints--shippingTable td:nth-child(5) > span:nth-child(2)");
            var estimateFirstShipDateSelector2 = By.CssSelector($".ux-table-section-with-hints--shippingTable td:nth-child(5) > .ux-textspans--BOLD:nth-child(4)");

            var firstShipDateString = driver.FindElement(estimateFirstShipDateSelector).Text;
            var firstShipUsingSecondSelector = false;

            if (firstShipDateString.Trim() == "") { 
                firstShipUsingSecondSelector = true;
                firstShipDateString = driver.FindElement(estimateFirstShipDateSelector2).Text;
            }


            var estimateSecondShipDateSelector = By.CssSelector($".ux-table-section-with-hints--shippingTable td:nth-child(5) > span:nth-child(4)");
            var estimateSecondShipDateSelector2 = By.CssSelector($".ux-table-section-with-hints--shippingTable td:nth-child(5) > .ux-textspans--BOLD:nth-child(6)");
            var secondShipDateString = driver.FindElement(estimateSecondShipDateSelector).Text;

            if (secondShipDateString.Trim() == "" || firstShipUsingSecondSelector)
            {
                secondShipDateString = driver.FindElement(estimateSecondShipDateSelector2).Text;    
            }

            DateTime firstShipDate = DateTime.ParseExact(firstShipDateString, pattern, CultureInfo.InvariantCulture);
            DateTime secondShipDate = DateTime.ParseExact(secondShipDateString, pattern, CultureInfo.InvariantCulture);

            var estimatedShipsDay = (int)(secondShipDate - firstShipDate).TotalDays;

            var returnDaysSelector = By.CssSelector("table.ux-table-section:nth-child(2) > tbody:nth-child(2) > tr:nth-child(1) > td:nth-child(1) > span:nth-child(1)");
            var returnDaysTopSelector = By.CssSelector(".ux-labels-values--returns > div:nth-child(2) > div:nth-child(1) > div:nth-child(1) > span:nth-child(1)");
            var returnDaysString = driver.FindElement(returnDaysSelector).Text;

            if (returnDaysString == "")
            {
                returnDaysString = driver.FindElement(returnDaysTopSelector).Text;
            }

            var returnDays = -1;

            if (!returnDaysString.Contains("Seller does not accept returns"))
            {
                returnDaysString = returnDaysString.Split(" ")[0];
                returnDays = int.Parse(returnDaysString);
            }

            Console.WriteLine($"======\nCatalog: {category}\nProduct: {title}\nPrice: {price}\nShip cost: {shipCost}\nShip Date: {estimatedShipsDay}\nReturn Days: {returnDays}");

            RabbitResponseProductData productData = new RabbitResponseProductData(rabbitProductRequest.userId, imageUrl, category, title, price, estimatedShipsDay, shipCost, rabbitProductRequest.productUrl, returnDays);   

            return productData;
        }
    }

}
