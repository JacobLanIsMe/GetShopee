using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjGetShopee
{
    public partial class GetShopee : Form
    {
        public GetShopee()
        {
            InitializeComponent();
        }
        List<string> bigTypeUrl = new List<string>();
        Random random = new Random();
        private async void btnGetProduct_Click(object sender, EventArgs e)
        {
            EdgeOptions edgeoptions = new EdgeOptions();
            edgeoptions.PageLoadStrategy = PageLoadStrategy.Normal;
            EdgeDriver driver = new EdgeDriver(edgeoptions);
            driver.Navigate().GoToUrl("https://shopee.tw/all_categories");
            var bigType = driver.FindElements(By.CssSelector(".category-grid"));
            foreach (var i in bigType)
            {
                string bigTypeName = i.Text;
                if (bigTypeName == "其他類別") continue;
                bigTypeUrl.Add(i.GetAttribute("href"));
            }
            foreach (var i in bigTypeUrl)
            {
                driver.Navigate().GoToUrl(i);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                string bigTypeName = driver.FindElement(By.CssSelector("a.shopee-category-list__main-category__link")).Text;
                List<string> smallTypeUrl = new List<string>();
                List<string> smallTypeName = new List<string>();
                driver.FindElement(By.CssSelector("div.shopee-category-list__toggle-btn")).Click();
                var smallType = driver.FindElements(By.CssSelector("a.shopee-category-list__sub-category"));
                foreach (var j in smallType)
                {
                    smallTypeUrl.Add(j.GetAttribute("href"));
                    smallTypeName.Add(j.Text);
                }
                for (int j = 0; j<smallTypeUrl.Count;j++)
                {
                    
                    driver.Navigate().GoToUrl(smallTypeUrl[j]);
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                    driver.FindElement(By.CssSelector("div.shopee-sort-by-options>div:nth-child(2)")).Click();
                    int productCount = random.Next(10, 30);
                    for (int k = 1; k <= productCount; k++)
                    {
                        driver.FindElement(By.XPath($"//*[@id='main']/div/div[2]/div/div/div[3]/div[2]/div/div[2]/div[{k}]/a")).Click();
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                        string productName = driver.FindElement(By.CssSelector("._2rQP1z>span")).Text;
                        List<byte[]> productPhotos = await GetProductPhotos(driver);

                        driver.Navigate().Back();
                    }
                    
                }
            }
            
            driver.Quit();
        }
        private async Task<List<byte[]>> GetProductPhotos(EdgeDriver driver)
        {
            driver.FindElement(By.CssSelector("div.xK9doz")).Click();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            var photos = driver.FindElements(By.CssSelector("div.xK9doz div._1OPdfl>div"));
            List<byte[]> productPhotos = new List<byte[]>();
            foreach (var i in photos)
            {
                string photoUrl = i.GetAttribute("style").Split('"')[1];
                HttpClient client = new HttpClient();
                byte[] photo = await client.GetByteArrayAsync(photoUrl);
                productPhotos.Add(photo);
            }
            return productPhotos;
        }
        private async void btnTest_ClickAsync(object sender, EventArgs e)
        {
            
        }
    }
}
