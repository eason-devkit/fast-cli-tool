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

## GitHub Release 發佈流程

### 前置條件

- 已安裝 [GitHub CLI (gh)](https://cli.github.com/)
- 已登入 GitHub：`gh auth login`
- 所有變更已 commit 並 push 至 remote

### 步驟

#### 1. 更新版本號

編輯 `fast-cli-tool.csproj` 中的版本資訊：

```xml
<Version>1.0.1</Version>
<AssemblyVersion>1.0.1.0</AssemblyVersion>
<FileVersion>1.0.1.0</FileVersion>
```

#### 2. 建置單一執行檔

確保 `fast-cli-tool.exe` 未在執行中，否則會因檔案鎖定而建置失敗。

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o bin/publish-single
```

#### 3. Commit 並 Push

```bash
git add fast-cli-tool.csproj
git commit -m "[更新]版本號升至 v1.0.1"
git push
```

#### 4. 打包附件

附件命名格式：`fast-cli-tool-v{版本號}-win-x64.zip`

```bash
cd bin/publish-single
cp fast-cli-tool.exe fast-cli-tool-v1.0.1-win-x64.exe
powershell Compress-Archive -Path fast-cli-tool-v1.0.1-win-x64.exe -DestinationPath fast-cli-tool-v1.0.1-win-x64.zip -Force
```

#### 5. 建立 Release

`gh release create` 會自動在 GitHub 上建立對應的 git tag（如 `v1.0.1`），不需手動執行 `git tag`。

```bash
gh release create v1.0.1 bin/publish-single/fast-cli-tool-v1.0.1-win-x64.zip --title "v1.0.1" --notes "$(cat <<'EOF'
## 變更內容

- 變更項目 1
- 變更項目 2

## 下載

下載 `fast-cli-tool-v1.0.1-win-x64.zip` 解壓後直接執行，不需安裝 .NET Runtime。
EOF
)"
```

### 管理 Release

```bash
# 列出所有 release
gh release list

# 查看特定 release 詳細資訊
gh release view v1.0.1

# 刪除並重新上傳附件
gh release delete-asset v1.0.1 <檔名> --yes
gh release upload v1.0.1 <新檔案路徑>

# 在瀏覽器開啟 release 頁面
start https://github.com/eason-devkit/fast-cli-tool/releases/tag/v1.0.1
```
