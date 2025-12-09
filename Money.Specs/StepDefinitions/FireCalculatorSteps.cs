using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TechTalk.SpecFlow;
using NUnit.Framework;

namespace Money.Specs.StepDefinitions
{
    [Binding]
    public class FireCalculatorSteps
    {
        private IWebDriver _driver = null!;

        [Given(@"我開啟瀏覽器進入 FIRE 計算機 ""(.*)""")]
        public void Given我開啟瀏覽器進入FIRE計算機(string path)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            _driver = new ChromeDriver(options);
            _driver.Navigate().GoToUrl($"http://localhost:5050{path}");
        }

        [Then(@"卡片標題顯示 ""(.*)""")]
        public void Then卡片標題顯示(string expectedTitle)
        {
            var title = _driver.FindElement(By.CssSelector(".card-title"));
            Assert.That(title.Text, Does.Contain(expectedTitle), 
                $"卡片標題應該包含 '{expectedTitle}'，但實際是 '{title.Text}'");
        }

        [Then(@"顯示「基本設定」輸入區塊")]
        public void Then顯示基本設定輸入區塊()
        {
            var basicSettings = _driver.FindElement(By.CssSelector(".basic-settings"));
            Assert.That(basicSettings.Displayed, Is.True, "應該顯示「基本設定」輸入區塊");
        }

        [Then(@"顯示計算按鈕")]
        public void Then顯示計算按鈕()
        {
            var calculateButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
            Assert.That(calculateButton.Displayed, Is.True, "應該顯示計算按鈕");
        }

        [AfterScenario]
        public void CleanUp()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}
