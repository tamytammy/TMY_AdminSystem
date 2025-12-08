# TMY Admin System - 行政管理系統 

這是一個運用 **ASP.NET Web Forms** 開發的企業內部管理系統。專案藉由過往在職相關經驗出發去探索相關企業需求，包含員工權限管理、公告發布系統以及多層級公文簽核流程。

本專案展示了從**資料庫設計**、**後端邏輯開發**、**前端介面實作**到**Azure 雲端部署**的全端開發技能。

## 線上 Demo
* **Live Demo (Azure App Service):** [點擊前往系統](https://tmy-dbecgpekeaabckd6.eastasia-01.azurewebsites.net/)
* **測試帳號:**
    * 一般員工: `hr001` / `hr001` 
    * 人資主管: `hr002` / `hr002` 

---

## 🛠️ 使用技術 (Tech Stack)

### Backend & Database
* **Framework:** .NET Framework 4.0 / ASP.NET Web Forms
* **Language:** C#
* **Database:** MS SQL Server / Azure SQL Database / SSMS管理

### Frontend
* **UI Framework:** Bootstrap 5 (RWD 響應式設計)
* **Third-party Components:** CKEditor 4 (LTS)

### DevOps & Tools
* **Version Control:** Git / GitHub
* **Cloud Platform:** Microsoft Azure App Service
* **IDE:** Visual Studio 2022

---

## ✨ 主要功能 (Key Features)

### 1. 員工與權限管理 (RBAC)
* 實作 **Role-Based Access Control**，區分 Admin、Manager、User 權限。
* 透過後端 SQL 強制過濾部門資料，防止越權查詢。
* 員工資料的 CRUD 與關鍵字模糊搜尋。

### 2. 公告發布系統
* 整合 **CKEditor 4** 實現文本編輯，並解決 ASP.NET `RequestValidation` 安全性問題。
* 實作公告狀態管理（草稿、已發布、排程發布）。
* 首頁彈跳視窗 (Modal) 顯示最新公告，提升使用者體驗。

### 3. 公文簽核流程 (開發中)
* 設計 **Flow_Forms** 與 **Flow_Logs** 資料表架構，完整記錄簽核軌跡。
* 實作 **層級簽核 (Hierarchy Approval)**：
    * 系統自動判斷單據金額/類別，決定是否需要更高層級主管核准。
* 實作 **跨部門轉單邏輯**：
    * 例如：HR 申請資訊設備 -> 直屬主管同意 -> 自動轉單至資訊部主管。

---


##  作者資訊 

### 開發者：何怡德 Tammy
### Email：momo09041027@gamil.com
* 歡迎來信，我一定會看！


---

