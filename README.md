# LinqORM

LinqORM 是一個以 C# 開發的輕量級 ORM (Object-Relational Mapping) 框架。
本專案的主要目的是探索並實作類似 Entity Framework 的底層運作機制，透過解析 C# LINQ 表達式樹 (Expression Tree) 將其動態轉換為 SQL 語法，並利用反射 (Reflection) 達成資料庫與 C# 物件之間的自動映射。

## ✨ 核心特色 (Features)

* **LINQ to SQL 翻譯**：自訂 `SQLExpressionVisitor` 解析 `Expression<Func<T, bool>>`，支援常數、二元運算 (And, Or, >=, <= 等) 以及常用方法 (如 `.Contains()` 轉為 `LIKE`) 轉換成 SQL WHERE 條件。
* **動態資料映射 (Reflection Mapping)**：執行查詢後，透過 `SQLEnumerator` 與反射機制，動態將 `SqlDataReader` 的欄位對應並賦值給 C# 泛型模型 (POCO)。
* **延遲執行 (Deferred Execution)**：實作 `IEnumerable`，在串接 `Where` 或 `Select` 條件時僅組裝 SQL 語法，直到真正呼叫迭代器 (如 `ToList()`) 時才向資料庫發起連線查詢。
* **動態 CRUD 操作**：提供基於物件狀態的 `AddData`, `UpdateData`, `DeleteData` 方法，並能自動查詢系統表 (`INFORMATION_SCHEMA`) 以識別 Primary Key 作為更新與刪除的依據。

## 📂 專案結構 (Architecture)

* `DatabaseContext.cs` / `SQLHelper.cs`：負責資料庫連線管理、SQL 指令執行與泛型 CRUD 操作。
* `DBSet<T>.cs`：模擬 EF 的 DbSet，負責存放資料表定義並提供 `Where`、`Select` 等 LINQ 方法入口。
* `SQLExpressionVisitor.cs`：核心解析器，負責遍歷 C# 語法樹並組裝成對應的 SQL 語句。
* `SQLEnumerator.cs`：實作 `IEnumerator`，負責逐筆讀取 `SqlDataReader` 並實例化泛型物件。
* `Extensions.cs`：提供 `IEnumerable<T>` 的擴充方法 (如自訂的 `ToList()`)。

## 🚀 快速開始 (Getting Started)

### 1. 環境需求
* .NET Framework 4.7.2 或以上
* MS SQL Server (預設使用 LocalDB 或 SQLExpress)

### 2. 資料庫配置
請確保你的 SQL Server 中有一個名為 `misDB` 的資料庫，並且擁有 `Employee` 資料表（包含 `EmpId`, `EmpName`, `JobTitle`, `MonthSalary` 欄位）。
連線字串可在 `Program.cs` 中修改：

```csharp
string connection_string = "Persist Security Info=False;Integrated Security=true; Initial Catalog=misDB;Server=YOUR_SERVER_NAME;Encrypt=True;TrustServerCertificate=true;";
