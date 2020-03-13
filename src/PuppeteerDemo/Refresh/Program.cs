using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NSoup;
using NSoup.Nodes;
using PuppeteerSharp;

namespace Refresh
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            Demo d=new Demo();
            Console.WriteLine($"开始执行!{DateTime.Now}");
            //从网络上下载浏览器便捷式安装包download-Win64-536395.zip到你本地，里面解压后是一个Chromium浏览器,这里需要等待一些时间
            //下载完第二次进来就不会再去下载了
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            //浏览器参数
            var launchOptions = new LaunchOptions
            {
                Headless = false,//是否无头浏览器
            };
            var browser = await Puppeteer.LaunchAsync(launchOptions);
            Console.WriteLine($"浏览器创建完成!{DateTime.Now}");
            var page = await browser.NewPageAsync();
            //await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
            //Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3617.0 Safari/537.36
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1366,
                Height = 768
            });
            await page.GoToAsync("https://oapi.dingtalk.com/connect/qrconnect?appid=dingoakpa9drsyzymtwxgw&response_type=code&scope=snsapi_login&state=web-occ&redirect_uri=http://occ.xxxxxx.com/manage/DingTalkLogin");

            //while (true)
            //{
            //    Thread.Sleep(3000);
            //    var htmlString = page.GetContentAsync().Result;
            //    Document htmlDoc = NSoupClient.Parse(htmlString);
            //    var text = htmlDoc.Body.GetElementById("times").Text();
            //    Console.WriteLine(text);
            //    await page.ReloadAsync();
            //}

            page.Response += async (sender, e) =>
            {
                Console.WriteLine(e.Response.Ok);
                var response = await e.Response.TextAsync();
                Console.WriteLine(response);
            };

            Console.ReadKey();
        }

        public class  Demo
        {
            public Enum Code { set; get; } = default;

            public int Count { set; get; } = 1;

            public string Str { set; get; }

        }
    }
}
