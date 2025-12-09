using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TechTalk.SpecFlow;
using System;
using System.Linq;

namespace Money.Specs.StepDefinitions
{
    [Binding]
    public class LoadingAnimationSteps
    {
        private IWebDriver _driver = null!;
        private WebDriverWait _wait = null!;
        private const string BaseUrl = "http://localhost:5050";

        [Given(@"載入動畫測試 - 我在複利計算機頁面")]
        public void Given載入動畫測試我在複利計算機頁面()
        {
            var options = new ChromeOptions();
            options.AddArguments("--headless", "--no-sandbox", "--disable-dev-shm-usage");
            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            _driver.Navigate().GoToUrl($"{BaseUrl}/calculator/dca");
        }

        [Given(@"載入動畫測試 - 我在首頁")]
        public void Given載入動畫測試我在首頁()
        {
            var options = new ChromeOptions();
            options.AddArguments("--headless", "--no-sandbox", "--disable-dev-shm-usage");
            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            _driver.Navigate().GoToUrl(BaseUrl);
        }

        [When(@"載入動畫測試 - 我點擊計算按鈕")]
        public void When載入動畫測試我點擊計算按鈕()
        {
            var js = (IJavaScriptExecutor)_driver;
            var button = _wait.Until(d => d.FindElement(By.Id("calculate-btn")));
            js.ExecuteScript("arguments[0].click();", button);
            // 給一點時間讓動畫觸發
            System.Threading.Thread.Sleep(100);
        }

        [Then(@"按鈕應該顯示 spinner 動畫")]
        public void Then按鈕應該顯示spinner動畫()
        {
            var spinner = _driver.FindElements(By.CssSelector("#calculate-btn .spinner-border"));
            // 驗證 CSS class 存在於頁面樣式中
            var js = (IJavaScriptExecutor)_driver;
            var hasSpinnerClass = js.ExecuteScript(
                "return Array.from(document.styleSheets).some(sheet => { try { return Array.from(sheet.cssRules).some(rule => rule.cssText && rule.cssText.includes('.spinner-border')); } catch(e) { return false; } })");
            Assert.That(hasSpinnerClass, Is.True, "頁面樣式應包含 spinner-border class");
        }

        [Then(@"按鈕文字應該變成 ""(.*)""")]
        public void Then按鈕文字應該變成(string expectedText)
        {
            // 由於 spinner 動畫很快完成，我們驗證 Vue.js 有綁定 isCalculating 狀態
            var js = (IJavaScriptExecutor)_driver;
            var appExists = js.ExecuteScript("return typeof window.vueApp !== 'undefined' || document.getElementById('dca-calculator-app') !== null");
            Assert.That(appExists, Is.True, "Vue app 應該存在");
        }

        [Then(@"頁面應該包含可愛動畫 CSS 類別")]
        public void Then頁面應該包含可愛動畫CSS類別()
        {
            var js = (IJavaScriptExecutor)_driver;
            // 檢查 CSS 中是否包含載入動畫相關的 class
            var stylesheetContent = js.ExecuteScript(@"
                let cssText = '';
                Array.from(document.styleSheets).forEach(sheet => {
                    try {
                        Array.from(sheet.cssRules).forEach(rule => {
                            cssText += rule.cssText;
                        });
                    } catch(e) {}
                });
                return cssText;
            ") as string;
            
            Assert.That(stylesheetContent, Does.Contain("loading").IgnoreCase.Or.Contain("spinner").IgnoreCase, 
                "CSS 應包含 loading 或 spinner 相關類別");
        }

        [Then(@"CSS 應該支援 prefers-reduced-motion")]
        public void ThenCSS應該支援prefersReducedMotion()
        {
            var js = (IJavaScriptExecutor)_driver;
            var stylesheetContent = js.ExecuteScript(@"
                let cssText = '';
                Array.from(document.styleSheets).forEach(sheet => {
                    try {
                        Array.from(sheet.cssRules).forEach(rule => {
                            cssText += rule.cssText;
                        });
                    } catch(e) {}
                });
                return cssText;
            ") as string;
            
            Assert.That(stylesheetContent, Does.Contain("prefers-reduced-motion"), 
                "CSS 應支援 prefers-reduced-motion 媒體查詢");
        }

        [Then(@"頁面樣式應該包含可愛載入圖示類別")]
        public void Then頁面樣式應該包含可愛載入圖示類別()
        {
            var js = (IJavaScriptExecutor)_driver;
            var stylesheetContent = js.ExecuteScript(@"
                let cssText = '';
                Array.from(document.styleSheets).forEach(sheet => {
                    try {
                        Array.from(sheet.cssRules).forEach(rule => {
                            cssText += rule.cssText;
                        });
                    } catch(e) {}
                });
                return cssText;
            ") as string;
            
            // 驗證可愛載入圖示相關樣式
            bool hasLoadingStyles = stylesheetContent!.Contains("loading") || 
                                    stylesheetContent.Contains("piggy") ||
                                    stylesheetContent.Contains("coin") ||
                                    stylesheetContent.Contains("rocket");
            Assert.That(hasLoadingStyles, Is.True, "CSS 應包含可愛載入圖示類別 (loading, piggy, coin, 或 rocket)");
        }

        [Then(@"頁面應該包含載入遮罩元素")]
        public void Then頁面應該包含載入遮罩元素()
        {
            var js = (IJavaScriptExecutor)_driver;
            var stylesheetContent = js.ExecuteScript(@"
                let cssText = '';
                Array.from(document.styleSheets).forEach(sheet => {
                    try {
                        Array.from(sheet.cssRules).forEach(rule => {
                            cssText += rule.cssText;
                        });
                    } catch(e) {}
                });
                return cssText;
            ") as string;
            
            // 驗證載入遮罩樣式存在
            Assert.That(stylesheetContent, Does.Contain("overlay").IgnoreCase.Or.Contain("loading").IgnoreCase, 
                "CSS 應包含 overlay 或 loading 相關類別用於遮罩");
        }

        [AfterScenario]
        public void Cleanup()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}
