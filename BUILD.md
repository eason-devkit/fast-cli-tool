# Fast CLI Tool - 建置指南

## 環境需求

- .NET 9.0 SDK
- Windows 10/11

## 建置方式

### 1. 一般建置（開發用）

```bash
dotnet build -c Debug
```

輸出位置：`bin/Debug/net9.0-windows/`

### 2. Release 發佈（多檔案）

```bash
dotnet publish -c Release -r win-x64 --self-contained -o bin/publish
```

- 包含 .NET Runtime，不需安裝 .NET
- 輸出多個 DLL 檔案 + exe

### 3. 單一 exe 發佈（推薦）

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o bin/publish-single
```

- 產出單一 `fast-cli-tool.exe`（約 128 MB）
- 可直接在任何 Windows 電腦執行，不需安裝 .NET Runtime
- `.pdb` 檔為偵錯符號，發佈時可忽略

### 4. 單一 exe + 裁剪（最小體積）

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true -o bin/publish-trimmed
```

- 移除未使用的程式碼，檔案更小
- 注意：裁剪可能移除反射相關的程式碼，需充分測試
