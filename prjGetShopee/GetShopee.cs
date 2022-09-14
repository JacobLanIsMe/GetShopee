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
using System.Threading;
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
        iSpanProjectEntities1 dbContext = new iSpanProjectEntities1();
        List<string> bigTypeUrl = new List<string>();
        Random random = new Random();
        private async void btnGetProduct_Click(object sender, EventArgs e)
        {
            EdgeOptions edgeoptions = new EdgeOptions();
            edgeoptions.PageLoadStrategy = PageLoadStrategy.Normal;
            EdgeDriver driver = new EdgeDriver(edgeoptions);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("https://shopee.tw/all_categories");
            var memberIDs = dbContext.MemberAccounts.Select(a => a.MemberID).ToList();
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
                BigType bigtype = new BigType
                {
                    BigTypeName = bigTypeName
                };
                dbContext.BigTypes.Add(bigtype);
                dbContext.SaveChanges();
                int bigTypeID = dbContext.BigTypes.Where(a => a.BigTypeName == bigTypeName).Select(a => a.BigTypeID).FirstOrDefault();
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
                    string smalltypename = smallTypeName[j];
                    SmallType smalltype = new SmallType
                    {
                        SmallTypeName = smalltypename,
                        BigTypeID = bigTypeID
                    };
                    dbContext.SmallTypes.Add(smalltype);
                    dbContext.SaveChanges();
                    int smallTypeID = dbContext.SmallTypes.Where(a => a.SmallTypeName == smalltypename && a.BigTypeID == bigTypeID).Select(a => a.SmallTypeID).FirstOrDefault();
                    listBox1.Items.Add($"    {smalltypename}");
                    driver.Navigate().GoToUrl(smallTypeUrl[j]);
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                    driver.FindElement(By.CssSelector("div.shopee-sort-by-options>div:nth-child(2)")).Click();
                    var moveToEle = driver.FindElement(By.XPath("//*[@id='main']/div/div[2]/div/div/div[3]/div[2]/div/div[3]/div"));
                    Actions action = new Actions(driver);
                    action.MoveToElement(moveToEle).Build().Perform();
                    var producturls = driver.FindElements(By.CssSelector(".shopee-search-item-result__item>a"));
                    List<string> productUrls = new List<string>();
                    foreach (var p in producturls)
                    {
                        string productUrl = p.GetAttribute("href");
                        productUrls.Add(productUrl);
                    }
                    int productCount = random.Next(10, productUrls.Count+1);
                    for (int p = 0; p < productCount; p++)
                    {
                        driver.Navigate().GoToUrl(productUrls[p]);
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                        string productName = driver.FindElement(By.CssSelector("._2rQP1z>span")).Text;
                        int memberID = random.Next(0, memberIDs.Count);
                        memberID = memberIDs[memberID];
                        int regionID = dbContext.MemberAccounts.Where(a => a.MemberID == memberID).Select(a => a.RegionID).FirstOrDefault();
                        var descriptions = driver.FindElements(By.CssSelector("p._2jrvqA"));
                        string description = "";
                        foreach (var d in descriptions)
                        {
                            description += d.Text;
                        }
                        label1.Text = description;
                        Product product = new Product
                        {
                            ProductName = productName,
                            SmallTypeID = smallTypeID,
                            MemberID = memberID,
                            RegionID = regionID,
                            AdFee = random.Next(0,10000),
                            Description = description,
                            ProductStatusID = 0
                        };
                        dbContext.Products.Add(product);
                        try
                        {
                            dbContext.SaveChanges();
                        }
                        catch(Exception ex)
                        {
                            continue;
                        }
                        int productID = dbContext.Products.Where(a => a.ProductName == productName && a.MemberID == memberID && a.SmallTypeID == smallTypeID).Select(a => a.ProductID).FirstOrDefault();
                        listBox1.Items.Add($"        {productName}");
                        var variations = driver.FindElements(By.CssSelector("button.product-variation"));
                        if (variations.Count > 0)
                        {
                            foreach (var k in variations)
                            {
                                if (Convert.ToBoolean(k.GetAttribute("aria-disabled"))) continue;
                                try
                                {
                                    //Actions actions = new Actions(driver);
                                    //actions.MoveToElement(k).Click().Build().Perform();
                                    k.Click();
                                    Thread.Sleep(350);
                                    string photoUrl = driver.FindElement(By.CssSelector("div._1OPdfl>div")).GetAttribute("style").Split('"')[1];
                                    string price = driver.FindElement(By.CssSelector("div._2Shl1j")).Text;
                                    string style = k.Text;
                                    int quantity = random.Next(1, 1000);
                                    HttpClient client = new HttpClient();
                                    byte[] photo = await client.GetByteArrayAsync(photoUrl);

                                    ProductDetail productDetail = new ProductDetail
                                    {
                                        ProductID = productID,
                                        Style = style,
                                        Quantity = quantity,
                                        UnitPrice = Convert.ToDecimal(price.Replace("$", "").Replace(",", "")),
                                        Pic = photo
                                    };
                                    dbContext.ProductDetails.Add(productDetail);
                                    dbContext.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            string price = driver.FindElement(By.CssSelector("div._2Shl1j")).Text;
                            int quantity = random.Next(1, 1000);
                            string photoUrl = driver.FindElement(By.CssSelector("div._1OPdfl>div")).GetAttribute("style").Split('"')[1];
                            HttpClient client = new HttpClient();
                            byte[] photo = await client.GetByteArrayAsync(photoUrl);
                            ProductDetail productDetail = new ProductDetail
                            {
                                ProductID = productID,
                                Style = "無特殊樣式",
                                Quantity = quantity,
                                UnitPrice = Convert.ToDecimal(price.Replace("$", "").Replace(",", "")),
                                Pic = photo
                            };
                            dbContext.ProductDetails.Add(productDetail);
                            dbContext.SaveChanges();
                        }
                        

                        
                    }

                }
            }
            driver.Quit();
            MessageBox.Show("商品新增完成");
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
        private void btnTest_ClickAsync(object sender, EventArgs e)
        {
            EdgeDriver driver = new EdgeDriver();
            driver.Navigate().GoToUrl("https://shopee.tw/Panasonic-CF-SZ6-%E6%9D%BE%E4%B8%8B-%E6%97%A5%E6%9C%AC%E8%A3%BD-%E7%AD%86%E8%A8%98%E5%9E%8B%E9%9B%BB%E8%85%A6-%E8%B6%85%E8%BC%95-i.3111013.21836770153?sp_atk=80182044-e1c6-488b-b8c9-11539f66a7a6&xptdk=80182044-e1c6-488b-b8c9-11539f66a7a6");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            bool boo = driver.FindElement(By.CssSelector("button.product-variation")).Displayed;
            if (boo)
            {
                label1.Text = "a";
            }
            else
            {
                label1.Text = "b";
            }
        
        }

        private void btnClearDB_Click(object sender, EventArgs e)
        {
            var commentPics = dbContext.CommentPics.Select(i => i);
            foreach (var i in commentPics)
            {
                dbContext.CommentPics.Remove(i);
            }
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
            //var memberAccounts = dbContext.MemberAccounts.Select(i => i);
            //foreach (var i in memberAccounts)
            //{
            //    dbContext.MemberAccounts.Remove(i);
            //}
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
                byte[] photo;
                try
                {
                    photo = await client.GetByteArrayAsync(photoUrl);
                }
                catch(Exception ex)
                {
                    int photoNumber = random.Next(1, 14);
                    MemoryStream ms = new MemoryStream();
                    Image image = Image.FromFile($"../../images/avatar{photoNumber}.png");
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    photo = ms.GetBuffer();
                }
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
