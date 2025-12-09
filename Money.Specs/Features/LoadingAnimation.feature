@LoadingAnimation
Feature: 載入動畫
    為了提供更好的使用者體驗
    當頁面載入或計算進行時
    我需要看到可愛的載入動畫

Scenario: 按鈕點擊微載入動畫
    Given 載入動畫測試 - 我在複利計算機頁面
    When 載入動畫測試 - 我點擊計算按鈕
    Then 按鈕應該顯示 spinner 動畫
    And 按鈕文字應該變成 "計算中..."

Scenario: 載入動畫無障礙支援
    Given 載入動畫測試 - 我在首頁
    Then 頁面應該包含可愛動畫 CSS 類別
    And CSS 應該支援 prefers-reduced-motion

Scenario: 可愛載入元素
    Given 載入動畫測試 - 我在首頁
    Then 頁面樣式應該包含可愛載入圖示類別

Scenario: 全域載入動畫遮罩
    Given 載入動畫測試 - 我在首頁
    Then 頁面應該包含載入遮罩元素

