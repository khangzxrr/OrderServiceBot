

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

            string hostname = $"http://{Environment.GetEnvironmentVariable("HOSTNAME")}:4444/wd/hub/";

            driver = new RemoteWebDriver(new Uri(hostname), chromeOptions);

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
        }

        public RabbitResponseProductData Scrape(RabbitRequestProductData rabbitProductRequest)
        {
            driver.Navigate().GoToUrl(rabbitProductRequest.productUrl.Trim());

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

            Thread.Sleep(500);

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

            Console.WriteLine("waiting for present shipCost element");


            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

            wait.Until((driver) =>
            {
                var tableSelector = By.CssSelector(".ux-table-section-with-hints--shippingTable");

                var tableElement = driver.FindElement(tableSelector);

                if (tableElement.Displayed)
                {
                    return tableElement;
                }

                return null;
            });

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

            var firstShipDateSelector = new List<By>
            {
                By.CssSelector($".ux-table-section-with-hints--shippingTable td:nth-child(5) > span:nth-child(2)"),
                By.CssSelector($".ux-table-section-with-hints--shippingTable td:nth-child(5) > .ux-textspans--BOLD:nth-child(4)"),
                By.CssSelector($"td.ux-table-section__cell:nth-child(4) > span:nth-child(2)")
            };


            int firstShipDateSelectorIndex;
            string firstShipDateString = "";
            for (firstShipDateSelectorIndex = 0; firstShipDateSelectorIndex < firstShipDateSelector.Count; firstShipDateSelectorIndex++)
            {
                var currentSelector = firstShipDateSelector[firstShipDateSelectorIndex];

                var firstShipTextElements = driver.FindElements(currentSelector);

                if (firstShipTextElements.Count > 0)
                {
                    firstShipDateString = firstShipTextElements.First().Text;
                    
                    if (firstShipDateString.Trim() == "") //continue to find text if string is empty
                    {
                        continue;
                    }

                    break;
                }
            }

            var estimatedShipsDay = 0;

            if (firstShipDateSelectorIndex == firstShipDateSelector.Count)
            {
                estimatedShipsDay = 15; //estimate if cannot find firstShip
            } else
            {
                var secondShipDateSelectors = new List<By>
            {
                By.CssSelector($".ux-table-section-with-hints--shippingTable td:nth-child(5) > span:nth-child(4)"),
                By.CssSelector($".ux-table-section-with-hints--shippingTable td:nth-child(5) > .ux-textspans--BOLD:nth-child(6)"),
                By.CssSelector($"td.ux-table-section__cell:nth-child(4) > span:nth-child(4)")
            };

                var secondShipDateElement = driver.FindElement(secondShipDateSelectors[firstShipDateSelectorIndex]);

                if (secondShipDateElement == null)
                {
                    throw new NoSuchElementException("no such element second ship date, selector: " + secondShipDateSelectors[firstShipDateSelectorIndex]);
                }

                var secondShipDateString = secondShipDateElement.Text;

                DateTime firstShipDate = DateTime.ParseExact(firstShipDateString, pattern, CultureInfo.InvariantCulture);
                DateTime secondShipDate = DateTime.ParseExact(secondShipDateString, pattern, CultureInfo.InvariantCulture);

                estimatedShipsDay = (int)(secondShipDate - firstShipDate).TotalDays;
            }

            var returnDaysSelector = By.CssSelector("table.ux-table-section:nth-child(2) > tbody:nth-child(2) > tr:nth-child(1) > td:nth-child(1) > span:nth-child(1)");
            var returnDaysTopSelector = By.CssSelector(".ux-labels-values--returns > div:nth-child(2) > div:nth-child(1) > div:nth-child(1) > span:nth-child(1)");
            var returnDaysString = driver.FindElement(returnDaysSelector).Text;

            if (returnDaysString == "")
            {
                returnDaysString = driver.FindElement(returnDaysTopSelector).Text;
            }

            returnDaysString = returnDaysString.Split(" ")[0];

            int returnDays;

            bool successParseReturnDays = int.TryParse(returnDaysString, out returnDays);

            if (!successParseReturnDays)
            {
                returnDays = -1;
            }
            

            Console.WriteLine($"======\nCatalog: {category}\nProduct: {title}\nPrice: {price}\nShip cost: {shipCost}\nShip Date: {estimatedShipsDay}\nReturn Days: {returnDays}");

            RabbitResponseProductData productData = new RabbitResponseProductData(rabbitProductRequest.userId, imageUrl, category, title, price, estimatedShipsDay, shipCost, rabbitProductRequest.productUrl, returnDays);   

            return productData;
        }
    }

}
