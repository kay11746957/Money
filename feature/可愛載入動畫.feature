# language: zh-TW
Feature: 可愛的載入動畫
  作為使用者
  我想要在載入模組時看到可愛的動畫
  為了讓等待過程更有趣不無聊

  @Loading @Animation
  Scenario: 全域載入動畫規格
    Given 任何頁面正在載入中
    Then 顯示置中的可愛載入動畫:
      | 屬性           | 說明                                     |
      | 位置           | 頁面正中央 (fixed, z-index: 9999)        |
      | 背景           | 半透明遮罩 (rgba(255,255,255,0.9))       |
      | 動畫主題       | 錢幣/豬公/小火箭 擇一                    |
      | 動畫效果       | 彈跳 + 旋轉 (bounce + rotate)            |
      | 顏色主題       | 紫色調配合整體設計                       |
    And 顯示載入文字 (隨機切換):
      | 文字                         |
      | "正在計算你的財富... 🚀"     |
      | "數錢中，請稍候... 💰"       |
      | "財務自由的路上... 🏃‍♂️"      |
      | "複利魔法施展中... ✨"       |
    And 文字有打字機效果 (typewriter animation)

  @Loading @Skeleton
  Scenario: 骨架屏載入效果
    Given 頁面主體內容正在載入
    Then 顯示骨架屏 (Skeleton Loading):
      | 元素           | 效果                                 |
      | 卡片區域       | 灰色方塊 + shimmer 波光效果          |
      | 標題           | 長條形骨架                           |
      | 輸入欄位       | 圓角矩形骨架                         |
      | 按鈕           | 圓角矩形骨架                         |
    And 骨架有 shimmer 動畫 (左到右閃爍)

  @Loading @Micro
  Scenario: 按鈕點擊微載入動畫
    Given 使用者點擊任何計算按鈕
    Then 按鈕顯示:
      | 狀態           | 效果                                 |
      | 載入中         | 內容替換為 spinner + "計算中..."     |
      | 按鈕           | disabled + opacity: 0.7              |
      | spinner        | 圓形旋轉動畫 (border-spinner)        |
    And spinner 顏色與按鈕主色搭配

  @Loading @Progress
  Scenario: 複雜計算進度指示
    Given 回測模擬或複雜計算進行中
    Then 顯示進度條動畫:
      | 元素           | 效果                                 |
      | 進度條         | 漸層色填充 (紫色調)                  |
      | 進度文字       | "計算中 45%..." 即時更新             |
      | 預估時間       | "大約需要 3 秒"                      |
    And 進度條有 pulse 脈動效果

  @Loading @PageTransition
  Scenario: 頁面切換過渡動畫
    Given 使用者點擊導航連結
    When 頁面開始載入新模組
    Then 顯示過渡動畫:
      | 階段           | 效果                                 |
      | 離開動畫       | 當前頁面淡出 (fade-out 200ms)        |
      | 載入動畫       | 可愛 loading 圖示出現                |
      | 進入動畫       | 新頁面淡入 (fade-in 300ms)           |
    And 過渡時間不超過 500ms

  @Loading @CuteElements
  Scenario: 可愛載入圖示庫
    Then 載入動畫可選用以下可愛元素:
      | 圖示           | CSS className          | 動畫                   |
      | 🐷 存錢豬      | .loading-piggy         | 搖擺 + 閃亮眼睛        |
      | 💰 錢幣堆疊    | .loading-coins         | 硬幣掉落堆疊動畫       |
      | 🚀 小火箭      | .loading-rocket        | 上下彈跳 + 火焰閃爍    |
      | 📈 成長曲線    | .loading-chart         | 曲線繪製動畫           |
      | ⭐ 星星        | .loading-stars         | 閃爍 + 旋轉            |
    And 圖示使用 Lottie 或 CSS animation 實作
    And 圖示大小適中 (64px - 96px)

  @Loading @Accessibility
  Scenario: 載入動畫無障礙支援
    Then 載入動畫應有以下無障礙特性:
      | 特性                   | 實作                             |
      | 螢幕閱讀器             | aria-live="polite" 宣告載入狀態  |
      | role                   | role="status" or role="alert"    |
      | 減少動態偏好           | prefers-reduced-motion 支援      |
    And 若使用者偏好減少動態，則只顯示靜態載入指示

  @Loading @Timing
  Scenario: 載入動畫時機控制
    Then 載入動畫顯示規則:
      | 情境                   | 行為                             |
      | 載入 < 200ms           | 不顯示動畫 (避免閃爍)            |
      | 載入 200ms - 1s        | 顯示簡單 spinner                 |
      | 載入 > 1s              | 顯示完整可愛動畫 + 文字          |
      | 載入 > 5s              | 額外顯示 "快好了" 鼓勵文字       |
