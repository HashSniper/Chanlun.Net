using NativeAdapter;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace NativeAdapter;

public static unsafe partial class NativeExports
{
    // 函数指针委托类型，匹配通达信的 cdecl 调用约定
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PluginFuncDelegate(int nCount, float* pOut, float* a, float* b, float* c);

    //=========================================================================
    // 通用辅助：在线程池中执行 HTTP 调用，避免在 UnmanagedCallersOnly
    // 的回调线程上同步阻塞（NativeAOT 中这会导致 GC/线程崩溃）。
    // 同时捕获所有异常，确保不会将托管异常抛到非托管代码中。
    //=========================================================================
    private static float[] SafeCallApi<TRequest>(string endpoint, TRequest request, JsonTypeInfo<TRequest> requestTypeInfo)
        where TRequest : class
    {
        try
        {
            // 在线程池中执行，避免阻塞从非托管进入的线程
            return Task.Run(() => ApiClient.CallApi(endpoint, request, requestTypeInfo)).GetAwaiter().GetResult();
        }
        catch(Exception ex)
        {
            return [];
        }
    }

    private static void CopyToOutput(int nCount, float* pOut, float[]? source)
    {
        if (source == null || pOut == null)
            return;
        int len = Math.Min(nCount, source.Length);
        for (int i = 0; i < len; i++)
            pOut[i] = source[i];
        // 剩余位置保持通达信传入时的默认值（通常为 0）
    }

    //=========================================================================
    // 输出函数1号：输出简笔顶底端点
    //=========================================================================
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "Func1")]
    public static void Func1(int nCount, float* pOut, float* pHigh, float* pLow, float* pIgnore)
    {
        if (nCount <= 0 || pOut == null || pHigh == null || pLow == null) return;

        float[] high = new float[nCount];
        float[] low = new float[nCount];
        for (int i = 0; i < nCount; i++)
        {
            high[i] = pHigh[i];
            low[i] = pLow[i];
        }
        var request = new BiRequest { NCount = nCount, High = high, Low = low };
        float[] outArr = SafeCallApi("/api/calculation/bi1", request, ApiJsonContext.Default.BiRequest);
        CopyToOutput(nCount, pOut, outArr);
    }

    //=========================================================================
    // 输出函数2号：输出标准笔顶底端点
    //=========================================================================
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "Func2")]
    public static void Func2(int nCount, float* pOut, float* pHigh, float* pLow, float* pIgnore)
    {
        if (nCount <= 0 || pOut == null || pHigh == null || pLow == null) return;

        float[] high = new float[nCount];
        float[] low = new float[nCount];
        for (int i = 0; i < nCount; i++)
        {
            high[i] = pHigh[i];
            low[i] = pLow[i];
        }
        var request = new BiRequest { NCount = nCount, High = high, Low = low };
        float[] outArr = SafeCallApi("/api/calculation/bi2", request, ApiJsonContext.Default.BiRequest);
        CopyToOutput(nCount, pOut, outArr);
    }

    //=========================================================================
    // 输出函数3号：输出段的端点标准画法
    //=========================================================================
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "Func3")]
    public static void Func3(int nCount, float* pOut, float* pIn, float* pHigh, float* pLow)
    {
        if (nCount <= 0 || pOut == null || pIn == null || pHigh == null || pLow == null) return;

        float[] bi = new float[nCount];
        float[] high = new float[nCount];
        float[] low = new float[nCount];
        for (int i = 0; i < nCount; i++)
        {
            bi[i] = pIn[i];
            high[i] = pHigh[i];
            low[i] = pLow[i];
        }
        var request = new DuanRequest { NCount = nCount, Bi = bi, High = high, Low = low };
        float[] outArr = SafeCallApi("/api/calculation/duan1", request, ApiJsonContext.Default.DuanRequest);
        CopyToOutput(nCount, pOut, outArr);
    }

    //=========================================================================
    // 输出函数4号：输出段的端点1+1终结画法
    //=========================================================================
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "Func4")]
    public static void Func4(int nCount, float* pOut, float* pIn, float* pHigh, float* pLow)
    {
        if (nCount <= 0 || pOut == null || pIn == null || pHigh == null || pLow == null) return;

        float[] bi = new float[nCount];
        float[] high = new float[nCount];
        float[] low = new float[nCount];
        for (int i = 0; i < nCount; i++)
        {
            bi[i] = pIn[i];
            high[i] = pHigh[i];
            low[i] = pLow[i];
        }
        var request = new DuanRequest { NCount = nCount, Bi = bi, High = high, Low = low };
        float[] outArr = SafeCallApi("/api/calculation/duan2", request, ApiJsonContext.Default.DuanRequest);
        CopyToOutput(nCount, pOut, outArr);
    }

    //=========================================================================
    // 输出函数5号：中枢高点数据
    //=========================================================================
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "Func5")]
    public static void Func5(int nCount, float* pOut, float* pIn, float* pHigh, float* pLow)
    {
        if (nCount <= 0 || pOut == null || pIn == null || pHigh == null || pLow == null) return;

        float[] bi = new float[nCount];
        float[] high = new float[nCount];
        float[] low = new float[nCount];
        for (int i = 0; i < nCount; i++)
        {
            bi[i] = pIn[i];
            high[i] = pHigh[i];
            low[i] = pLow[i];
        }
        var request = new ZsRequest { NCount = nCount, Bi = bi, High = high, Low = low };
        float[] outArr = SafeCallApi("/api/calculation/zs/high", request, ApiJsonContext.Default.ZsRequest);
        CopyToOutput(nCount, pOut, outArr);
    }

    //=========================================================================
    // 输出函数6号：中枢低点数据
    //=========================================================================
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "Func6")]
    public static void Func6(int nCount, float* pOut, float* pIn, float* pHigh, float* pLow)
    {
        if (nCount <= 0 || pOut == null || pIn == null || pHigh == null || pLow == null) return;

        float[] bi = new float[nCount];
        float[] high = new float[nCount];
        float[] low = new float[nCount];
        for (int i = 0; i < nCount; i++)
        {
            bi[i] = pIn[i];
            high[i] = pHigh[i];
            low[i] = pLow[i];
        }
        var request = new ZsRequest { NCount = nCount, Bi = bi, High = high, Low = low };
        float[] outArr = SafeCallApi("/api/calculation/zs/low", request, ApiJsonContext.Default.ZsRequest);
        CopyToOutput(nCount, pOut, outArr);
    }

    //=========================================================================
    // 输出函数7号：中枢起点、终点信号
    //=========================================================================
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "Func7")]
    public static void Func7(int nCount, float* pOut, float* pIn, float* pHigh, float* pLow)
    {
        if (nCount <= 0 || pOut == null || pIn == null || pHigh == null || pLow == null) return;

        float[] bi = new float[nCount];
        float[] high = new float[nCount];
        float[] low = new float[nCount];
        for (int i = 0; i < nCount; i++)
        {
            bi[i] = pIn[i];
            high[i] = pHigh[i];
            low[i] = pLow[i];
        }
        var request = new ZsRequest { NCount = nCount, Bi = bi, High = high, Low = low };
        float[] outArr = SafeCallApi("/api/calculation/zs/signal", request, ApiJsonContext.Default.ZsRequest);
        CopyToOutput(nCount, pOut, outArr);
    }

    //=========================================================================
    // 输出函数8号：中枢方向数据
    //=========================================================================
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "Func8")]
    public static void Func8(int nCount, float* pOut, float* pIn, float* pHigh, float* pLow)
    {
        if (nCount <= 0 || pOut == null || pIn == null || pHigh == null || pLow == null) return;

        float[] bi = new float[nCount];
        float[] high = new float[nCount];
        float[] low = new float[nCount];
        for (int i = 0; i < nCount; i++)
        {
            bi[i] = pIn[i];
            high[i] = pHigh[i];
            low[i] = pLow[i];
        }
        var request = new ZsRequest { NCount = nCount, Bi = bi, High = high, Low = low };
        float[] outArr = SafeCallApi("/api/calculation/zs/direction", request, ApiJsonContext.Default.ZsRequest);
        CopyToOutput(nCount, pOut, outArr);
    }

    //=========================================================================
    // 输出函数9号：同方向的第几个中枢
    //=========================================================================
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "Func9")]
    public static void Func9(int nCount, float* pOut, float* pIn, float* pHigh, float* pLow)
    {
        if (nCount <= 0 || pOut == null || pIn == null || pHigh == null || pLow == null) return;

        float[] bi = new float[nCount];
        float[] high = new float[nCount];
        float[] low = new float[nCount];
        for (int i = 0; i < nCount; i++)
        {
            bi[i] = pIn[i];
            high[i] = pHigh[i];
            low[i] = pLow[i];
        }
        var request = new ZsRequest { NCount = nCount, Bi = bi, High = high, Low = low };
        float[] outArr = SafeCallApi("/api/calculation/zs/index", request, ApiJsonContext.Default.ZsRequest);
        CopyToOutput(nCount, pOut, outArr);
    }

    // 静态函数信息表
    private static readonly PluginTCalcFuncInfo[] Info;
    private static readonly GCHandle _infoHandle;

    static NativeExports()
    {
        Info = new PluginTCalcFuncInfo[]
        {
            new() { NFuncMark = 1, PCallFunc = (nint)(delegate* unmanaged[Cdecl]<int, float*, float*, float*, float*, void>)&Func1 },
            new() { NFuncMark = 2, PCallFunc = (nint)(delegate* unmanaged[Cdecl]<int, float*, float*, float*, float*, void>)&Func2 },
            new() { NFuncMark = 3, PCallFunc = (nint)(delegate* unmanaged[Cdecl]<int, float*, float*, float*, float*, void>)&Func3 },
            new() { NFuncMark = 4, PCallFunc = (nint)(delegate* unmanaged[Cdecl]<int, float*, float*, float*, float*, void>)&Func4 },
            new() { NFuncMark = 5, PCallFunc = (nint)(delegate* unmanaged[Cdecl]<int, float*, float*, float*, float*, void>)&Func5 },
            new() { NFuncMark = 6, PCallFunc = (nint)(delegate* unmanaged[Cdecl]<int, float*, float*, float*, float*, void>)&Func6 },
            new() { NFuncMark = 7, PCallFunc = (nint)(delegate* unmanaged[Cdecl]<int, float*, float*, float*, float*, void>)&Func7 },
            new() { NFuncMark = 8, PCallFunc = (nint)(delegate* unmanaged[Cdecl]<int, float*, float*, float*, float*, void>)&Func8 },
            new() { NFuncMark = 9, PCallFunc = (nint)(delegate* unmanaged[Cdecl]<int, float*, float*, float*, float*, void>)&Func9 },
            new() { NFuncMark = 0, PCallFunc = 0 }
        };
        _infoHandle = GCHandle.Alloc(Info, GCHandleType.Pinned);
    }

    //=========================================================================
    // 通达信函数注册
    //=========================================================================
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) }, EntryPoint = "RegisterTdxFunc")]
    public static int RegisterTdxFunc(PluginTCalcFuncInfo** pInfo)
    {
        if (*pInfo == null)
        {
            *pInfo = (PluginTCalcFuncInfo*)_infoHandle.AddrOfPinnedObject();
            return 1; // TRUE
        }
        return 0; // FALSE
    }
}
