using ExcelDataReader;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Interactions;
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
        iSpanProjectEntities dbContext = new iSpanProjectEntities();
        List<string> bigTypeUrl = new List<string>();
        Random random = new Random();
        private void btnGetProduct_Click(object sender, EventArgs e)
        {
            EdgeOptions edgeoptions = new EdgeOptions();
            edgeoptions.PageLoadStrategy = PageLoadStrategy.Normal;
            EdgeDriver driver = new EdgeDriver(edgeoptions);
            driver.Manage().Window.Maximize();
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
                listBox1.Items.Add(bigTypeName);
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
                    listBox1.Items.Add($"    {smallTypeName[j]}");
                    driver.Navigate().GoToUrl(smallTypeUrl[j]);
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                    driver.FindElement(By.CssSelector("div.shopee-sort-by-options>div:nth-child(2)")).Click();
                    //var moveToEle = driver.FindElement(By.CssSelector("div.shopee-page-controller"));
                    //Actions action = new Actions(driver);
                    //action.MoveToElement(moveToEle).Perform();
                    //int productTotalCount = driver.FindElements(By.CssSelector("div.shopee-search-item-result__item>a")).ToList().Count;
                    //listBox1.Items.Add($"        {productTotalCount}");
                    int productCount = random.Next(5, 15+1);
                    for (int k = 1; k <= productCount; k++)
                    {
                        driver.FindElement(By.XPath($"//*[@id='main']/div/div[2]/div/div/div[3]/div[2]/div/div[2]/div[{k}]/a")).Click();
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                        string productName = driver.FindElement(By.CssSelector("._2rQP1z>span")).Text;
                        listBox1.Items.Add($"        {productName}");
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
            var q = dbContext.MemberAccounts.Select(i => i.MemPic);
            foreach (var i in q)
            {
                MemoryStream ms = new MemoryStream(i);
                PictureBox pb = new PictureBox();
                pb.Image = Image.FromStream(ms);
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                flowLayoutPanel1.Controls.Add(pb);
            }
        }

        private void btnClearDB_Click(object sender, EventArgs e)
        {
            var comments = dbContext.Comments.Select(i => i);
            foreach(var i in comments)
            {
                dbContext.Comments.Remove(i);
            }
            var follows = dbContext.Follows.Select(i => i);
            foreach (var i in follows)
            {
                dbContext.Follows.Remove(i);
            }
            var likes = dbContext.Likes.Select(i => i);
            foreach (var i in likes)
            {
                dbContext.Likes.Remove(i);
            }
            var officialCoupons = dbContext.OfficialCoupons.Select(i => i);
            foreach (var i in officialCoupons)
            {
                dbContext.OfficialCoupons.Remove(i);
            }
            var orderDetails = dbContext.OrderDetails.Select(i => i);
            foreach (var i in orderDetails)
            {
                dbContext.OrderDetails.Remove(i);
            }
            var orders = dbContext.Orders.Select(i => i);
            foreach (var i in orders)
            {
                dbContext.Orders.Remove(i);
            }
            var productDetails = dbContext.ProductDetails.Select(i => i);
            foreach (var i in productDetails)
            {
                dbContext.ProductDetails.Remove(i);
            }
            var productPics = dbContext.ProductPics.Select(i => i);
            foreach (var i in productPics)
            {
                dbContext.ProductPics.Remove(i);
            }
            var shipperToProducts = dbContext.ShipperToProducts.Select(i => i);
            foreach (var i in shipperToProducts)
            {
                dbContext.ShipperToProducts.Remove(i);
            }
            var products = dbContext.Products.Select(i => i);
            foreach (var i in products)
            {
                dbContext.Products.Remove(i);
            }
            var smallTypes = dbContext.SmallTypes.Select(i => i);
            foreach (var i in smallTypes)
            {
                dbContext.SmallTypes.Remove(i);
            }
            var bigTypes = dbContext.BigTypes.Select(i => i);
            foreach (var i in bigTypes)
            {
                dbContext.BigTypes.Remove(i);
            }
            var memberAccounts = dbContext.MemberAccounts.Select(i => i);
            foreach (var i in memberAccounts)
            {
                dbContext.MemberAccounts.Remove(i);
            }
            dbContext.SaveChanges();
            MessageBox.Show("清除完成");

        }

        private async void btnAddMember_Click(object sender, EventArgs e)
        {
            FileStream fs = File.Open("memberInfo.xlsx", FileMode.Open, FileAccess.Read);
            var reader = ExcelReaderFactory.CreateOpenXmlReader(fs);

            DataSet ds = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });
            DataTable dt = ds.Tables["員工資料1"];
            
            Random random = new Random();
            DateTime startDate = new DateTime(1950, 1, 1);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string countryName = dt.Rows[i]["Country"].ToString();
                int countryID = dbContext.CountryLists.Where(k => k.CountryName == countryName).Select(k => k.CountryID).FirstOrDefault();
                string regionName = dt.Rows[i]["Region"].ToString();
                int regionID = dbContext.RegionLists.Where(k => k.RegionName == regionName && k.CountryID == countryID).Select(k => k.RegionID).FirstOrDefault();
                string phone = "09" + random.Next(1, 99999999).ToString("00000000");
                string email = dt.Rows[i]["登入帳號"].ToString();
                string address = dt.Rows[i]["地址"].ToString();
                string name = dt.Rows[i]["員工姓名"].ToString();
                int dateRange = (DateTime.Today - startDate).Days;
                DateTime birthday = startDate.AddDays(random.Next(dateRange));
                string bio = dt.Rows[i]["所屬區域"].ToString() + dt.Rows[i]["部門"].ToString() + dt.Rows[i]["職稱"].ToString();
                HttpClient client = new HttpClient();
                string photoUrl = dt.Rows[i]["照片"].ToString();
                byte[] photo = await client.GetByteArrayAsync(photoUrl);
                MemberAccount member = new MemberAccount
                {
                    MemberAcc = $"Acc{i + 1}",
                    MemberPw = $"Pw{i + 1}",
                    TWorNOT = true,
                    RegionID = regionID,
                    Phone = phone,
                    Email = email,
                    Address = address,
                    Name = name,
                    Birthday = birthday,
                    Bio = bio,
                    MemPic = photo,
                    MemStatusID = 1
                };
                dbContext.MemberAccounts.Add(member);
                dbContext.SaveChanges();
            }
            MessageBox.Show("新增會員完成");
            dataGridView1.DataSource = ds.Tables["員工資料1"];

            
           
        }

       
    }
}
