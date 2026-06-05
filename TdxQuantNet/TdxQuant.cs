using System.Runtime.InteropServices;
using System.Text.Json;
using TdxQuantNet.Models;

namespace TdxQuantNet;

public partial class TdxQuant : IDisposable
{
    private IntPtr _mainDllModule;
    private int _runId;
    private int _runMode = -1;
    private bool _disposed;
    private string _tqName;
    private string _tdxPath;
    
    public TdxQuant(string tqName, string tdxPath)
    {
        _tqName = tqName;
        _tdxPath = tdxPath;
        
    }
    
    public void Dispose()
    {
        // ReSharper disable once InvertIf
        if (!_disposed)
        {
            _ = CloseConnect(_runId, -1);
            _ = FreeLibrary(_mainDllModule);
            _mainDllModule = IntPtr.Zero;
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    public bool Init()
    {
        var dllPath = Path.Join(_tdxPath, @"\PYPlugins");
        var mainDllPath = Path.Combine(dllPath, "TPythClient.dll");
        var dirSet = SetDllDirectory(dllPath);
        if (!dirSet) throw new Exception("SetDllDirectory failed");

        _mainDllModule = LoadLibrary(mainDllPath);
        if (_mainDllModule <= IntPtr.Zero)
        {
            var err = Marshal.GetLastWin32Error();
            throw new Exception($"LoadLibrary failed: code {err}");
        }
        
        var ptr = InitConnect(_tqName, mainDllPath, _runMode,314, false);
        var str = Marshal.PtrToStringUTF8(ptr);
        if (str is null) throw new Exception("TQ InitConnect failed");
        var response = JsonSerializer.Deserialize<CallInitConnectModel>(str, Helper.JsonSerializerOptions);
        if (response is null) throw new Exception("TQ InitConnect failed");

        if (response.ErrorId is not (0 or 12))
        {
            return false;
            //throw new Exception($"TQ InitConnect failed: {response.Error}");
        }

     
        _runId = response.RunId;
        return true;
    }

    [DllImport("kernel32", EntryPoint = "SetDllDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetDllDirectory(string lpPathName);

    [DllImport("kernel32", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpLibFileName);

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeLibrary(IntPtr hModule);
}

// init
public partial class TdxQuant
{
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "InitConnect")]
    private static extern IntPtr InitConnect(string tqName, string dllPath, int mode, int version,
        [MarshalAs(UnmanagedType.Bool)] bool reInit);

    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "CloseConnect")]
    private static extern IntPtr CloseConnect(int runId, int runMode);
}

// 行情类信息
public partial class TdxQuant
{
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetHISDATsInStr")]
    private static extern IntPtr GetHisdaTsInStr(int runId, string stockCode, string startDate, string endStart,
        string period, int dividendType, int count, int timeout);

    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetREPORTInStr")]
    private static extern IntPtr GetReportInStr(int runId, string stockCode, int timeout);

    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetSTOCKInStr")]
    private static extern IntPtr GetStockInStr(int runId, string stockCode, int timeout);

    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetMoreInfoInStr")]
    private static extern IntPtr GetMoreInfoInStr(int runId, string stockCode, int timeout);
    
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetCWDATAInStr")]
    private static extern IntPtr GetCwDataInStr(int runId, string stockCode, string startDate, string endStart, int timeout);
    
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetIPOINFOInStr")]
    private static extern IntPtr GetIpoInfoInStr(int runId, int ipoType, int ipoDate, int timeout);
    
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetGbInfoInStr")]
    private static extern IntPtr GetGbInfoInStr(int runId, string json, int timeout);
    
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetGPBlockInStr")]
    private static extern IntPtr GetGpBlockInStr(int runId, string code, int timeout);
}

// 财务类数据
public partial class TdxQuant
{
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetProDataInStr")]
    private static extern IntPtr GetProDataInStr(int runId, string json, int timeout);
}

// 分类/板块成份股
public partial class TdxQuant
{
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetStockListInStr")]
    private static extern IntPtr GetStockListInStr(int runId, string market, int listType, int timeout);

    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetBlockListInStr")]
    private static extern IntPtr GetBlockListInStr(int runId, int listType, int timeout);

    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetBlockStocksInStr")]
    private static extern IntPtr GetBlockStocksInStr(int runId, string code, int listType, int timeout);
}

// 自选股/自定义板块
public partial class TdxQuant
{
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetUserBlockInStr")]
    private static extern IntPtr GetUserBlockInStr(int runId, int timeout);
    
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "SetResToMain")]
    private static extern IntPtr SetResToMain(int runId, int mode, string data, int timeout);
    
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "UserBlockControl")]
    private static extern IntPtr UserBlockControl(int runId, int mode, string code, string name, int timeout);
}

// ETF/可转债/期货数据
public partial class TdxQuant
{
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetTrackZsETFInfoInStr")]
    private static extern IntPtr GetTrackZsEtfInfoInStr(int runId, string code, int timeout);
    
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetCBINFOInStr")]
    private static extern IntPtr GetCbInfoInStr(int runId, string code, int timeout);
}

// 调用通达信公式
public partial class TdxQuant
{
    
}

// 交易函数
public partial class TdxQuant
{
    [DllImport("TPythClient.dll", CharSet = CharSet.Ansi, EntryPoint = "GetOrderStr")]
    private static extern IntPtr GetOrderStr(int runId, string json, int timeout);
}
