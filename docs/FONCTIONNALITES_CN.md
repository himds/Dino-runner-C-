# Dino Runner — 功能实现说明（中文）

本文档说明本项目中各功能在**架构层面**如何实现：前后端如何协作、数据如何持久化、关键文件位于何处。

---

## 1. 技术栈与整体结构

| 层级 | 技术 | 作用 |
|------|------|------|
| 后端 | ASP.NET Core 8、MVC 控制器 | 提供 `/api/*` JSON 接口 |
| ORM | Entity Framework Core 8 + SQLite | 映射实体、读写 `dino.db` |
| 前端 | 静态 HTML/CSS/JS（`wwwroot`） | 无打包工具，页面内联或通过 `fetch` 调 API |
| 本地会话 | `localStorage` 键 `dino_user` | 保存 `{ id, coins }`，**无服务端 Cookie/Session** |

入口：`Program.cs` — 注册控制器、数据库、CORS、Swagger（开发环境）、静态文件；启动时用 `EnsureCreated()` 创建库表并应用种子数据。API 路由在 `MapControllers()` 中注册，且**先于** `MapFallbackToFile("index.html")`，避免 `/api` 请求被前端回退路由拦截。

---

## 2. 用户注册与登录

**实现位置：** `AuthController.cs`、`login.html`、`Models/User.cs`

- **注册** `POST /api/auth/register`：检查用户名是否已存在；新建 `User`，密码经 `SHA256(用户名 + ":" + 密码)` 十六进制字符串写入 `PasswordHash`，**不明文存密码**。
- **登录** `POST /api/auth/login`：按用户名查用户，用相同规则计算哈希比对；成功返回 `Id`、`Username`、`Coins`。
- 前端在成功后将 `{ id, coins }` 写入 `localStorage.setItem('dino_user', ...)`，各页面读取后决定是否跳转登录页。

**错误信息：** 接口以 JSON 返回 `message` 字段（法语），供页面 `showError` 等直接展示。

---

## 3. 金币（Pièces）

**实现位置：** `User.Coins` 字段、`ScoresController`、`ShopController`、`index.html` / `shop.html`

- **账户金币：** 存储在 `Users` 表的 `Coins` 列；注册后默认为 0（见 `AppDbContext` 中默认值配置）。
- **对局内拾取：** 游戏画布上生成可拾取的金币，碰撞后增加本局计数 `runCoins`；**一局结束**时通过 `POST /api/scores` 的 body 字段 `coinsCollected` 上报，服务端将其**累加**到 `user.Coins`（与「按分数换算金币」不同，本项目以**前端上报的拾取数**为准）。
- **消费：** 商城购买成功后从 `user.Coins` 扣除 `ShopItem.Price`。

---

## 4. 分数提交、排行榜与历史记录

**实现位置：** `ScoresController.cs`、`Dtos/ScoreSubmitRequest.cs`、`Dtos/ScoreSubmitResponse.cs`

- **提交分数** `POST /api/scores`（路由到控制器的 `SubmitScore`）：校验用户存在 → 写入 `Scores` 表一条记录 → 按 `CoinsCollected` 增加用户金币 → 调用成就判定 → 返回 `ScoreSubmitResponse`（含 `NewCoins`、`TotalCoins`、`UnlockedAchievementIds` 等）。
- **排行榜** `GET /api/scores/top?limit=`：按分数降序、时间升序，默认最多 20 条，`limit` 限制在 1～100。
- **某用户历史** `GET /api/scores/user/{userId}`：按时间倒序返回该用户所有分数记录。

---

## 5. 成就系统

**实现位置：** `AppDbContext` 种子中的 `Achievement`、`UserAchievement`、`ScoresController.EvaluateAchievements`

- **成就定义：** 存在 `Achievements` 表；种子中配置 `Condition` 字符串，例如 `score>=1000`、`games>=10`。
- **解锁时机：** 在每次成功提交分数并保存分数记录之后执行；对每条未解锁成就解析 `Condition`：
  - `score>=X`：用**本局**分数 `request.Value` 与阈值比较；
  - `games>=X`：用该用户已有分数记录条数（`user.Scores.Count`，此时已包含刚写入的一局）与阈值比较。
- **解锁记录：** 写入 `UserAchievements`（用户、成就、解锁时间）。
- **前端：** `achievements.html` 拉取全部成就定义 + 某用户已解锁列表，合并后展示锁定/已解锁状态。

---

## 6. 商城与道具购买

**实现位置：** `ShopController.cs`、`ShopItem` / `UserItem` 实体、`shop.html`、`AppDbContext` 种子

- **商品列表** `GET /api/shop/items`：返回所有 `ShopItem`（名称、描述、价格等）。
- **已购列表** `GET /api/shop/user/{userId}`：返回该用户的 `UserItem` 摘要（含 `shopItemId`），供前端判断是否拥有某道具。
- **购买** `POST /api/shop/purchase`：校验用户、商品存在；若已拥有则冲突；若金币不足则 400；否则扣费并插入 `UserItem`。

种子中商品 ID 与**游戏内效果**的约定（在 `index.html` 中硬编码）：

| ShopItemId | 游戏内效果（概览） |
|------------|-------------------|
| 1 | 二段跳（空格，空中可再跳一次） |
| 2 | 障碍移动速度乘以约 0.7 |
| 3 | 护盾：一次碰撞消耗护盾并消除该障碍，不结束游戏 |
| 4 | 种子中有「分数加成」类描述；**当前 `index.html` 主循环未根据 id 4 实现按键或翻倍逻辑**，购买后仅作收藏/展示，除非后续扩展 |

---

## 7. 游戏主循环（Canvas）

**实现位置：** `wwwroot/index.html` 内联脚本

- 使用 **Canvas 2D** 绘制背景、地面、恐龙矩形、障碍物与轨道金币。
- **主循环** `requestAnimationFrame(loop)`：每帧递增 `frame` 与 `score`；按帧间隔生成障碍；用 `speedReduction` 调整整体速度；检测恐龙与障碍 AABB 碰撞；护盾逻辑见上。
- **跳跃：** 监听 `Space`；若购买 id 1，则 `maxJumps = 2`。
- **结束一局：** `gameOver()` 停止循环，调用 `POST /api/scores` 上传 `userId`、`value`（分数）、`coinsCollected`（本局拾取），再更新 `localStorage` 中的 `coins` 并 `loadOwnedItems()` 刷新护盾等状态。

可选模块 `wwwroot/item.js` 将「加载已购道具 → `gameState`」抽成 ES 模块；**当前主游戏页使用内联逻辑**，与 `item.js` 并行存在，便于以后统一重构。

---

## 8. 数据库与种子数据

**实现位置：** `Data/AppDbContext.cs`、`dino.db`（连接串在 `appsettings.json`）

- 使用 `EnsureCreated()`：**无 EF 迁移文件**时由运行时创建表结构；`HasData` 提供成就与商城商品的初始行。
- 修改种子后，若本地库已存在，**不会自动更新已有行**；开发中常通过删除 `dino.db` 重建（会清空用户与购买数据）。

---

## 9. CORS 与 API 文档

- **CORS：** `Program.cs` 中策略 `AllowAll`，便于本地静态页任意端口访问 API（仅建议开发环境）。
- **Swagger：** 开发环境下 `/swagger` 浏览 OpenAPI，便于调试控制器。

---

## 10. 主要文件索引

| 功能 | 后端 | 前端 |
|------|------|------|
| 注册/登录 | `AuthController.cs` | `login.html` |
| 游戏 | `ScoresController.cs` | `index.html` |
| 排行榜/历史 | `ScoresController.cs` | 按需调用 API |
| 成就 | `AchievementsController.cs` + `ScoresController` 内判定 | `achievements.html` |
| 商城 | `ShopController.cs` | `shop.html` |
| 样式 | — | `style.css` |

---

*文档版本与代码库同步撰写；若实现变更，请以实际代码为准。*
