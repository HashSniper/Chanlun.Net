# NativeAOT DLL 调试指南（Visual Studio）

## 问题原因

NativeAOT 编译后的 DLL 是**纯原生机器码**（类似 C++），不含 .NET 运行时，因此：
- ❌ VS 的**托管调试器**（Managed）无法命中断点
- ✅ 必须使用 VS 的**本机调试器**（Native）才能调试

---

## 正确调试步骤

### 第一步：重新编译 Debug 版本

```bash
cd ChanlunX.CSharp
dotnet publish -c Debug -r win-x64 -p:PublishAot=true
```

输出文件位置：
```
bin/Debug/net10.0-windows/win-x64/native/
├── ChanlunX.CSharp.dll   ← 复制到通达信目录
└── ChanlunX.CSharp.pdb   ← 必须与 DLL 放在一起！
```

### 第二步：复制 DLL + PDB 到通达信目录

**必须同时复制 `.pdb` 文件**，否则 VS 无法映射源代码！

```batch
xcopy /Y "bin\Debug\net10.0-windows\win-x64\native\ChanlunX.CSharp.dll" "C:\通达信\T0002\dlls\"
xcopy /Y "bin\Debug\net10.0-windows\win-x64\native\ChanlunX.CSharp.pdb" "C:\通达信\T0002\dlls\"
```

> ⚠️ 如果通达信正在运行且已加载 DLL，必须先**关闭通达信**才能覆盖文件！

### 第三步：在 VS 中打开源码并设置断点

1. 在 VS 中打开 `ChanlunX.CSharp` 项目（或单独打开 `.cs` 源文件）
2. 在 `NativeExports.cs` 中设置断点（如 `Func1`、`Func2` 等）

### 第四步：Attach 到通达信进程（关键！）

1. 先启动通达信，进入 K 线图界面（确保会调用 DLL 函数）
2. VS 菜单 → **调试 → 附加到进程**（Ctrl+Alt+P）
3. 找到 `TdxW.exe`，选中
4. ⭐**关键步骤**：点击 **"附加到:"** 右侧的 **"选择"** 按钮
5. 勾选 **"调试这些代码类型:"** → 选择 **"本机 (Native)"**
   
   ❌ 不要选 "托管 (.NET Core, .NET 5+)" 或 "自动"
   
   ✅ 必须选 **"本机"**

6. 点击 **"附加"**

### 第五步：验证符号已加载

1. VS 菜单 → **调试 → 窗口 → 模块**（Ctrl+Alt+U）
2. 在模块列表中找到 `ChanlunX.CSharp.dll`
3. 查看 **"符号状态"** 列：
   - ✅ 应显示 **"符号已加载"**
   - ❌ 如果显示 **"无法找到或打开 PDB 文件"**，检查：
     - PDB 是否与 DLL 在同一目录
     - 是否复制了 Debug 版本的 PDB

### 第六步：触发断点

在通达信中切换股票、切换周期、或刷新 K 线图，触发指标计算。

如果一切正常，断点会从**空心圆**变为**红色实心圆**，并在命中时暂停。

---

## 常见问题排查

### 1. 断点显示"空心圆"（未绑定）

| 原因 | 解决方法 |
|------|---------|
| 符号未加载 | 确保 PDB 与 DLL 同目录，查看模块窗口确认 |
| Attach 类型错误 | 重新 Attach，选择 **本机 (Native)** |
| 通达信未加载 DLL | 先在通达信中打开调用该 DLL 的指标公式 |
| 编译优化导致代码内联 | 使用 Debug 配置编译（已降低优化） |
| DLL 版本不匹配 | 关闭通达信，重新复制最新 DLL 和 PDB |

### 2. 模块窗口中找不到 ChanlunX.CSharp.dll

说明通达信还没有加载该 DLL。先确保：
- 通达信公式中正确引用了该 DLL
- 在通达信中打开了使用该 DLL 的指标
- DLL 文件名与通达信公式中引用的名称一致

### 3. 断点命中但堆栈显示异常

NativeAOT 的堆栈回溯可能不完美。如果需要查看变量值：
- 在**自动窗口**或**局部变量窗口**中查看
- 某些局部变量可能被优化掉，改用 `Console.WriteLine` 或日志输出辅助调试

---

## 快速调试脚本

创建 `deploy_debug.bat`：

```batch
@echo off
set TDX_DLL_DIR=C:\通达信\T0002\dlls
set DLL_NAME=ChanlunX.CSharp

echo [1/3] 编译 Debug 版本...
cd /d "%~dp0"
dotnet publish -c Debug -r win-x64 -p:PublishAot=true

echo [2/3] 复制 DLL + PDB 到通达信目录...
copy /Y "bin\Debug\net10.0-windows\win-x64\native\%DLL_NAME%.dll" "%TDX_DLL_DIR%\"
copy /Y "bin\Debug\net10.0-windows\win-x64\native\%DLL_NAME%.pdb" "%TDX_DLL_DIR%\"

echo [3/3] 完成！
echo 请确保通达信已关闭后再运行此脚本。
pause
```

---

## 发布正式版本

调试完成后，使用 Release 配置编译正式版本：

```bash
dotnet publish -c Release -r win-x64 -p:PublishAot=true
```

Release 版本会启用速度优化，体积更小，但调试信息较少。
