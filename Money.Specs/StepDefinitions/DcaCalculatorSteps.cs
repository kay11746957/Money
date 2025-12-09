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

        [AfterScenario]
        public void CleanUp()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}
