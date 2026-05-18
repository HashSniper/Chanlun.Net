using System.Runtime.InteropServices;

namespace NativeAdapter;

// 通达信插件函数信息
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PluginTCalcFuncInfo
{
    public ushort NFuncMark;
    public nint PCallFunc; // 函数指针
}