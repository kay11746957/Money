# language: zh-TW
功能: ETF 歷史回測比較
  作為投資者
  我想要比較台股與美股 ETF 的歷史績效
  為了做出更好的投資決策

  # ===== Phase 1: Walking Skeleton =====
  
  @WalkingSkeleton @Backtest
  場景: ETF 回測頁面存在
    假設 我開啟瀏覽器進入 ETF 回測頁面 "/calculator/backtest"
    那麼 頁面應該載入成功
    而且 頁面標題應該包含 "ETF 歷史回測"

  # ===== Phase 2: ETF 選擇介面 =====

  @Backtest @ETFSelection
  場景: ETF 選擇介面
    假設 我開啟瀏覽器進入 ETF 回測頁面 "/calculator/backtest"
    那麼 我應該看到 ETF 選擇區塊
    而且 應該有 ETF 搜尋框
    而且 預設選擇 "0050" 與 "VTI" 兩檔 ETF

  @skip @SearchableDropdown
  場景: 可搜尋的 ETF 下拉選單
    假設 我點擊 ETF 搜尋框
    那麼 應該顯示可搜尋的下拉選單
    而且 輸入時即時顯示符合的選項

  @skip @ETFList
  場景: 支援的 ETF 清單
    那麼 台股 ETF 應該包含 "0050", "0056", "006208"
    而且 美股 ETF 應該包含 "VTI", "VOO", "VT", "QQQ"

  # ===== Phase 3: 回測參數設定 =====

  @Backtest @BacktestParams
  場景: 回測參數設定
    假設 我開啟瀏覽器進入 ETF 回測頁面 "/calculator/backtest"
    那麼 我應該看到回測參數設定區
    而且 應該可以選擇回測期間
    而且 應該可以選擇投資方式
    而且 應該有開始回測按鈕

  # ===== Phase 4: 後端 API 資料抓取 =====

  @Backtest @DataFetch
  場景: 後端抓取 ETF 歷史資料
    假設 我開啟瀏覽器進入 ETF 回測頁面 "/calculator/backtest"
    當 點擊開始回測按鈕
    那麼 回測完成後應該顯示結果區塊

  # ===== Phase 5: 績效走勢圖 =====

  @Backtest @Chart
  場景: 績效走勢圖顯示
    假設 我開啟瀏覽器進入 ETF 回測頁面 "/calculator/backtest"
    當 點擊開始回測按鈕
    那麼 回測完成後應該顯示結果區塊
    而且 應該顯示績效走勢圖

  # ===== Phase 6: 投資組合回測 =====

  @Backtest @Portfolio
  場景: 投資組合模式切換
    假設 我開啟瀏覽器進入 ETF 回測頁面 "/calculator/backtest"
    那麼 應該有回測模式切換選項
    而且 可以切換到投資組合模式

  # ===== Phase 7: 匯出與分享 =====

  @Backtest @Export
  場景: 回測結果匯出
    假設 我開啟瀏覽器進入 ETF 回測頁面 "/calculator/backtest"
    當 點擊開始回測按鈕
    那麼 回測完成後應該顯示結果區塊
    而且 應該有分享按鈕

  # ===== Phase 8: 投資組合風險指標 =====

  @Backtest @PortfolioRiskMetrics
  場景: 投資組合進階風險指標
    假設 我開啟瀏覽器進入 ETF 回測頁面 "/calculator/backtest"
    當 切換到投資組合模式
    而且 點擊開始回測按鈕
    那麼 應該顯示投資組合風險指標區塊
    而且 應該顯示組合與個別ETF比較表
    而且 組合的年化報酬率應該有數值
    而且 組合的夏普比率應該有數值
    而且 風險指標旁邊應該有解釋圖示
