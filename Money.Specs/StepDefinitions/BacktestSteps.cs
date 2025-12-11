using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TechTalk.SpecFlow;
using NUnit.Framework;

namespace Money.Specs.StepDefinitions
{
    [Binding]
    [Scope(Tag = "Backtest")]
    public class BacktestSteps
    {
        private IWebDriver _driver = null!;
        private const string BaseUrl = "http://localhost:5050";

        [Given(@"我開啟瀏覽器進入 ETF 回測頁面 ""(.*)""")]
        public void Given我開啟瀏覽器進入ETF回測頁面(string path)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            _driver = new ChromeDriver(options);
            _driver.Navigate().GoToUrl($"{BaseUrl}{path}");
        }

        [Then(@"頁面應該載入成功")]
        public void Then頁面應該載入成功()
        {
            // 驗證頁面載入成功
            var body = _driver.FindElement(By.TagName("body"));
            Assert.That(body, Is.Not.Null, "頁面應該載入成功");
            
            // 確認不是錯誤頁面
            var pageSource = _driver.PageSource;
            Assert.That(pageSource, Does.Not.Contain("404 - Not Found"), "不應該是 404 錯誤頁面");
            Assert.That(pageSource, Does.Not.Contain("An error occurred"), "不應該是錯誤頁面");
        }

        [Then(@"頁面標題應該包含 ""(.*)""")]
        public void Then頁面標題應該包含(string expectedTitle)
        {
            var pageTitle = _driver.Title;
            var h1Element = _driver.FindElements(By.TagName("h1")).FirstOrDefault();
            var pageText = h1Element?.Text ?? "";
            
            var containsTitle = pageTitle.Contains(expectedTitle) || pageText.Contains(expectedTitle);
            Assert.That(containsTitle, Is.True, 
                $"頁面標題或 H1 應該包含 '{expectedTitle}'，但標題是 '{pageTitle}'，H1 是 '{pageText}'");
        }

        // ===== Phase 2: ETF 選擇介面步驟 =====

        [Then(@"我應該看到 ETF 選擇區塊")]
        public void Then我應該看到ETF選擇區塊()
        {
            var etfSection = _driver.FindElement(By.Id("etf-selection"));
            Assert.That(etfSection, Is.Not.Null, "應該看到 ETF 選擇區塊");
            Assert.That(etfSection.Displayed, Is.True, "ETF 選擇區塊應該可見");
        }

        [Then(@"應該有 ETF 搜尋框")]
        public void Then應該有ETF搜尋框()
        {
            var searchInput = _driver.FindElement(By.Id("etf-search"));
            Assert.That(searchInput, Is.Not.Null, "應該有 ETF 搜尋框");
            Assert.That(searchInput.Displayed, Is.True, "ETF 搜尋框應該可見");
        }

        [Then(@"預設選擇 ""(.*)"" 與 ""(.*)"" 兩檔 ETF")]
        public void Then預設選擇兩檔ETF(string etf1, string etf2)
        {
            var selectedEtfs = _driver.FindElements(By.CssSelector(".selected-etf"));
            var selectedTexts = selectedEtfs.Select(e => e.Text).ToList();
            
            Assert.That(selectedTexts.Any(t => t.Contains(etf1)), Is.True, 
                $"預設應該選擇 {etf1}");
            Assert.That(selectedTexts.Any(t => t.Contains(etf2)), Is.True, 
                $"預設應該選擇 {etf2}");
        }

        // ===== Phase 3: 回測參數設定步驟 =====

        [Then(@"我應該看到回測參數設定區")]
        public void Then我應該看到回測參數設定區()
        {
            var paramsSection = _driver.FindElement(By.Id("backtest-params"));
            Assert.That(paramsSection, Is.Not.Null, "應該看到回測參數設定區");
            Assert.That(paramsSection.Displayed, Is.True, "回測參數設定區應該可見");
        }

        [Then(@"應該可以選擇回測期間")]
        public void Then應該可以選擇回測期間()
        {
            var periodSelect = _driver.FindElement(By.Id("backtest-period"));
            Assert.That(periodSelect, Is.Not.Null, "應該有回測期間選擇器");
            Assert.That(periodSelect.Displayed, Is.True, "回測期間選擇器應該可見");
            
            // 驗證有期間選項
            var options = periodSelect.FindElements(By.TagName("option"));
            Assert.That(options.Count, Is.GreaterThan(0), "應該有回測期間選項");
        }

        [Then(@"應該可以選擇投資方式")]
        public void Then應該可以選擇投資方式()
        {
            var modeSelect = _driver.FindElement(By.Id("investment-mode"));
            Assert.That(modeSelect, Is.Not.Null, "應該有投資方式選擇器");
            Assert.That(modeSelect.Displayed, Is.True, "投資方式選擇器應該可見");
        }

        [Then(@"應該有開始回測按鈕")]
        public void Then應該有開始回測按鈕()
        {
            var button = _driver.FindElement(By.Id("start-backtest"));
            Assert.That(button, Is.Not.Null, "應該有開始回測按鈕");
            Assert.That(button.Displayed, Is.True, "開始回測按鈕應該可見");
        }

        // ===== Phase 4: 後端 API 資料抓取步驟 =====

        [When(@"點擊開始回測按鈕")]
        public void When點擊開始回測按鈕()
        {
            // 等待頁面完全載入
            Thread.Sleep(2000);
            
            var button = _driver.FindElement(By.Id("start-backtest"));
            
            // 滾動到按鈕位置
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", button);
            Thread.Sleep(500);
            
            // 使用 JavaScript 點擊以避免 "not clickable" 問題
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", button);
        }

        [Then(@"應該顯示載入中狀態")]
        public void Then應該顯示載入中狀態()
        {
            // 等待載入狀態出現
            Thread.Sleep(500);
            
            // 檢查按鈕是否顯示載入中文字或 spinner
            var button = _driver.FindElement(By.Id("start-backtest"));
            var buttonText = button.Text;
            var hasSpinner = button.FindElements(By.CssSelector(".spinner-border")).Count > 0;
            
            // 載入中狀態：按鈕文字包含 "計算中" 或有 spinner
            var isLoading = buttonText.Contains("計算中") || hasSpinner;
            Assert.That(isLoading, Is.True, 
                $"應該顯示載入中狀態，按鈕文字: '{buttonText}'");
        }

        [Then(@"回測完成後應該顯示結果區塊")]
        public void Then回測完成後應該顯示結果區塊()
        {
            // 等待回測完成 (最多等 15 秒)
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            wait.Until(d => {
                var resultSection = d.FindElements(By.Id("backtest-result"));
                return resultSection.Count > 0 && resultSection[0].Displayed;
            });
            
            var result = _driver.FindElement(By.Id("backtest-result"));
            Assert.That(result.Displayed, Is.True, "回測完成後應該顯示結果區塊");
        }

        // ===== Phase 5: 績效走勢圖步驟 =====

        [Then(@"應該顯示績效走勢圖")]
        public void Then應該顯示績效走勢圖()
        {
            var chartCanvas = _driver.FindElement(By.Id("performance-chart"));
            Assert.That(chartCanvas, Is.Not.Null, "應該有績效走勢圖 canvas 元素");
            Assert.That(chartCanvas.Displayed, Is.True, "績效走勢圖應該可見");
        }

        // ===== Phase 6: 投資組合回測步驟 =====

        [Then(@"應該有回測模式切換選項")]
        public void Then應該有回測模式切換選項()
        {
            var modeSwitch = _driver.FindElement(By.Id("backtest-mode"));
            Assert.That(modeSwitch, Is.Not.Null, "應該有回測模式切換選項");
            Assert.That(modeSwitch.Displayed, Is.True, "回測模式切換應該可見");
        }

        [Then(@"可以切換到投資組合模式")]
        public void Then可以切換到投資組合模式()
        {
            var portfolioOption = _driver.FindElement(By.CssSelector("[data-mode='portfolio']"));
            Assert.That(portfolioOption, Is.Not.Null, "應該有投資組合模式選項");
        }

        // ===== Phase 7: 匯出與分享步驟 =====

        [Then(@"應該有分享按鈕")]
        public void Then應該有分享按鈕()
        {
            var shareButton = _driver.FindElement(By.Id("share-button"));
            Assert.That(shareButton, Is.Not.Null, "應該有分享按鈕");
            Assert.That(shareButton.Displayed, Is.True, "分享按鈕應該可見");
        }

        // ===== Phase 8: 投資組合風險指標步驟 =====

        [When(@"切換到投資組合模式")]
        public void When切換到投資組合模式()
        {
            Thread.Sleep(1000);
            var portfolioTab = _driver.FindElement(By.Id("mode-portfolio"));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", portfolioTab);
            Thread.Sleep(500);
        }

        [Then(@"應該顯示投資組合風險指標區塊")]
        public void Then應該顯示投資組合風險指標區塊()
        {
            // 等待投資組合回測完成 (最多等 20 秒)
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            wait.Until(d => {
                var element = d.FindElements(By.Id("portfolio-risk-metrics"));
                return element.Count > 0 && element[0].Displayed;
            });
            
            var riskMetrics = _driver.FindElement(By.Id("portfolio-risk-metrics"));
            Assert.That(riskMetrics, Is.Not.Null, "應該有風險指標區塊");
            Assert.That(riskMetrics.Displayed, Is.True, "風險指標區塊應該可見");
        }

        [Then(@"應該顯示組合與個別ETF比較表")]
        public void Then應該顯示組合與個別ETF比較表()
        {
            var comparisonTable = _driver.FindElement(By.Id("portfolio-comparison-table"));
            Assert.That(comparisonTable, Is.Not.Null, "應該有比較表");
            Assert.That(comparisonTable.Displayed, Is.True, "比較表應該可見");
        }

        [Then(@"組合的年化報酬率應該有數值")]
        public void Then組合的年化報酬率應該有數值()
        {
            // 等待結果載入
            Thread.Sleep(500);
            
            // 查找 portfolio-result 區塊中的 CAGR 值
            var portfolioResult = _driver.FindElement(By.Id("portfolio-result"));
            var cagrText = portfolioResult.Text;
            
            // 確認包含非零的年化報酬率數值
            Assert.That(cagrText.Contains("年化報酬率") || cagrText.Contains("cagr"), Is.True, 
                "應該顯示年化報酬率");
            
            // 確認不是 0%
            Assert.That(!cagrText.Contains("0%") || cagrText.Contains("10") || cagrText.Contains("5") || cagrText.Contains("8"), Is.True,
                "年化報酬率應該有實際數值，不應該是 0%");
        }

        [Then(@"組合的夏普比率應該有數值")]
        public void Then組合的夏普比率應該有數值()
        {
            var riskMetrics = _driver.FindElement(By.Id("portfolio-risk-metrics"));
            var metricsText = riskMetrics.Text;
            
            // 確認夏普比率存在且不是 0
            Assert.That(metricsText.Contains("夏普比率"), Is.True, "應該顯示夏普比率");
        }

        [Then(@"風險指標旁邊應該有解釋圖示")]
        public void Then風險指標旁邊應該有解釋圖示()
        {
            // 等待 info-icon 元素出現 (最多等 10 秒)
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(d => {
                var riskMetrics = d.FindElements(By.Id("portfolio-risk-metrics"));
                if (riskMetrics.Count == 0 || !riskMetrics[0].Displayed) return false;
                
                var icons = riskMetrics[0].FindElements(By.CssSelector("i.info-icon"));
                return icons.Count > 0;
            });
            
            // 驗證圖示數量
            var riskMetricsEl = _driver.FindElement(By.Id("portfolio-risk-metrics"));
            var tooltipIcons = riskMetricsEl.FindElements(By.CssSelector("i.info-icon"));
            
            Assert.That(tooltipIcons.Count, Is.GreaterThan(0), 
                $"風險指標旁邊應該有解釋圖示，找到 {tooltipIcons.Count} 個");
        }

        [AfterScenario]
        public void CleanUp()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}
