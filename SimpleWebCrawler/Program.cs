using AngleSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace SimpleWebCrawler
{
    class Program
    {
        //todo
        //https://anglesharp.github.io/

        static void Main(string[] args)
        {
            AngleSharpTest();

            Console.ReadLine();

            ISet<string> Urls = new HashSet<string>();
            SortedSet<string> products = new SortedSet<string>();


            Urls.Add("https://www.elgiganten.dk/catalog/gaming/dk-gaming-stationaer-pc/gaming-stationar-pc?SearchParameter=%26%40QueryTerm%3D*%26ContextCategoryUUID%3DMV.sGQV5c6IAAAFaBK4MCIVM%26discontinued%3D0%26online%3D1%26ProductListPrice%3D4000%2B-%2B19999%26%40Sort.ViewCount%3D1&PageSize=12&ProductElementCount=&searchResultTab=Products&CategoryName=dk-gaming-stationaer-pc&CategoryDomainName=store-elgigantenDK-ProductCatalog#filter-sidebar");
            Urls.Add("https://www.komplett.dk/category/21660/gaming/gaming-pc/stationaer?nlevel=10431%C2%A721659%C2%A721660&pricegross.min=4000&pricegross.max=19999&pricegross.omin=4000&pricegross.omax=19999");

            foreach (var url in Urls)
            {
                foreach (var link in crawlSite(url))
                {
                    products.Add(link);
                }
            }


            foreach (var link in products)
            {
                //if (link.Contains("lenovo-legion-t530"))
                //{
                var price = FindPrice(link);
                Console.WriteLine($"price: {price}, procuctLink: {link} ");
                //}
            }

            Console.ReadKey();
        }

        private static async System.Threading.Tasks.Task AngleSharpTest()
        {
            //Use the default configuration for AngleSharp
            var config = Configuration.Default;

            //Create a new context for evaluating webpages with the given config
            var context = BrowsingContext.New(config);

            //Source to be parsed
            var source = "<h1>Some example source</h1><p>This is a paragraph element";

            //Create a virtual request to specify the document to load (here from our fixed string)
            var document = await context.OpenAsync(req => req.Content(source));

            //Do something with document like the following
            Console.WriteLine("Serializing the (original) document:");
            Console.WriteLine(document.DocumentElement.OuterHtml);

            var p = document.CreateElement("p");
            p.TextContent = "This is another paragraph.";

            Console.WriteLine("Inserting another element in the body ...");
            document.Body.AppendChild(p);

            Console.WriteLine("Serializing the document again:");
            Console.WriteLine(document.DocumentElement.OuterHtml);
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
