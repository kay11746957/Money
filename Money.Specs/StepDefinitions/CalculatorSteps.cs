using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TechTalk.SpecFlow;
using NUnit.Framework;

namespace Money.Specs.StepDefinitions
{
    [Binding]
    public class CalculatorSteps
    {
        private IWebDriver _driver = null!;
        private string _pageTitle = string.Empty;

        [Given(@"我開啟瀏覽器進入首頁")]
        public void Given我開啟瀏覽器進入首頁()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            _driver = new ChromeDriver(options);
            _driver.Navigate().GoToUrl("http://localhost:5050/");
            _pageTitle = _driver.Title;
        }

        [Then(@"頁面標題應該包含 ""(.*)""")]
        public void Then頁面標題應該包含(string expectedTitle)
        {
            Assert.That(_pageTitle, Does.Contain(expectedTitle), 
                $"頁面標題應該包含 '{expectedTitle}'，但實際是 '{_pageTitle}'");
        }

        [AfterScenario]
        public void CleanUp()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}
