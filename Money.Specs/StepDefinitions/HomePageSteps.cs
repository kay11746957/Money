using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using NUnit.Framework;

namespace Money.Specs.StepDefinitions
{
    [Binding]
    public class HomePageSteps
    {
        private IWebDriver _driver = null!;

        [Given(@"我開啟瀏覽器進入首頁 ""(.*)""")]
        public void Given我開啟瀏覽器進入首頁(string path)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            _driver = new ChromeDriver(options);
            _driver.Navigate().GoToUrl($"http://localhost:5050{path}");
        }

        [Then(@"我應該看到 Hero 區塊包含:")]
        public void Then我應該看到Hero區塊包含(Table table)
        {
            var heroSection = _driver.FindElement(By.CssSelector(".hero-section"));
            
            foreach (var row in table.Rows)
            {
                var element = row["元素"];
                var expectedContent = row["內容"];
                
                switch (element)
                {
                    case "主標題":
                        var mainTitle = heroSection.FindElement(By.CssSelector(".hero-title"));
                        Assert.That(mainTitle.Text, Does.Contain(expectedContent), 
                            $"主標題應該包含 '{expectedContent}'，但實際是 '{mainTitle.Text}'");
                        break;
                    case "副標題":
                        var subTitle = heroSection.FindElement(By.CssSelector(".hero-subtitle"));
                        Assert.That(subTitle.Text, Does.Contain(expectedContent), 
                            $"副標題應該包含 '{expectedContent}'，但實際是 '{subTitle.Text}'");
                        break;
                    case "CTA 按鈕":
                        var ctaButton = heroSection.FindElement(By.CssSelector(".hero-cta"));
                        Assert.That(ctaButton.Text, Does.Contain(expectedContent), 
                            $"CTA 按鈕應該包含 '{expectedContent}'，但實際是 '{ctaButton.Text}'");
                        break;
                }
            }
        }

        [Then(@"功能卡片區應該顯示 (\d+) 張卡片:")]
        public void Then功能卡片區應該顯示張卡片(int expectedCount, Table table)
        {
            var featureCards = _driver.FindElements(By.CssSelector(".feature-card"));
            Assert.That(featureCards.Count, Is.EqualTo(expectedCount), 
                $"應該有 {expectedCount} 張功能卡片，但實際是 {featureCards.Count} 張");
            
            foreach (var row in table.Rows)
            {
                var expectedTitle = row["卡片標題"];
                var expectedIcon = row["圖示"];
                var expectedLink = row["連結"];
                
                // 找到對應的卡片
                var card = featureCards.FirstOrDefault(c => 
                    c.FindElement(By.CssSelector(".feature-card-title")).Text.Contains(expectedTitle));
                
                Assert.That(card, Is.Not.Null, $"找不到標題為 '{expectedTitle}' 的功能卡片");
                
                // 驗證圖示
                var icon = card!.FindElement(By.CssSelector(".feature-card-icon"));
                Assert.That(icon.Text, Does.Contain(expectedIcon), 
                    $"卡片 '{expectedTitle}' 的圖示應該是 '{expectedIcon}'，但實際是 '{icon.Text}'");
                
                // 驗證連結
                var link = card.GetAttribute("href") ?? card.FindElement(By.TagName("a")).GetAttribute("href");
                Assert.That(link, Does.Contain(expectedLink), 
                    $"卡片 '{expectedTitle}' 的連結應該包含 '{expectedLink}'，但實際是 '{link}'");
            }
        }

        [Then(@"區塊標題顯示 ""(.*)""")]
        public void Then區塊標題顯示(string expectedTitle)
        {
            var sectionTitle = _driver.FindElement(By.CssSelector(".quotes-section .section-title"));
            Assert.That(sectionTitle.Text, Does.Contain(expectedTitle), 
                $"區塊標題應該包含 '{expectedTitle}'，但實際是 '{sectionTitle.Text}'");
        }

        [Then(@"顯示至少 (\d+) 則名言內容")]
        public void Then顯示至少則名言內容(int minCount)
        {
            var quotes = _driver.FindElements(By.CssSelector(".quotes-section .quote-item"));
            Assert.That(quotes.Count, Is.GreaterThanOrEqualTo(minCount), 
                $"應該至少有 {minCount} 則名言，但實際是 {quotes.Count} 則");
        }

        [Then(@"頁面底部顯示 Footer 包含:")]
        public void Then頁面底部顯示Footer包含(Table table)
        {
            var footer = _driver.FindElement(By.CssSelector("footer"));
            var footerText = footer.Text;
            
            foreach (var row in table.Rows)
            {
                var item = row["項目"];
                var expectedContent = row["內容"];
                
                Assert.That(footerText, Does.Contain(expectedContent), 
                    $"Footer 中應該包含 '{expectedContent}' ({item})，但未找到");
            }
        }

        [Then(@"我應該看到 ""(.*)"" 區塊")]
        public void Then我應該看到指定區塊(string sectionName)
        {
            if (sectionName == "投資智慧語錄")
            {
                var quotesSection = _driver.FindElement(By.CssSelector(".quotes-section"));
                Assert.That(quotesSection.Displayed, Is.True, $"應該看到「{sectionName}」區塊");
            }
            else if (sectionName == "為什麼選擇被動投資")
            {
                var whyPassiveSection = _driver.FindElement(By.CssSelector(".why-passive-section"));
                Assert.That(whyPassiveSection.Displayed, Is.True, $"應該看到「{sectionName}」區塊");
            }
        }

        [Then(@"區塊內包含至少 (\d+) 個統計數據:")]
        public void Then區塊內包含至少個統計數據(int minCount, Table table)
        {
            var statsItems = _driver.FindElements(By.CssSelector(".why-passive-section .stat-item"));
            Assert.That(statsItems.Count, Is.GreaterThanOrEqualTo(minCount), 
                $"應該至少有 {minCount} 個統計數據，但實際是 {statsItems.Count} 個");
            
            foreach (var row in table.Rows)
            {
                var expectedData = row["數據"];
                var expectedDesc = row["說明"];
                
                var sectionText = _driver.FindElement(By.CssSelector(".why-passive-section")).Text;
                Assert.That(sectionText, Does.Contain(expectedData), 
                    $"區塊應該包含數據 '{expectedData}'");
                Assert.That(sectionText, Does.Contain(expectedDesc), 
                    $"區塊應該包含說明 '{expectedDesc}'");
            }
        }

        [Then(@"頁面頂部顯示導覽列包含:")]
        public void Then頁面頂部顯示導覽列包含(Table table)
        {
            var navbar = _driver.FindElement(By.CssSelector("nav.navbar"));
            Assert.That(navbar.Displayed, Is.True, "應該看到導覽列");
            
            foreach (var row in table.Rows)
            {
                var item = row["項目"];
                var expectedLink = row["連結"];
                
                if (item == "Logo")
                {
                    var logoLink = navbar.FindElement(By.CssSelector(".navbar-brand"));
                    var href = logoLink.GetAttribute("href");
                    Assert.That(href, Does.EndWith(expectedLink), 
                        $"Logo 連結應該是 '{expectedLink}'，但實際是 '{href}'");
                }
                else
                {
                    var navLinks = navbar.FindElements(By.CssSelector(".nav-link"));
                    var found = navLinks.Any(link => 
                        link.Text.Contains(item) || link.GetAttribute("href")?.Contains(expectedLink) == true);
                    Assert.That(found, Is.True, $"導覽列應該包含 '{item}' 項目");
                }
            }
        }

        [Given(@"我用手機尺寸 (\d+)x(\d+) 瀏覽首頁 ""(.*)""")]
        public void Given我用手機尺寸瀏覽首頁(int width, int height, string path)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            _driver = new ChromeDriver(options);
            _driver.Manage().Window.Size = new System.Drawing.Size(width, height);
            _driver.Navigate().GoToUrl($"http://localhost:5050{path}");
        }

        [Then(@"導覽列顯示漢堡選單圖示")]
        public void Then導覽列顯示漢堡選單圖示()
        {
            var hamburgerButton = _driver.FindElement(By.CssSelector(".navbar-toggler"));
            Assert.That(hamburgerButton.Displayed, Is.True, "在手機版應該顯示漢堡選單圖示");
        }

        [Then(@"頁面無橫向捲軸")]
        public void Then頁面無橫向捲軸()
        {
            var body = _driver.FindElement(By.TagName("body"));
            var viewportWidth = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return window.innerWidth");
            var scrollWidth = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return document.documentElement.scrollWidth");
            
            Assert.That(scrollWidth, Is.LessThanOrEqualTo(viewportWidth + 10), 
                $"頁面不應有橫向捲軸 (viewport: {viewportWidth}, scrollWidth: {scrollWidth})");
        }

        [AfterScenario]
        public void CleanUp()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}
