
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 仅在非 IIS/生产环境下使用默认 URL（IIS 通过 web.config 绑定端口）
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_PORT")))
{
    builder.WebHost.UseUrls("http://localhost:5000");
}

builder.Services.AddControllers();

// 添加 Swagger/OpenAPI 服务，并读取 XML 注释
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// 启用 Swagger 和 Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ChanlunX.API V1");
    options.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Content("""
<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8">
    <title>ChanlunX.API 服务状态</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif; max-width: 900px; margin: 60px auto; padding: 0 20px; color: #333; }
        h1 { color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px; }
        .status { display: inline-block; background: #2ecc71; color: white; padding: 6px 14px; border-radius: 20px; font-weight: 600; font-size: 14px; }
        table { width: 100%; border-collapse: collapse; margin-top: 30px; }
        th, td { text-align: left; padding: 12px; border-bottom: 1px solid #e0e0e0; }
        th { background: #f8f9fa; font-weight: 600; }
        tr:hover { background: #f8f9fa; }
        code { background: #f4f4f4; padding: 2px 6px; border-radius: 4px; font-family: Consolas, monospace; font-size: 13px; }
        .footer { margin-top: 40px; color: #888; font-size: 13px; text-align: center; }
        .swagger-link { display: inline-block; margin-top: 20px; background: #3498db; color: white; padding: 10px 20px; border-radius: 6px; text-decoration: none; font-weight: 500; }
        .swagger-link:hover { background: #2980b9; }
    </style>
</head>
<body>
    <h1>ChanlunX.API <span class="status">运行中</span></h1>
    <p>缠论计算服务 API，提供笔、段、中枢等核心指标计算接口。</p>
    <a class="swagger-link" href="/swagger/index.html">📘 查看 Swagger 接口文档（自动从代码注释生成）</a>

    <table>
        <thead>
            <tr><th>端点</th><th>方法</th><th>说明</th></tr>
        </thead>
        <tbody>
            <tr><td><code>/api/calculation/setstocktime</code></td><td>POST</td><td>生成key 已经记录所有的k 线时间</td></tr>
            <tr><td><code>/api/calculation/createchan</code></td><td>POST</td><td>将缠论所有的信息都处理</td></tr>
            <tr><td><code>/api/calculation/bilist</code></td><td>POST</td><td>获取笔相关</td></tr>
            <tr><td><code>/api/calculation/seglist</code></td><td>POST</td><td>获取线段</td></tr>
            <tr><td><code>/api/calculation/getsegpivotzg</code></td><td>POST</td><td>获取中枢高点（线段中枢）</td></tr>
            <tr><td><code>/api/calculation/getsegpivotzd</code></td><td>POST</td><td>获取中枢低点（线段中枢）</td></tr>
            <tr><td><code>/api/calculation/getsegpivotrange</code></td><td>POST</td><td>获取笔中枢起始点（线段中枢范围）</td></tr>
            <tr><td><code>/api/calculation/klineg</code></td><td>POST</td><td>合并后的k线的高点</td></tr>
            <tr><td><code>/api/calculation/klined</code></td><td>POST</td><td>合并后的k线的低点</td></tr>
            <tr><td><code>/api/calculation/klinerange</code></td><td>POST</td><td>合并后的k线的起始点</td></tr>
            <tr><td><code>/api/calculation/setmacd</code></td><td>POST</td><td>设置MACD</td></tr>
            <tr><td><code>/api/calculation/getbipivotzg</code></td><td>POST</td><td>获取中枢高点（笔中枢）</td></tr>
            <tr><td><code>/api/calculation/getbipivotzd</code></td><td>POST</td><td>获取中枢低点（笔中枢）</td></tr>
            <tr><td><code>/api/calculation/getbipivotrange</code></td><td>POST</td><td>获取笔中枢起始点</td></tr>
        </tbody>
    </table>

    <div class="footer">ChanlunX.CSharp &middot; .NET 10 Web API</div>
</body>
</html>
""", "text/html"));

app.Run();
