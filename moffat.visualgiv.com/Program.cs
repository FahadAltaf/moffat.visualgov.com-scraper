
using HtmlAgilityPack;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;

List<string> links = new List<string>();

var extra = new PuppeteerExtra();

// Use stealth plugin
extra.Use(new StealthPlugin());
var browserFetcher = new BrowserFetcher();
browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision).Wait();
var browser = extra.LaunchAsync(new LaunchOptions()
{
    Headless = false,
    //  Args = new string[] { "--proxy-server=gate2.proxyfuel.com:2000" }
}).Result;
var page = browser.NewPageAsync().Result;

page.GoToAsync("http://moffat.visualgov.com/Results.aspx?Name=&RollType=All&TaxYear=2021&SearchType=1").Wait();
while (true)
{
    page.WaitForTimeoutAsync(2000).Wait();
    try
    {
        string content = page.GetContentAsync().Result;
        var entries = FetchEntries(content);
        if (entries.Count > 0)
            links.AddRange(entries);
        else
            Console.WriteLine("Unable to fetch entries");

        var b = (page.XPathAsync("//*[@id=\"ctl00_ContentPlaceHolder1_grdResults\"]/tbody/tr[28]/td/a[2]").Result)[0];
        b.ClickAsync().Wait();
    }
    catch { }
}

List<string> FetchEntries(string content)
{
    List<string> links = new List<string>();
    HtmlDocument doc = new HtmlDocument();
    doc.LoadHtml(content);

    var section = doc.DocumentNode.SelectSingleNode("/html/body/div/div[4]/div/form/table");
    if (section != null)
    {
        foreach (var row in section.ChildNodes.Skip(3).Where(x => x.Name == "tr"))
        {
            try
            {
                HtmlDocument docx = new HtmlDocument();
                docx.LoadHtml(row.InnerHtml);

                var linkNode = docx.DocumentNode.SelectSingleNode("/td[1]/a[1]");
                if (linkNode != null)
                {
                    var link = linkNode.Attributes.FirstOrDefault(x => x.Name == "href");
                    if (link != null)
                    {
                        if (!link.Value.Contains("javascript"))
                        {
                            links.Add("http://moffat.visualgov.com/" + link.Value);
                            File.WriteAllLines("Result.txt", links);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
    return links;
}








