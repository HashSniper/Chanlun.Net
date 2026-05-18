# ChanlunX.CSharp - 通达信 NativeAOT DLL

本项目将原 C++ 版 ChanlunX 核心算法翻译为 C#，并通过 .NET NativeAOT 编译为无托管依赖的 Windows x64 DLL，可直接供通达信调用。

## 核心功能

| 函数 | 说明 |
|------|------|
| `Func1` | 简笔顶底端点 |
| `Func2` | 标准笔顶底端点 |
| `Func3` | 段的端点（标准画法） |
| `Func4` | 段的端点（1+1终结画法） |
| `Func5` | 中枢高点数据 |
| `Func6` | 中枢低点数据 |
| `Func7` | 中枢起点、终点信号 |
| `Func8` | 中枢方向数据 |
| `Func9` | 同方向的第几个中枢 |
| `RegisterTdxFunc` | 通达信插件注册入口 |

## 编译说明

### 环境要求
- .NET SDK 10.0 或更高版本（需支持 NativeAOT）
- Visual Studio 2022（含 C++ 工作负载，用于 NativeAOT 链接器）

### 编译命令
```bash
cd ChanlunX.CSharp
dotnet publish -c Release -r win-x64 -p:PublishAot=true
```

编译产物位于：
```
bin/Release/net10.0-windows/win-x64/native/ChanlunX.CSharp.dll
```

### DLL 特点
- **纯原生代码**：通过 NativeAOT 编译，不依赖 .NET 运行时
- **无额外依赖**：仅依赖 Windows 系统 DLL（kernel32、ucrtbase 等）
- **C 标准导出**：`Func1` ~ `Func9` 为 `cdecl` 调用约定，`RegisterTdxFunc` 为 `stdcall` 调用约定，与原版 C++ DLL 完全兼容

## 通达信使用方式

将生成的 `ChanlunX.CSharp.dll` 拷贝到通达信安装目录的 `T0002/dlls/` 文件夹下，并在公式中按通达信扩展数据接口规范调用即可。

通达信公式调用示例：
```
{ 简笔顶底 }
BI1: DLLNAME.Func1(HIGH, LOW);

{ 标准笔顶底 }
BI2: DLLNAME.Func2(HIGH, LOW);
```

> 注意：`DLLNAME` 需替换为你在通达信中注册的实际 DLL 名称。

## 文件结构

```
ChanlunX.CSharp/
├── ChanlunX.CSharp.csproj   # 项目文件（已配置 NativeAOT）
├── NativeExports.cs          # DLL 导出函数（通达信接口）
├── Models.cs                 # 数据结构（K线、笔、中枢等）
├── KxianChuLi.cs             # K线包含处理
├── BiChuLi.cs                # 笔处理
├── BiCalculator.cs           # 简笔/标准笔计算
├── DuanCalculator.cs         # 段计算
└── ZhongShuCalculator.cs     # 中枢计算
```

## 与原版 C++ 的对应关系

| C++ 文件 | C# 文件 | 说明 |
|----------|---------|------|
| `Main.cpp` | `NativeExports.cs` | DLL 导出接口 |
| `KxianChuLi.h/cpp` | `Models.cs` + `KxianChuLi.cs` | K线包含处理 |
| `BiChuLi.h/cpp` | `BiChuLi.cs` | 笔处理 |
| `Bi.h/cpp` | `BiCalculator.cs` | 笔计算 |
| `Duan.h/cpp` | `DuanCalculator.cs` | 段计算 |
| `ZhongShu.h/cpp` | `ZhongShuCalculator.cs` | 中枢计算 |

## 注意事项

1. **平台限制**：当前仅编译为 `win-x64`，如需 x86 需修改 `<RuntimeIdentifier>` 为 `win-x86`
2. **数据安全**：所有导出函数内部均将 `float*` 指针数据复制到托管数组进行处理，避免不安全内存操作
3. **性能**：NativeAOT 编译后的代码性能接近原生 C++，满足通达信实时计算需求
