using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stock.Data;
using Stock.Data.Entities;
using Stock.Service.Entitys;
using Stock.Service.Interface;

namespace Chanlun.API.Controllers;

/// <summary>
/// 交易管理 API —— 账户、持仓、交易记录、下单
/// </summary>
[ApiController]
[Route("api/trading")]
public class TradingController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ITradeService _tradeService;

    public TradingController(AppDbContext dbContext, ITradeService tradeService)
    {
        _dbContext = dbContext;
        _tradeService = tradeService;
    }

    #region 账户管理

    /// <summary>获取所有交易账户</summary>
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts(CancellationToken ct)
    {
        var accounts = await _dbContext.TradingAccounts
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
        return Ok(accounts);
    }

    /// <summary>获取单个账户详情（含持仓和交易记录数）</summary>
    [HttpGet("accounts/{id:long}")]
    public async Task<IActionResult> GetAccount(long id, CancellationToken ct)
    {
        var account = await _dbContext.TradingAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (account == null) return NotFound(new { error = "账户不存在" });

        var positionCount = await _dbContext.StockPositions.CountAsync(p => p.AccountId == id, ct);
        var recordCount = await _dbContext.StockTradeRecords.CountAsync(r => r.AccountId == id, ct);

        return Ok(new
        {
            account,
            positionCount,
            recordCount,
        });
    }

    /// <summary>创建交易账户</summary>
    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        var account = new TradingAccount
        {
            Name = request.Name,
            TotalBalance = request.InitialBalance,
            AvailableBalance = request.InitialBalance,
            FrozenBalance = 0,
            MarketValue = 0,
            TotalProfitLoss = 0,
        };

        _dbContext.TradingAccounts.Add(account);
        await _dbContext.SaveChangesAsync(ct);

        return Ok(account);
    }

    /// <summary>删除交易账户（级联删除持仓和交易记录）</summary>
    [HttpDelete("accounts/{id:long}")]
    public async Task<IActionResult> DeleteAccount(long id, CancellationToken ct)
    {
        var account = await _dbContext.TradingAccounts.FindAsync(new object[] { id }, ct);
        if (account == null) return NotFound(new { error = "账户不存在" });

        _dbContext.TradingAccounts.Remove(account);
        await _dbContext.SaveChangesAsync(ct);

        return Ok(new { message = "账户已删除" });
    }

    #endregion

    #region 持仓管理

    /// <summary>获取某账户的持仓列表</summary>
    [HttpGet("accounts/{accountId:long}/positions")]
    public async Task<IActionResult> GetPositions(long accountId, CancellationToken ct)
    {
        var positions = await _dbContext.StockPositions
            .AsNoTracking()
            .Where(p => p.AccountId == accountId)
            .OrderByDescending(p => p.TotalCost)
            .ToListAsync(ct);

        return Ok(positions);
    }

    #endregion

    #region 交易记录

    /// <summary>获取某账户的交易记录</summary>
    [HttpGet("accounts/{accountId:long}/records")]
    public async Task<IActionResult> GetRecords(long accountId, [FromQuery] int limit = 100, CancellationToken ct = default)
    {
        var records = await _dbContext.StockTradeRecords
            .AsNoTracking()
            .Where(r => r.AccountId == accountId)
            .OrderByDescending(r => r.TradeTime)
            .Take(limit)
            .ToListAsync(ct);

        return Ok(records);
    }

    /// <summary>获取所有交易记录（报表用）</summary>
    [HttpGet("records")]
    public async Task<IActionResult> GetAllRecords([FromQuery] int limit = 1000, CancellationToken ct = default)
    {
        var records = await _dbContext.StockTradeRecords
            .AsNoTracking()
            .OrderByDescending(r => r.TradeTime)
            .Take(limit)
            .ToListAsync(ct);

        return Ok(records);
    }

    #endregion

    #region 交易执行

    /// <summary>执行交易（买入或卖出）</summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteTrade([FromBody] ExecuteTradeRequest request, CancellationToken ct)
    {
        var record = await _tradeService.ExecuteTradeAsync(request, ct);
        return Ok(record);
    }

    /// <summary>T+1 结算：将昨日买入的不可用持仓变为可用</summary>
    [HttpPost("settlement/{accountId:long}")]
    public async Task<IActionResult> Settlement(long accountId, CancellationToken ct)
    {
        await _tradeService.SettlementAsync(accountId, ct);
        return Ok(new { message = "T+1 结算完成" });
    }

    #endregion
}

/// <summary>创建账户请求</summary>
public class CreateAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
}
