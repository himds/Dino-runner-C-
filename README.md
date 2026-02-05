## Dino Runner

基于 ASP.NET Core Web API + 前端小游戏的示例项目，包含用户、分数、排行榜、成就与商城。

### 运行
- 安装 .NET 8 SDK
- 在项目根目录执行：
  - `dotnet restore`（如遇证书/网络问题需先配置 NuGet 代理或证书）
  - `dotnet ef migrations add InitialCreate`（首次建库，需安装 dotnet-ef 全局工具）
  - `dotnet ef database update`
  - `dotnet run`
- 运行后访问 `http://localhost:5000` 或 `https://localhost:5001` 打开内置前端。

### 主要 API
- `POST /api/users` 创建用户 `{ username }`
- `GET /api/users/{id}` 用户信息
- `POST /api/scores` 上传分数 `{ userId, value }`，自动加金币与判定成就
- `GET /api/scores/top?limit=20` 全局排行榜
- `GET /api/scores/user/{userId}` 玩家历史分数
- `GET /api/achievements` 成就定义
/- `GET /api/achievements/user/{userId}` 玩家已解锁成就
- `GET /api/shop/items` 商城列表
- `POST /api/shop/purchase` 购买 `{ userId, shopItemId }`
- `GET /api/shop/user/{userId}` 玩家已购商品

### 游戏规则（默认）
- 上传分数时奖励金币：`value / 10`
- 成就：
  - `score>=1000`：单局达到 1000 分
  - `games>=10`：完成 10 局
- 商城初始物品：Double Jump / Slow Speed / Shield

### 前端
- 内置于 `wwwroot/index.html`，简单 Canvas 跑酷，空格跳跃，结束自动上传分数并刷新排行榜/成就/商城。
