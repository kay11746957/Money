# language: zh-TW
Feature: ETF 歷史回測比較
  作為投資者
  我想要比較台股與美股 ETF 的歷史績效
  為了做出更好的投資決策

  Background:
    Given 我開啟瀏覽器進入 ETF 回測頁面 "/calculator/backtest"
    Then 頁面應該載入在 3 秒內

  # ===== ETF 選擇 =====

  @UI @ETFSelection
  Scenario: ETF 選擇介面
    Then 我應該看到 ETF 選擇區塊包含:
      | 元素           | 內容                                   |
      | 區塊標題       | "選擇要比較的 ETF"                      |
      | ETF 搜尋框     | 可輸入關鍵字搜尋的下拉選單 (Select2 風格) |
      | 新增 ETF 按鈕  | 可新增最多 5 檔 ETF 進行比較             |
    And 預設選擇 "0050" 與 "VTI" 兩檔 ETF

  @UI @SearchableDropdown
  Scenario: 可搜尋的 ETF 下拉選單
    Given 我點擊 ETF 搜尋框
    Then 應該顯示可搜尋的下拉選單:
      | 功能             | 說明                                    |
      | 關鍵字搜尋       | 可輸入代碼或名稱進行篩選                 |
      | 分類顯示         | 依「台股」和「美股」分組顯示              |
      | 即時過濾         | 輸入時即時顯示符合的選項                 |
      | 選項格式         | 顯示「代碼 - 名稱」如 "0050 - 元大台灣50" |
      | 空狀態           | 無符合結果時顯示 "找不到符合的 ETF"       |
    
  @UI @SearchExample
  Scenario Outline: ETF 搜尋範例
    When 我在搜尋框輸入 "<關鍵字>"
    Then 應該顯示包含 "<預期結果>" 的選項
    
    Examples:
      | 關鍵字   | 預期結果                     |
      | 0050     | 0050 - 元大台灣50            |
      | 台灣     | 0050 - 元大台灣50            |
      | VTI      | VTI - Vanguard 全美股市       |
      | 高股息   | 0056 - 元大高股息, 00878 - 國泰永續高股息 |
      | S&P      | VOO - Vanguard S&P 500       |

  @UI @ETFList
  Scenario: 支援的 ETF 清單
    Then 台股 ETF 應該包含以下選項:
      | 代碼     | 名稱                    |
      | 0050     | 元大台灣50              |
      | 0056     | 元大高股息              |
      | 006208   | 富邦台50                |
      | 00878    | 國泰永續高股息           |
      | 00692    | 富邦公司治理            |
    And 美股 ETF 應該包含以下選項:
      | 代碼     | 名稱                    |
      | VTI      | Vanguard 全美股市        |
      | VOO      | Vanguard S&P 500        |
      | VT       | Vanguard 全世界股市      |
      | QQQ      | Invesco 納斯達克100      |
      | VWO      | Vanguard 新興市場        |
      | VEA      | Vanguard 已開發市場      |

  # ===== ETF 組合回測 =====

  @UI @PortfolioMode
  Scenario: 切換回測模式
    Then 回測頁面應該提供兩種模式切換:
      | 模式           | 說明                                   |
      | 單一 ETF 比較  | 比較多檔獨立 ETF 的績效 (預設)          |
      | 投資組合回測   | 建立 ETF 組合並設定配置比例             |
    And 點擊 Tab 可切換模式

  @UI @PortfolioBuilder
  Scenario: 投資組合建立介面
    Given 我切換到 "投資組合回測" 模式
    Then 我應該看到投資組合建立區:
      | 元素               | 說明                                    |
      | 組合名稱輸入框     | 可自訂組合名稱，如 "我的股債配置"         |
      | ETF 搜尋框         | 可搜尋並新增 ETF 到組合中                |
      | 配置比例滑桿       | 每檔 ETF 可設定 1%-100% 的配置比例       |
      | 比例合計顯示       | 顯示目前總比例，必須等於 100%            |
      | 新增組合按鈕       | 可建立多個投資組合進行比較               |
    And 最多可新增 3 個投資組合進行比較

  @UI @PortfolioWeightAllocation
  Scenario: 設定 ETF 配置比例
    Given 我在投資組合中新增 "VTI" 和 "BND"
    Then 我應該看到配置比例設定:
      | ETF   | 預設比例   | 可調整範圍   |
      | VTI   | 50%        | 1% - 99%     |
      | BND   | 50%        | 1% - 99%     |
    And 拖曳滑桿可調整比例
    And 也可以直接輸入數字
    And 總比例必須等於 100%，否則顯示警告

  @UI @PortfolioWeightValidation
  Scenario: 配置比例驗證
    Given 我設定 VTI 60% 和 VOO 50%
    Then 應該顯示錯誤 "配置比例總和為 110%，請調整為 100%"
    And "開始回測" 按鈕應該被禁用
    When 我將 VOO 調整為 40%
    Then 錯誤訊息消失
    And "開始回測" 按鈕可以點擊

  @UI @PresetPortfolios
  Scenario: 預設投資組合範本
    Given 我切換到 "投資組合回測" 模式
    Then 應該提供預設組合範本:
      | 範本名稱           | 組合內容                                |
      | 經典 60/40        | VTI 60% + BND 40%                       |
      | 全球股市          | VTI 50% + VEA 30% + VWO 20%             |
      | 台美配置          | 0050 50% + VTI 50%                      |
      | 三基金組合        | VTI 40% + VXUS 40% + BND 20%            |
    And 點擊範本可快速套用

  @Logic @PortfolioRebalancing
  Scenario: 再平衡策略設定
    Given 我建立了一個投資組合
    Then 應該可以設定再平衡策略:
      | 策略             | 說明                                    |
      | 不再平衡         | 買入後不做任何調整 (預設)                |
      | 年度再平衡       | 每年 1 月 1 日調整回原始比例             |
      | 季度再平衡       | 每季調整回原始比例                       |
      | 閾值再平衡       | 當偏離原始比例超過 5% 時調整             |

  @UI @PortfolioResult
  Scenario: 投資組合回測結果
    Given 完成投資組合回測
    Then 應該顯示組合績效摘要:
      | 欄位               | 我的組合範例   | 經典 60/40 範例 |
      | 最終資產價值       | NT$ 3,234,567  | NT$ 2,876,543   |
      | 年化報酬率 (CAGR)  | 8.5%           | 7.2%            |
      | 年化波動率         | 12.3%          | 9.8%            |
      | 最大回撤           | -28.5%         | -22.1%          |
      | 夏普比率           | 0.69           | 0.73            |
    And 顯示各組合的報酬/風險效率比較

  @UI @PortfolioChart
  Scenario: 投資組合績效圖表
    Given 完成投資組合回測
    Then 應該顯示以下圖表:
      | 圖表名稱           | 說明                                    |
      | 累積報酬走勢       | 各組合的累積報酬率比較                   |
      | 組合配置圓餅圖     | 顯示目前/最終的資產配置比例              |
      | 各成分貢獻度       | 顯示各 ETF 對組合報酬的貢獻度            |
      | 回撤比較           | 各組合的回撤走勢比較                     |

  @UI @PortfolioBreakdown  
  Scenario: 組合成分績效明細
    Given 完成投資組合回測
    Then 應該顯示各成分 ETF 的績效明細:
      | ETF   | 配置比例 | 個別報酬率 | 貢獻度  |
      | VTI   | 60%      | +156%      | +93.6%  |
      | BND   | 40%      | +28%       | +11.2%  |
    And 可以展開查看各 ETF 的詳細數據|

  # ===== 回測參數設定 =====

  @UI @BacktestParams
  Scenario: 回測參數設定
    Then 我應該看到回測參數設定區:
      | 參數             | 預設值     | 說明                        |
      | 回測起始日期     | 10年前     | 可選擇 1, 3, 5, 10, 20 年    |
      | 投資方式         | 定期定額   | 單筆投入 / 定期定額          |
      | 每月投入金額     | 10,000     | 定期定額時使用               |
      | 單筆投入金額     | 1,000,000  | 單筆投入時使用               |
      | 是否配息再投入   | 是         | 勾選框                       |

  @Logic @DateValidation
  Scenario: 日期驗證
    Given 我選擇回測期間為 "20 年"
    When 選擇的 ETF 成立時間不足 20 年
    Then 應該顯示警告 "部分 ETF 資料不足 20 年，將以實際成立日開始計算"
    And 顯示各 ETF 的實際起始日期

  # ===== 後端資料抓取 =====

  @Backend @DataFetch
  Scenario: 後端抓取 ETF 歷史資料
    Given 使用者選擇 "0050" 和 "VTI"
    When 點擊 "開始回測" 按鈕
    Then 後端應該從 Yahoo Finance API 抓取資料
    And 資料應該包含:
      | 欄位           | 說明                     |
      | 日期           | 交易日期                 |
      | 調整後收盤價   | 已調整配息配股的價格      |
      | 配息           | 該期間的配息記錄          |
    And 資料快取 24 小時避免重複請求

  @Backend @DataSource
  Scenario: 資料來源處理
    Then 台股 ETF 資料來源為:
      | 符號格式       | 例如 "0050.TW"           |
      | API 來源       | Yahoo Finance            |
      | 貨幣           | TWD                      |
    And 美股 ETF 資料來源為:
      | 符號格式       | 例如 "VTI"               |
      | API 來源       | Yahoo Finance            |
      | 貨幣           | USD                      |

  @Backend @CurrencyConversion
  Scenario: 匯率轉換
    When 比較台股與美股 ETF
    Then 應該提供兩種比較模式:
      | 模式           | 說明                                    |
      | 原始貨幣       | 各自以原幣計算報酬率                     |
      | 統一換算 TWD   | 美股報酬以當時匯率換算成台幣             |
    And 預設使用 "原始貨幣" 模式進行報酬率比較

  # ===== 回測結果顯示 =====

  @UI @BacktestResult
  Scenario: 回測結果摘要
    Given 完成回測計算
    Then 應該顯示結果摘要卡片:
      | 欄位               | 0050 範例      | VTI 範例       |
      | 最終資產價值       | NT$ 2,456,789  | $45,678        |
      | 總投入本金         | NT$ 1,200,000  | $40,000        |
      | 總報酬率           | +104.7%        | +89.3%         |
      | 年化報酬率 (CAGR)  | 7.2%           | 6.5%           |
      | 最大回撤           | -35.4%         | -33.9%         |
      | 夏普比率           | 0.82           | 0.78           |
    And 勝出的 ETF 應該以綠色標示

  @UI @Chart
  Scenario: 績效走勢圖
    Then 應該顯示績效走勢圖:
      | 圖表類型       | 折線圖                              |
      | X 軸           | 時間軸 (月/年)                      |
      | Y 軸           | 累積報酬率 (%)                      |
      | 圖例           | 各 ETF 以不同顏色顯示               |
    And 支援以下互動:
      | 功能           | 說明                               |
      | 滑鼠懸停       | 顯示該時間點各 ETF 的報酬率         |
      | 縮放           | 可拖曳選取特定時間區間              |
      | 圖例點擊       | 顯示/隱藏特定 ETF 線條              |

  @UI @DrawdownChart
  Scenario: 回撤走勢圖
    Then 應該顯示回撤走勢圖:
      | 圖表類型       | 面積圖 (負值)                       |
      | X 軸           | 時間軸                              |
      | Y 軸           | 回撤幅度 (%)                        |
    And 用紅色標示最大回撤區間

  # ===== 進階分析 =====

  @UI @YearlyReturn
  Scenario: 年度報酬比較
    Then 應該顯示年度報酬表格:
      | 年份   | 0050    | VTI     |
      | 2023   | +26.3%  | +21.8%  |
      | 2022   | -21.8%  | -19.5%  |
      | 2021   | +18.5%  | +25.7%  |
      | ...    | ...     | ...     |
    And 每年勝出的 ETF 以綠色背景標示

  @UI @RollingReturn
  Scenario: 滾動報酬分析
    Then 應該顯示滾動報酬統計:
      | 統計項目              | 0050    | VTI     |
      | 任意 1 年正報酬機率   | 72%     | 75%     |
      | 任意 3 年正報酬機率   | 88%     | 91%     |
      | 任意 5 年正報酬機率   | 95%     | 97%     |
      | 任意 10 年正報酬機率  | 100%    | 100%    |

  # ===== RWD 響應式設計 =====

  @RWD @Mobile
  Scenario: 手機版回測介面
    Given 我用手機尺寸 (375x667) 瀏覽
    Then ETF 選擇器應該變成全寬下拉式
    And 圖表應該支援橫向捲動
    And 結果表格應該可左右滑動
    And 所有按鈕高度至少 48px (觸控友善)

  # ===== 錯誤處理 =====

  @Error @APIFailure
  Scenario: API 抓取失敗處理
    Given Yahoo Finance API 無法連線
    Then 應該顯示錯誤訊息 "無法取得 ETF 資料，請稍後再試"
    And 提供 "重新嘗試" 按鈕
    And 記錄錯誤日誌供後續分析

  @Error @InvalidETF
  Scenario: 無效的 ETF 代碼
    Given 使用者輸入不存在的 ETF 代碼
    Then 應該顯示 "找不到此 ETF，請確認代碼是否正確"

  @Error @PartialDataFailure
  Scenario: 部分 ETF 資料抓取失敗
    Given 使用者選擇 "0050", "VTI", "INVALID_ETF"
    When 點擊 "開始回測"
    Then 應該顯示警告 "部分 ETF 資料無法取得：INVALID_ETF"
    And 繼續顯示其他成功抓取的 ETF 回測結果
    And 失敗的 ETF 以灰色標示並顯示錯誤原因

  @Error @NetworkTimeout
  Scenario: 網路逾時處理
    Given API 請求超過 30 秒未回應
    Then 應該顯示 "請求逾時，請檢查網路連線"
    And 提供 "重新嘗試" 按鈕

  # ===== 載入狀態與使用者體驗 =====

  @UX @LoadingState
  Scenario: 回測計算中的載入狀態
    Given 使用者點擊 "開始回測"
    Then 應該顯示載入狀態:
      | 元素           | 說明                                    |
      | 載入動畫       | 顯示旋轉的 spinner 動畫                  |
      | 進度文字       | 顯示目前處理狀態，如 "正在抓取 VTI 資料..." |
      | 取消按鈕       | 可點擊取消本次回測                       |
    And "開始回測" 按鈕變為禁用狀態
    And 載入完成後自動平滑捲動至結果區

  @UX @LoadingProgress
  Scenario: 多 ETF 載入進度顯示
    Given 使用者選擇 5 檔 ETF 進行回測
    When 開始載入資料
    Then 應該顯示進度條和狀態:
      | 進度             | 狀態文字                    |
      | 0%               | 開始抓取資料...              |
      | 20%              | 正在抓取 0050 (1/5)...       |
      | 40%              | 正在抓取 VTI (2/5)...        |
      | 60%              | 正在抓取 VOO (3/5)...        |
      | 80%              | 正在計算回測結果...           |
      | 100%             | 完成！                       |

  @UX @SkeletonLoading
  Scenario: 骨架屏載入效果
    Given 頁面正在載入 ETF 清單
    Then 應該顯示骨架屏 (Skeleton) 佔位效果
    And 骨架屏的形狀與實際內容相同
    And 骨架屏有輕微的閃爍動畫

  # ===== 資料快取機制 =====

  @Backend @CacheStrategy
  Scenario: 記憶體快取策略
    Given 使用者查詢 "VTI" 10 年歷史資料
    Then 後端應該使用 IMemoryCache 快取結果
    And 快取鍵格式為 "etf_{symbol}_{period}_{interval}"
    And 快取有效期為 24 小時 (86400 秒)
    And 快取項目應設定優先級為 Normal

  @Backend @CacheHit
  Scenario: 快取命中
    Given "VTI" 10 年資料已在快取中
    When 使用者再次查詢 "VTI" 10 年資料
    Then 應該直接從快取回傳資料
    And 不應該發送 API 請求
    And 回應時間應小於 100ms

  @Backend @CacheMiss
  Scenario: 快取未命中
    Given "VTI" 資料不在快取中
    When 使用者查詢 "VTI" 10 年資料
    Then 應該發送 API 請求至 Yahoo Finance
    And 成功取得資料後存入快取
    And 下次查詢時可從快取取得

  @Backend @CacheInvalidation
  Scenario: 快取失效處理
    Given 快取中的資料已超過 24 小時
    When 使用者查詢該 ETF 資料
    Then 應該重新從 API 抓取最新資料
    And 更新快取內容

  # ===== 結果匯出功能 =====

  @Export @ExportOptions
  Scenario: 匯出回測結果選項
    Given 完成回測計算
    Then 應該顯示匯出按鈕包含以下選項:
      | 匯出格式       | 說明                               |
      | 下載 PNG       | 匯出績效走勢圖表為圖片              |
      | 複製連結       | 產生可分享的回測結果連結            |

  @Export @ExportPNG
  Scenario: 匯出圖表為 PNG
    Given 使用者點擊 "下載 PNG"
    Then 應該下載績效走勢圖的 PNG 圖片
    And 圖片解析度至少 1920x1080
    And 圖片包含所有圖例和標籤
    And 圖片背景為白色

  # ===== 分享功能 =====

  @Share @ShareLink
  Scenario: 產生分享連結
    Given 使用者完成 "0050 vs VTI" 10 年回測
    When 點擊 "複製連結" 按鈕
    Then 應該產生可分享的 URL
    And URL 格式如 "/calculator/backtest?etfs=0050,VTI&period=10y&mode=dca"
    And 顯示 "連結已複製到剪貼簿" 提示

  @Share @ShareLinkRestore
  Scenario: 從分享連結還原設定
    Given 使用者開啟分享連結 "/calculator/backtest?etfs=0050,VTI&period=10y"
    Then 應該自動還原回測設定:
      | 設定           | 值                    |
      | 選擇的 ETF     | 0050, VTI             |
      | 回測期間       | 10 年                 |
    And 自動開始回測並顯示結果

  # ===== 額外 ETF 支援 =====

  @ETF @BondETF
  Scenario: 債券 ETF 清單
    Then 應該支援以下債券 ETF:
      | 代碼     | 名稱                    | 市場   |
      | BND      | Vanguard 美國總債券      | 美股   |
      | BNDX     | Vanguard 國際債券        | 美股   |
      | TLT      | iShares 20年期美國公債   | 美股   |
      | 00679B   | 元大美債20年            | 台股   |
      | 00751B   | 元大AAA至A公司債        | 台股   |

  @ETF @ETFInfo
  Scenario: ETF 基本資訊顯示
    Given 使用者選擇 "VTI"
    Then 應該顯示 ETF 基本資訊:
      | 資訊           | VTI 範例                              |
      | 完整名稱       | Vanguard Total Stock Market ETF      |
      | 發行公司       | Vanguard                              |
      | 成立日期       | 2001-05-24                            |
      | 費用率         | 0.03%                                 |
      | 追蹤指數       | CRSP US Total Market Index           |

  # ===== 無障礙設計 (Accessibility) =====

  @A11y @ScreenReader
  Scenario: 螢幕閱讀器支援
    Then 頁面應該符合 WCAG 2.1 AA 標準:
      | 要求                 | 實作方式                              |
      | 圖片替代文字         | 所有圖表都有 aria-label 描述          |
      | 表單標籤             | 所有輸入欄位都有 label 元素           |
      | 焦點可見             | 鍵盤焦點有明顯的視覺指示              |
      | 顏色對比             | 文字與背景對比度至少 4.5:1           |

  @A11y @KeyboardNavigation
  Scenario: 鍵盤操作支援
    Then 使用者應該可以只用鍵盤操作:
      | 按鍵           | 功能                                  |
      | Tab            | 在表單元件間移動焦點                   |
      | Enter          | 確認選擇/提交表單                      |
      | Escape         | 關閉下拉選單/對話框                    |
      | 方向鍵         | 在下拉選單選項間移動                   |

  @A11y @ColorBlindFriendly
  Scenario: 色盲友善圖表
    Given 使用者啟用色盲友善模式
    Then 圖表應該使用色盲友善配色
    And 線條應該有不同的樣式 (實線/虛線/點線)
    And 資料點應該有不同的形狀 (圓形/方形/三角形)

  # ===== 效能需求 =====

  @Performance @ResponseTime
  Scenario: 回應時間需求
    Then 頁面效能應該符合以下標準:
      | 指標                   | 目標值                |
      | 首次內容繪製 (FCP)     | < 1.5 秒             |
      | 最大內容繪製 (LCP)     | < 2.5 秒             |
      | 首次輸入延遲 (FID)     | < 100 毫秒           |
      | 累積版面位移 (CLS)     | < 0.1                |

  @Performance @DataFetchTime
  Scenario: 資料抓取時間
    Given 使用者選擇 3 檔 ETF 進行 10 年回測
    Then 從點擊按鈕到顯示結果應在 5 秒內完成
    And 顯示結果後圖表渲染應在 1 秒內完成
