系統架構概述（High-Level Vibes）
# Task
請你嚴格遵照「行為驅動開發 (BDD)」的方式，來完成 @order.feature 中所有驗收情境的開發。
不可同時進行 BDD 開發流程中多個步驟也不能略過任何一步驟，必須一步一步扎實執行並確認每一步的結果。

## 技術棧
- 後端：ASP.NET Core MVC
- 前端：Vue.js + Razor Views + Chart.js（CDN）
- RWD：Bootstrap 5
- 版控：GitHub

## CI/CD 與安全性 (GitHub Actions)

### 持續整合 (CI)
- 自動建置：每次 Push 或 PR 觸發
- 單元測試：dotnet test
- BDD 測試：SpecFlow

### 安全掃描 (GitHub 內建)
- **Dependabot**：自動偵測相依套件漏洞，發 PR 更新
- **CodeQL**：靜態程式碼分析，檢測安全弱點
- **Secret Scanning**：防止意外提交敏感資訊

### 部署
-Azure App Service


# BDD 開發流程
1. 先建置出 behave walking skeleton - 可順利運行 behave 以及至少一個 scenario，確認至少有一個 test case 被測試框架執行到。

2. 嚴格遵守 BDD 以及最小增量原則來開發所有程式碼，針對所有 scenario，一次開發一個 scenario，依序進行：
    A. 一次選擇一個 scenario 實作，除此 scenario 之外的測試全部都用 @skip 標記忽略。撰寫此 scenario 對應的 Steps (given, when, then)、開啟相關類別，但是每個類別的行為都不實作，並且執行測試，確認測試失敗 (test fail)，並且測試失敗的原因並非框架層級的錯誤，而是期望的「值」上的錯誤。嚴格確認這步驟完成後才能進行下一步的實作。
    B. 為了通過上一步所撰寫的測試程式碼，請實作相關類別所需的程式碼，並確認能讓所有的測試程式碼都通過。請嚴格確認有執行到測試程式碼，從 test report 中覆述一次目前 test passed 的數量。
    C. 遵守 clean code 原則，思考是否要重構每個類別的內部程式碼，如果必要重構的話，在重構完成之後，再執行一次測試，確保所有測試仍然通過，否則需修正邏輯直到測試全數通過。