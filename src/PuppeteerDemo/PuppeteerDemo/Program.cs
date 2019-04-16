using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace PuppeteerDemo
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine($"开始执行!{DateTime.Now}");
            //从网络上下载浏览器便捷式安装包download-Win64-536395.zip到你本地，里面解压后是一个Chromium浏览器,这里需要等待一些时间
            //下载完第二次进来就不会再去下载了
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

            await Login();
            //await CookieLogin();
        }

        private static async Task CookieLogin()
        {
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
            Console.WriteLine($"页面创建完成!{DateTime.Now}");
            string cookiesStr = File.ReadAllText("cookies.txt");
            CookieParam[] cookies = JsonConvert.DeserializeObject<CookieParam[]>(cookiesStr);
            await page.SetCookieAsync(cookies);
            await page.GoToAsync("https://i.taobao.com/");
            await AliNoSlider(page);

            for (int i = 1; i <= 2; i++)
            {
                await page.GoToAsync($"https://pb89.tmall.com/search.htm?search=y&pageNo={i}");
                await AliNoSlider(page);
                //await page.WaitForNavigationAsync();
            }

            Console.ReadKey();
        }


        private static async Task Login()
        {
            //浏览器参数
            var launchOptions = new LaunchOptions
            {
                Headless = false,//是否无头浏览器
                //Args = new[] {"--proxy-server=socks5://127.0.0.1:1080"} //https://peter.sh/experiments/chromium-command-line-switches/
            };
            var browser = await Puppeteer.LaunchAsync(launchOptions);
            Console.WriteLine($"浏览器创建完成!{DateTime.Now}");
            //string username = "alitestforisv01";
            //string password = "Test1234,./";

            string username = "";
            string password = "";
            var page = await browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1366,
                Height = 768
            });
            Console.WriteLine($"页面创建完成!{DateTime.Now}");
            Console.WriteLine($"开始登入…{DateTime.Now}");
            await page.GoToAsync("https://login.taobao.com/member/login.jhtml?style=mini");

            await AliNoSlider(page);
            await page.TypeAsync("#TPL_username_1", username);
            await page.TypeAsync("#TPL_password_1", password);
            Thread.Sleep(200);

            //检测页面是否有滑块。原理是检测页面元素。
            var isSliderResult = await page.QuerySelectorAsync("#nocaptcha");
            var isSlider = await isSliderResult.IsIntersectingViewportAsync();
            bool sliderSuccess = true;
            if (isSlider)
            {
                //存在滑动验证码
                sliderSuccess = await MouseSlide(page);
                Console.WriteLine("滑动验证成功");
            }

            if (!sliderSuccess)
            {
                Console.Write("滑动验证失败，登入失败");
                Console.ReadKey();
            }
            await page.ClickAsync("#J_SubmitStatic");
            await page.WaitForNavigationAsync();
            if (!page.Url.Contains("i.taobao.com"))
            {
                Console.WriteLine("跳转到了登入页面");
                Thread.Sleep(1000);
                await page.GoToAsync("https://i.taobao.com/");
            }

            if (page.Url.Contains("login.taobao.com"))
            {
                Console.WriteLine($"登入失败!{DateTime.Now}");
            }
            else
            {
                //await page.ScreenshotAsync("1.jpg");
                await GetCookie(page);
                Console.WriteLine("cookie获取完成!");
                Console.WriteLine($"登入成功!{DateTime.Now}");
            }
            //await page.CloseAsync();
            //await browser.CloseAsync();
            Console.ReadKey();
        }

        /// <summary>
        /// 禁用淘宝检测浏览器
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private static async Task AliNoSlider(Page page)
        {
            await page.EvaluateFunctionAsync(AliNoSliderJs.Js1);
            await page.EvaluateFunctionAsync(AliNoSliderJs.Js2);
            await page.EvaluateFunctionAsync(AliNoSliderJs.Js3);
            await page.EvaluateFunctionAsync(AliNoSliderJs.Js4);
            await page.EvaluateFunctionAsync(AliNoSliderJs.Js5);

        }

        /// <summary>
        /// 获取cookies
        /// </summary>
        /// <param name="page">页面信息</param>
        /// <returns></returns>
        private static async Task<List<CookieModel>> GetCookie(Page page)
        {
            var cookies=await page.GetCookiesAsync();
            var cookiesStr = JsonConvert.SerializeObject(cookies);
            var cookieList = JsonConvert.DeserializeObject<List<CookieModel>>(cookiesStr);
            File.WriteAllText("cookies.txt", cookiesStr, Encoding.Default);
            return cookieList;
        }


        /// <summary>
        /// 滑块验证
        /// </summary>
        /// <param name="page">页面信息</param>
        /// <param name="tryTimes">重试次数</param>
        /// <returns></returns>
        private static async Task<bool> MouseSlide(Page page,int tryTimes=0)
        {
            try
            {
                //鼠标移动到滑块，按下，滑动到头（然后延时处理），松开按键,不同场景的验证码模块能名字不同。
                await page.HoverAsync("#nc_1_n1z");
                await page.Mouse.DownAsync();
                MoveOptions moveOptions = new MoveOptions {Steps = new Random().Next(10, 30)};
                await page.Mouse.MoveAsync(2000, 0, moveOptions);
                await page.Mouse.UpAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("验证失败:"+ex.Message);
            }
            //判断是否通过
            var pageContent = await page.GetContentAsync();
            HtmlDocument doc=new HtmlDocument();
            doc.LoadHtml(pageContent);
            var sliderText = doc.DocumentNode.SelectSingleNode("//span[@class='nc-lang-cnt']").InnerText;
            if (!sliderText.Contains("验证通过"))
            {
                if (tryTimes>3)
                {
                    return false;
                }
                await page.ClickAsync("span.nc-lang-cnt a");
                Thread.Sleep(200);
                tryTimes++;
                await MouseSlide(page,tryTimes);
            }
            return true;
        }
    }

    /// <summary>
    /// cookie模型
    /// </summary>
    public class CookieModel
    {
        public bool Secure { set; get; }
        public bool IsHttpOnly { set; get; }
        public string Name { set; get; }
        public string Value { set; get; }
        public string Domain { set; get; }
        public string Path { set; get; }
        public DateTime? Expiry { set; get; }
    }

    /// <summary>
    /// 替换淘宝在检测浏览时采集的一些参数
    /// 就是在浏览器运行的时候，始终让window.navigator.webdriver=false
    /// navigator是window对象的一个属性，同时修改plugins，languages，navigator 且让
    /// 以下为插入中间js，将淘宝会为了检测浏览器而调用的js修改其结果。
    /// </summary>
    public class AliNoSliderJs
    {
        /// <summary>
        /// Js1
        /// </summary>

        public static string Js1 = @"() =>{Object.defineProperties(navigator,{webdriver:{get: () => false}})}";

        /// <summary>
        /// Js2
        /// </summary>
        public static string Js2 = @"() => {window.navigator.chrome = {runtime: {},};}";

        /// <summary>
        /// Js3
        /// </summary>
        public static string Js3 = @"() =>{Object.defineProperty(navigator, 'languages', {get: () => ['en-US', 'en']});}";

        /// <summary>
        /// Js4
        /// </summary>
        public static string Js4 = @"() =>{Object.defineProperty(navigator, 'plugins', {get: () => [1, 2, 3, 4, 5,6],});}";

        /// <summary>
        /// Js5
        /// </summary>
        public static string Js5 = @"() => {alert(window.navigator.webdriver)}";
    }
}
