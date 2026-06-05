# TradingView Charting Library 集成模板

## 前置要求
1. 从 TradingView 获取 Charting Library 授权并下载
2. 将 `charting_library` 文件夹放置在本目录同级

## 目录结构
```
tv-charting-library/
├── index.html
├── datafeed.js
├── README.md
└── charting_library/   # ← 放置TradingView库文件
```

## 快速开始
1. 放置 Library 文件到 `charting_library/`
2. 启动 API 服务：`dotnet run --project Chanlun.API`
3. 推送 K 线数据到 API 缓存（首次使用）
4. 访问 `http://localhost:5000/tv-charting-library/index.html`

## 数据流
Charting Library → UDF DataFeed → ChanLun.API → 自动叠加缠论笔/线段/中枢
