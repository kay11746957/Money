# language: zh-TW
功能: FIRE 退休金計算機
  作為想規劃退休的投資人
  我想要計算每月需要投資多少錢才能達成退休目標
  為了確保退休後能維持理想的生活品質

  @UI @InitialState
  場景: 初始頁面載入與參數輸入區
    假設 我開啟瀏覽器進入 FIRE 計算機 "/calculator/fire"
    那麼 卡片標題顯示 "退休金缺口與投資計算器"
    並且 顯示「基本設定」輸入區塊
    並且 顯示計算按鈕

  @UI @LaborInsurance
  場景: 勞保年金簡易概算模式
    假設 我開啟瀏覽器進入 FIRE 計算機 "/calculator/fire"
    那麼 顯示勞保年金區塊
    並且 顯示勞保計算方式選項

  @Calculator @Result
  場景: 計算成功後顯示結果
    假設 我開啟瀏覽器進入 FIRE 計算機 "/calculator/fire"
    當 我點擊 FIRE 計算按鈕
    那麼 顯示 FIRE 計算結果區塊
