using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderServiceBot
{
    public interface IBotScrapper
    {
        public ProductData Scrape(string url);
        public void Init();
    }
}
