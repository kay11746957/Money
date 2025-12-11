# 📊 被動指數投資計算平台

## 目標
提供一般投資者一個 **即時、直觀** 的理財工具，讓使用者能快速計算退休金缺口、複利成長與 ETF 投資組合回測，並顯示年化報酬率、夏普比率、最大回撤等風險指標，協助制定可行的投資與退休規劃。

## 主要功能
- **複利計算機**：定期定額投資成長曲線
- **退休金缺口計算**：結合勞保年金、現有資產與預期支出
- **ETF 回測比較**：多支 ETF 歷史績效、風險指標與組合分析
- **互動圖表**：使用 Chart.js 呈現投資曲線與風險雷達圖
- **RWD 介面**：支援桌面、平板與手機

## 技術棧
- **後端**：ASP.NET Core 8, C#
- **前端**：Vue.js 3, Bootstrap 5, Chart.js
- **測試**：SpecFlow (BDD) + Selenium WebDriver
- **CI/CD**：GitHub Actions → Azure App Service
- **安全**：CodeQL 靜態掃描、Dependabot 依賴管理

## 快速開始
```bash
# 1. 取得原始碼
git clone https://github.com/kay11746957/Money.git
cd Money/Money

# 2. 安裝相依套件 (需要 .NET 8 SDK)
#   Windows 建議使用 Visual Studio 2022 或 dotnet CLI

dotnet restore

# 3. 本機執行
dotnet run --urls "http://localhost:5050"
```
開啟瀏覽器並前往 `http://localhost:5050` 即可使用各項計算器。

## 測試
```bash
# 執行 BDD 測試
cd ../Money.Specs
dotnet test --filter "Category=Backtest"
```
所有測試均以 **Given‑When‑Then** 方式撰寫，確保功能符合需求。

## 部署
專案已設定 GitHub Actions 工作流程，推送至 `main` 分支即會自動建置、測試並部署至 Azure。

## 貢獻指南
1. Fork 本倉庫
2. 建立 feature 分支
3. 撰寫對應的 SpecFlow 測試
4. 提交 Pull Request，CI 會自動驗證

## 授權
本專案採用 MIT 授權，詳情請見 `LICENSE` 檔案。
