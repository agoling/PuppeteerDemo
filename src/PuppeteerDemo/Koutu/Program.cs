using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace Koutu
{
    internal class Program
    {
        [DllImport("AutoItX3_x64.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static public extern int AU3_ControlFocus([MarshalAs(UnmanagedType.LPWStr)] string Title
                , [MarshalAs(UnmanagedType.LPWStr)] string Text, [MarshalAs(UnmanagedType.LPWStr)] string Control);

        [DllImport("AutoItX3_x64.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static public extern int AU3_WinWait([MarshalAs(UnmanagedType.LPWStr)]string Title, [MarshalAs(UnmanagedType.LPWStr)] string Text, int Timeout);

        [DllImport("AutoItX3_x64.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static public extern int AU3_ControlSetText([MarshalAs(UnmanagedType.LPWStr)] string Title
            , [MarshalAs(UnmanagedType.LPWStr)] string Text, [MarshalAs(UnmanagedType.LPWStr)] string Control
            , [MarshalAs(UnmanagedType.LPWStr)] string ControlText);

        [DllImport("AutoItX3_x64.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static public extern int AU3_ControlClick([MarshalAs(UnmanagedType.LPWStr)] string Title
                , [MarshalAs(UnmanagedType.LPWStr)] string Text, [MarshalAs(UnmanagedType.LPWStr)] string Control
                , [MarshalAs(UnmanagedType.LPWStr)] string Button, int NumClicks, int X, int Y);
        private static async Task Main(string[] args)
        {
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
            await page.GoToAsync("https://www.remove.bg/");
            await page.ClickAsync(".select-photo-url-btn");


            page.Dialog += async (sender, e) =>
            {
                if (e.Dialog.DialogType == DialogType.Confirm)
                {
                    await e.Dialog.Accept("https://o.remove.bg/uploads/14f9f1a1-a21f-4346-a2ad-9cab0785fa4d/u_4061252405_819153609_fm_173_app_25_f_JPEG.jpeg");
                    await e.Dialog.Dismiss();
                }

            };

            //page.Popup += async (sender, e) =>
            //{
            //    Console.WriteLine("456");
            //};



            // await page.WaitForTimeoutAsync(5);
            //await page.ClickAsync(".select-photo-file-btn");
            //Thread.Sleep(1000);
            //AU3_ControlFocus("打开","","Edit1");
            //AU3_WinWait("[CLASS:#32770]", "", 10);
            //AU3_ControlSetText("打开", "", "Edit1", "D:\\1.png");

            //AU3_ControlClick("打开", "", "Button1", "left", 1,1,1);
            //await page.EvaluateExpressionAsync("alert('yo');");

            //var element=await page.QuerySelectorAsync("input [type =\"file\"]");
            //await element.UploadFileAsync("D:\\1.png");

            //await page.CloseAsync();
            //await browser.CloseAsync();
            Console.ReadKey();
        }
    }
}
