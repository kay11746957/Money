系統架構概述（High-Level Vibes）

後端：ASP.NET Core MVC
前端：vue.js + Razor Views + Chart.js（CDN），RWD 用 Bootstrap 5。
計算邏輯：純服務層，複利公式每月計算一次（精準到小數）。
驗證：Model Binding + DataAnnotation，即時前端 JS 驗證。
圖表：Chart.js 折線圖，顯示每年資產成長（藍色曲線，tooltip 顯示金額）。
部署：github
安全性：無使用者資料，僅計算不存檔。
效能：計算 < 50ms，支援 1~50 年（極端值如 50 年 100k 月投也 OK）。