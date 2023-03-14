﻿using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using SeleniumUndetectedChromeDriver;
using OpenQA.Selenium;

new DriverManager().SetUpDriver(new ChromeConfig());

var driver = UndetectedChromeDriver.Create(driverExecutablePath: await new ChromeDriverInstaller().Auto());

string[] urls = new string[] {
    "https://www.ebay.com/itm/115458094959?hash=item1ae1d6b76f:g:rz8AAOSwxe1jGuEv&amdata=enc%3AAQAHAAAA4HQthA%2BukhWSA3h7Uo8GZWwH9iAXNLwMfV91D1gtHv2cuYQOZPXaF2F9roFydnJbf%2BhL3QyuMiQd5hcK77kDq2sWKZ4KLxz0onJYmI9e2prNb1gvH4zhwQETh%2FAsGEpc394RvovezxVM7eVjkv1fQOVtLqXxHIWzC77QX4S8d%2Bm3grjyIZTu%2F0io4atHfOO%2B%2BDc5E0CkEVF%2F4owDu4xrRytAgBqvz91eODPLNkqidX61ACKpHDoREraygeRv%2BZYjf4KyNZOQbi8fNKYuaLeXctYUWU1PTrOcTcEX2rgzvXnj%7Ctkp%3ABFBM7vHS_dth",
    "https://www.ebay.com/itm/143588642102?_trkparms=amclksrc%3DITM%26aid%3D1110006%26algo%3DHOMESPLICE.SIM%26ao%3D1%26asc%3D20201210111314%26meid%3D5faf5af3566f4c0fae2f7b263872dace%26pid%3D101195%26rk%3D4%26rkt%3D12%26sd%3D115458094959%26itm%3D143588642102%26pmt%3D1%26noa%3D0%26pg%3D2047675%26algv%3DSimplAMLv11WebTrimmedV3MskuWithLambda85KnnRecallV1V4V6ItemNrtInQueryAndCassiniVisualRankerAndBertRecall%26brand%3DMotto&_trksid=p2047675.c101195.m1851&amdata=cksum%3A1435886421025faf5af3566f4c0fae2f7b263872dace%7Cenc%3AAQAHAAABUBEC%252B8HimpTF6XPanG8lCeF3G6jifrewDlLoKJMPz0RnJH8s%252BUBlIo2cSQSdgSGvSVvCKC6zF4X6tOnQIr392G0gldH8QjhGQ2fmdEokLVFt29UKj%252FEph29NUO0KxqhEb84gZhbKWlWfbravBJF8o5LCHTk88OlYmtLg3XYCsBHFKtGgZIe%252BCzuqQFo1PXSM0LtMAWKXsDceLyg66gyrK1ooccr%252FQaLEmHu9BpozfylMhOnJUB608YD0g5DGXtWOgEubW62jLq8%252BVONswamQaFnPxVe104fFfjJiqsgmOs1e9iC14Aj1RfSWW3NKjqEmqMPcKZ1aO032EAMlXCsjwtriIIXtgKjQbA4lOc3sCuM4m9YX8JWj5WdfCCMxrakZ3%252B6Vz9C1A90lAKxLjByLj8XrqOW7PPMqD3jwCBUscIYJfnoB3oXq8KIgOo676C3yng%253D%253D%7Campid%3APL_CLK%7Cclp%3A2047675",
    "https://www.ebay.com/itm/354626699273?epid=12055490631&hash=item5291660009:g:VwAAAOSwjIJkA3rk",
    "https://www.ebay.com/itm/314184611122?hash=item4926dcb532:g:izwAAOSwPVljRiCE&amdata=enc%3AAQAHAAAA4NGRskQaF73PPaot8OgG8Xb837%2FPT%2BjI9UprTzUvX1GX3yIjooJI8z4%2FDtusjoj4Bea90vpPUoKrIDr2LYYl60bCKc53Oa6b1ltU7%2BLtz7BgL8UiRe126CmX0yE%2F5Bj1E3mykpWq9eWmJnDNCRy9qPgWhG%2FaLua654a1%2FyLRp5ATDPS8c2REf7j5yp6lfne3Gwo8CNlFamjwcnfbFS6IPL3EtWqGbol3yfpL%2FMMWmUSE%2FTNjMlrmLTW7A5DcJnAIG0Bs%2BkvQEEielwJ%2FyVnd4xQvxNNtThQ0DPbANXoVQSYB%7Ctkp%3ABFBMpK3b_dth"
};


foreach (var url in urls)
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

}
driver.Close();
driver.Quit();
driver.Dispose();
