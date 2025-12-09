using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TechTalk.SpecFlow;
using NUnit.Framework;

namespace Money.Specs.StepDefinitions
{
    [Binding]
    public class DcaCalculatorSteps
    {
        private IWebDriver _driver = null!;

        [Given(@"我開啟瀏覽器進入計算機頁面 ""(.*)""")]
        public void Given我開啟瀏覽器進入計算機頁面(string path)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            _driver = new ChromeDriver(options);
            _driver.Navigate().GoToUrl($"http://localhost:5050{path}");
        }

        [Given(@"我用手機尺寸 (\d+)x(\d+) 瀏覽計算機頁面 ""(.*)""")]
        public void Given我用手機尺寸瀏覽計算機頁面(int width, int height, string path)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            _driver = new ChromeDriver(options);
            _driver.Manage().Window.Size = new System.Drawing.Size(width, height);
            _driver.Navigate().GoToUrl($"http://localhost:5050{path}");
        }

        [Then(@"我應該看到計算機頁面標題 ""(.*)""")]
        public void Then我應該看到計算機頁面標題(string expectedTitle)
        {
            var title = _driver.FindElement(By.CssSelector(".card-title"));
            Assert.That(title.Text, Does.Contain(expectedTitle), 
                $"頁面標題應該包含 '{expectedTitle}'，但實際是 '{title.Text}'");
        }

        [Then(@"顯示月投入金額輸入欄位")]
        public void Then顯示月投入金額輸入欄位()
        {
            var input = _driver.FindElement(By.Id("monthlyInvestment"));
            Assert.That(input.Displayed, Is.True, "應該顯示月投入金額輸入欄位");
        }

        [Then(@"顯示年化報酬率選項")]
        public void Then顯示年化報酬率選項()
        {
            var select = _driver.FindElement(By.Id("annualRate"));
            Assert.That(select.Displayed, Is.True, "應該顯示年化報酬率選項");
        }

        [Then(@"顯示投資年數輸入欄位")]
        public void Then顯示投資年數輸入欄位()
        {
            var input = _driver.FindElement(By.Id("years"));
            Assert.That(input.Displayed, Is.True, "應該顯示投資年數輸入欄位");
        }

        [Then(@"計算機頁面無橫向捲軸")]
        public void Then計算機頁面無橫向捲軸()
        {
            var viewportWidth = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return window.innerWidth");
            var scrollWidth = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return document.documentElement.scrollWidth");
            Assert.That(scrollWidth, Is.LessThanOrEqualTo(viewportWidth + 10), 
                $"頁面不應有橫向捲軸 (viewport: {viewportWidth}, scrollWidth: {scrollWidth})");
        }

        [Then(@"輸入欄位寬度自適應")]
        public void Then輸入欄位寬度自適應()
        {
            var input = _driver.FindElement(By.Id("monthlyInvestment"));
            var inputWidth = input.Size.Width;
            var viewportWidth = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return window.innerWidth");
            Assert.That(inputWidth, Is.LessThanOrEqualTo(viewportWidth), "輸入欄位寬度應該自適應視窗");
        }

        [When(@"我輸入月投入金額為 (\d+)")]
        public void When我輸入月投入金額為(int amount)
        {
            var input = _driver.FindElement(By.Id("monthlyInvestment"));
            input.Clear();
            input.SendKeys(amount.ToString());
            // Trigger validation
            _driver.FindElement(By.Id("years")).Click();
            System.Threading.Thread.Sleep(300);
        }

        [When(@"我輸入投資年數為 (\d+)")]
        public void When我輸入投資年數為(int years)
        {
            var input = _driver.FindElement(By.Id("years"));
            input.Clear();
            input.SendKeys(years.ToString());
            // Trigger validation
            _driver.FindElement(By.Id("monthlyInvestment")).Click();
            System.Threading.Thread.Sleep(300);
        }

        [Then(@"顯示錯誤訊息 ""(.*)""")]
        public void Then顯示錯誤訊息(string expectedError)
        {
            var errorElements = _driver.FindElements(By.CssSelector(".invalid-feedback"));
            var allText = string.Join(" ", errorElements.Select(e => e.Text));
            Assert.That(allText, Does.Contain(expectedError), 
                $"應該顯示錯誤訊息 '{expectedError}'");
        }

        [When(@"我點擊計算按鈕")]
        public void When我點擊計算按鈕()
        {
            var button = _driver.FindElement(By.CssSelector("button[type='submit']"));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", button);
            System.Threading.Thread.Sleep(300);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", button);
            System.Threading.Thread.Sleep(1500); // Wait for calculation
        }

        [Then(@"顯示計算結果區塊")]
        public void Then顯示計算結果區塊()
        {
            var resultSection = _driver.FindElement(By.Id("result-section"));
            Assert.That(resultSection.Displayed, Is.True, "應該顯示計算結果區塊");
        }

        [Then(@"顯示圖表容器")]
        public void Then顯示圖表容器()
        {
            var chart = _driver.FindElement(By.Id("dca-chart"));
            Assert.That(chart.Displayed, Is.True, "應該顯示圖表容器");
        }

        [AfterScenario]
        public void CleanUp()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}
