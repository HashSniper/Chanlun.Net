
var builder = WebApplication.CreateBuilder(args);

// 仅在非 IIS/生产环境下使用默认 URL（IIS 通过 web.config 绑定端口）
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_PORT")))
{
    builder.WebHost.UseUrls("http://localhost:5000");
}

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Content("""
<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8">
    <title>ChanlunX.API 服务状态</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif; max-width: 800px; margin: 60px auto; padding: 0 20px; color: #333; }
        h1 { color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px; }
        .status { display: inline-block; background: #2ecc71; color: white; padding: 6px 14px; border-radius: 20px; font-weight: 600; font-size: 14px; }
        table { width: 100%; border-collapse: collapse; margin-top: 30px; }
        th, td { text-align: left; padding: 12px; border-bottom: 1px solid #e0e0e0; }
        th { background: #f8f9fa; font-weight: 600; }
        tr:hover { background: #f8f9fa; }
        code { background: #f4f4f4; padding: 2px 6px; border-radius: 4px; font-family: Consolas, monospace; font-size: 13px; }
        .footer { margin-top: 40px; color: #888; font-size: 13px; text-align: center; }
    </style>
</head>
<body>
    <h1>ChanlunX.API <span class="status">运行中</span></h1>
    <p>缠论计算服务 API，提供笔、段、中枢等核心指标计算接口。</p>

    <table>
        <thead>
            <tr><th>端点</th><th>方法</th><th>说明</th></tr>
        </thead>
        <tbody>
            <tr><td><code>/api/calculation/bi1</code></td><td>POST</td><td>简笔顶底端点</td></tr>
            <tr><td><code>/api/calculation/bi2</code></td><td>POST</td><td>标准笔顶底端点</td></tr>
            <tr><td><code>/api/calculation/duan1</code></td><td>POST</td><td>段端点（标准画法）</td></tr>
            <tr><td><code>/api/calculation/duan2</code></td><td>POST</td><td>段端点（1+1 终结画法）</td></tr>
            <tr><td><code>/api/calculation/zs/high</code></td><td>POST</td><td>中枢高点</td></tr>
            <tr><td><code>/api/calculation/zs/low</code></td><td>POST</td><td>中枢低点</td></tr>
            <tr><td><code>/api/calculation/zs/signal</code></td><td>POST</td><td>中枢起终点信号</td></tr>
            <tr><td><code>/api/calculation/zs/direction</code></td><td>POST</td><td>中枢方向</td></tr>
            <tr><td><code>/api/calculation/zs/index</code></td><td>POST</td><td>同方向中枢序号</td></tr>
        </tbody>
    </table>

    <div class="footer">ChanlunX.CSharp &middot; .NET 10 Web API</div>
</body>
</html>
""", "text/html"));

app.Run();