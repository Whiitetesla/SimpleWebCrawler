using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace SimpleWebCrawler
{
    class Program
    {

        static void Main(string[] args)
        {
            ISet<string> Urls = new HashSet<string>();
            ISet<string> products = new HashSet<string>();


            Urls.Add("https://www.elgiganten.dk/catalog/gaming/dk-gaming-stationaer-pc/gaming-stationar-pc?SearchParameter=%26%40QueryTerm%3D*%26ContextCategoryUUID%3DMV.sGQV5c6IAAAFaBK4MCIVM%26discontinued%3D0%26online%3D1%26ProductListPrice%3D7500%2B-%2B9999%26%40Sort.ViewCount%3D1&PageSize=12&ProductElementCount=&searchResultTab=Products&CategoryName=dk-gaming-stationaer-pc&CategoryDomainName=store-elgigantenDK-ProductCatalog#filter-sidebar");
            Urls.Add("https://www.komplett.dk/category/21660/gaming/gaming-pc/stationaer?nlevel=10431%C2%A721659%C2%A721660&pricegross.min=7500&pricegross.max=9999&pricegross.omin=4444&pricegross.omax=19990");

            foreach (var url in Urls)
            {
                foreach (var link in crawlSite(url))
                {
                    products.Add(link);
                }
            }

            foreach (var link in products)
            {
                if (link.Contains("lenovo-legion-t530"))
                {
                    var price = FindPrice(link);
                    Console.WriteLine(price);
                }
            }

            Console.ReadKey();
        }

        static ISet<string> crawlSite(string URL)
        {
            WebRequest myWebRequest;
            WebResponse myWebResponse;

            myWebRequest = WebRequest.Create(URL);
            myWebResponse = myWebRequest.GetResponse();

            Stream streamResponse = myWebResponse.GetResponseStream();

            StreamReader sreader = new StreamReader(streamResponse);
            var Rstring = sreader.ReadToEnd();

            var links = ExtractLinks(GetNewLinks(Rstring));
            
            streamResponse.Close();
            sreader.Close();
            myWebResponse.Close();

            return links;
        }

        static ISet<string> GetNewLinks(string content)
        {
            Regex regexLink = new Regex("(href=(?:'|\"))[^'\"]*?(?=(?:'|\"))");

            ISet<string> newLinks = new HashSet<string>();

            foreach (var match in regexLink.Matches(content))
            {
                if (!newLinks.Contains(match.ToString()))
                    newLinks.Add(match.ToString());
            }

            return newLinks;
        }
        
        static ISet<string> ExtractLinks(ISet<string> set)
        {
            ISet<string> newLinks = new HashSet<string>();

            foreach (var link in set)
            {
                var Links = link.Split('"');

                if (Links[1].Contains("/product/"))
                {
                    if (Links[1].StartsWith("/"))
                        Links[1] = "https://www.komplett.dk" + Links[1];
                    newLinks.Add(Links[1]);
                }
            }

            return newLinks;
        }


        static string FindPrice(string URL)
        {
            WebRequest myWebRequest;
            WebResponse myWebResponse;

            myWebRequest = WebRequest.Create(URL);
            myWebResponse = myWebRequest.GetResponse();

            Stream streamResponse = myWebResponse.GetResponseStream();

            StreamReader sreader = new StreamReader(streamResponse);
            var Rstring = sreader.ReadToEnd();

            var links = GetNewPrices(Rstring);

            streamResponse.Close();
            sreader.Close();
            myWebResponse.Close();

            return links;
        }


        static string GetNewPrices(string content)
        {
            string filter = "(<meta.itemprop=\"price\".content=(?:'|\"))[^'\"]*?(?=(?:'|\"))";
            Regex regexLink = new Regex(filter);

            ISet<string> newLinks = new HashSet<string>();

            foreach (var match in regexLink.Matches(content))
            {
                var Matchs = match.ToString().Split('"');
                return Matchs[Matchs.Length - 1];
            }

            return "";
        }
    }
}
