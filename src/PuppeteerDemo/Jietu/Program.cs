using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace Jietu
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            Stopwatch sw=new Stopwatch();
            sw.Start();
            //从网络上下载浏览器便捷式安装包download-Win64-536395.zip到你本地，里面解压后是一个Chromium浏览器,这里需要等待一些时间
            //下载完第二次进来就不会再去下载了
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            //浏览器参数
            var launchOptions = new LaunchOptions
            {
                Headless = false,//是否无头浏览器
            };
            var browser = await Puppeteer.LaunchAsync(launchOptions);
            var page = await browser.NewPageAsync();
            //await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
            //Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3617.0 Safari/537.36
            ViewPortOptions viewPortOptions = new ViewPortOptions();
            viewPortOptions.Width = 800;
            viewPortOptions.Height = 1200;
            await page.SetViewportAsync(viewPortOptions);

            var navigationOption = new NavigationOptions
            {
                WaitUntil = new[]
                {
                    WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle2
                }
            };

            //await page.GoToAsync("http://192.168.0.107:98/static/index.html#/preview?recordid=059d67fea03a4c3687bc49ac04669968&t=6deb2042ec114ac291fadb2514182352&generate=true", navigationOption);
            await page.GoToAsync("https://zhuangxiu.xxxxxx.com/#/preview?recordid=1095a3231f704b419b8b0676fa8d147e&t=e1859920de894de394c577f802f67167&generate=true", navigationOption);
            

            var o = new ScreenshotOptions { Type = ScreenshotType.Png, FullPage = true };
            var bytes = await page.ScreenshotDataAsync(o);

            var modules = await page.QuerySelectorAllAsync(".screenshot-box");
            var dic=new Dictionary<int, Clip>();
            for (var i = 0; i < modules.Length; i++)
            {
                var module = modules[i];
                var box = await module.BoundingBoxAsync();
                var clip = new Clip { Width = box.Width, Height = box.Height, X = box.X, Y = box.Y };
                dic.Add(i, clip);
            }

            Parallel.For(0, dic.Count, n =>
            {
                //计算出元素上、下、左、右 位置
                var left = (int)dic[n].X;
                var top = (int)dic[n].Y;
                var right = (int)dic[n].Width;
                var bottom = (int)dic[n].Height;

                //定义截取矩形
                var cropArea = new Rectangle(left, top, right, bottom); //要截取的区域大小
                //加载图片
                MemoryStream ms = new MemoryStream(bytes);
                var screenImage = (Bitmap)Image.FromStream(ms);
                //进行裁剪
                var bmpCrop = screenImage.Clone(cropArea, screenImage.PixelFormat);
                //保存成新文件
                bmpCrop.Save($"{n}.png");
                //释放对象
                screenImage.Dispose();

            });
          
            //await page.ScreenshotAsync($"{j}.png", options);
            //for (var n = 0; n < dic.Count; n++)
            //{

            //    //计算出元素上、下、左、右 位置
            //    var left = (int)dic[n].X;
            //    var top = (int)dic[n].Y;
            //    var right = (int)dic[n].Width;
            //    var bottom = (int)dic[n].Height;


            //    //var codeImagePath = imageDir + fileName + "_crop" + fileSuffix;
            //    //定义截取矩形
            //    var cropArea = new Rectangle(left, top, right, bottom); //要截取的区域大小
            //    //加载图片
            //    MemoryStream ms=new MemoryStream(bytes);
            //    var screenImage = (Bitmap)Image.FromStream(ms);
            //    //进行裁剪
            //    var bmpCrop = screenImage.Clone(cropArea, screenImage.PixelFormat);
            //    //保存成新文件
            //    bmpCrop.Save($"{n}.png");
            //    //释放对象
            //    screenImage.Dispose();


            //}


            //for (var j = 0; j < dic.Count; j++)
            //{
            //    var options = new ScreenshotOptions { Type = ScreenshotType.Png, Clip = dic[j] };
            //    //await page.ScreenshotDataAsync(options);
            //    await page.ScreenshotAsync($"{j}.png", options);
            //}
            sw.Stop();
            await page.CloseAsync();
            await browser.CloseAsync();
            Console.WriteLine($"success!{sw.Elapsed.TotalSeconds}");
            Console.ReadKey();
        }
    }
}
