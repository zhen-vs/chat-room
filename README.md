這是為 Work Exchange 媒合平台 開發的核心溝通模組，實作了雇主與求職者之間的即時對話功能。

核心功能實作
SignalR 即時雙向通訊：捨棄傳統的輪詢，採用 WebSocket 協議實作低延遲的即時訊息傳輸。

多房間管理邏輯：實作特定的 GroupId 隔離機制，確保對話僅存在於特定的媒合對象之間，互不干擾。

使用者狀態監控：實作 OnConnectedAsync 與 OnDisconnectedAsync 邏輯，即時掌握使用者在線狀態。

歷史訊息串接：結合 Entity Framework Core 於初始化連線時自動載入該對話紀錄。

使用技術
後端：C# / .NET API / SignalR Hubs

前端：Vue.js / SignalR Client SDK

資料庫：SQL Server (EF Core 實作資料持久化)

專案亮點
針對訊息傳輸實作了「斷線自動重連」機制，提升通訊穩定度。

效能優化：採用 DTO模式封裝傳輸物件，僅傳送必要欄位，減少網路頻寬負荷。

異步處理 (Async/Await)：全功能採用非同步程式設計，確保伺服器在高併發連線時仍能保持高效能運作。
