---
name: release
description: 建置、打包並發佈 GitHub Release
argument-hint: "[版本號（選填，如 v1.0.1）]"
allowed-tools: Bash, Read, Grep, Glob, Edit, AskUserQuestion
---

# GitHub Release 發佈助手

請依照以下流程執行 release。先讀取專案的 `BUILD.md` 了解建置方式。

## 流程

### 1. 確認版本號

- 執行 `gh release list --limit 5` 查看最近的 release
- 讀取 `fast-cli-tool.csproj` 中目前的版本號
- 如果有提供 $ARGUMENTS，使用指定的版本號
- 如果沒有，用 AskUserQuestion 詢問使用者要使用的版本號（建議 patch / minor / major）

### 2. 更新版本號

編輯 `fast-cli-tool.csproj` 中的三個版本欄位：

```xml
<Version>x.y.z</Version>
<AssemblyVersion>x.y.z.0</AssemblyVersion>
<FileVersion>x.y.z.0</FileVersion>
```

### 3. 建置單一執行檔

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o bin/publish-single
```

- 如果建置因檔案鎖定失敗，提示使用者關閉正在執行的 `fast-cli-tool.exe`，確認後重試

### 4. Commit 並 Push

```bash
git add fast-cli-tool.csproj
git commit -m "[更新]版本號升至 v{版本號}

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
git push
```

### 5. 打包附件

附件命名格式：`fast-cli-tool-v{版本號}-win-x64.zip`

```bash
cd bin/publish-single
cp fast-cli-tool.exe fast-cli-tool-v{版本號}-win-x64.exe
powershell Compress-Archive -Path fast-cli-tool-v{版本號}-win-x64.exe -DestinationPath fast-cli-tool-v{版本號}-win-x64.zip -Force
```

### 6. 產生 Release Notes

根據對話上下文和 git log，整理自上一個 release 以來的變更內容：

```bash
git log {上一個tag}..HEAD --oneline
```

用繁體中文撰寫 release notes，格式如下：

```
## 變更內容

- 變更項目 1
- 變更項目 2

## 下載

下載 `fast-cli-tool-v{版本號}-win-x64.zip` 解壓後直接執行，不需安裝 .NET Runtime。
```

### 7. 建立 Release

```bash
gh release create v{版本號} bin/publish-single/fast-cli-tool-v{版本號}-win-x64.zip --title "v{版本號}" --notes "{release notes}"
```

### 8. 確認結果

- 執行 `gh release view v{版本號}` 確認 release 建立成功
- 回報 release URL 給使用者

## 規則

- 版本號格式為 `v{major}.{minor}.{patch}`（如 `v1.0.1`）
- `gh release create` 會自動建立 git tag，不需手動執行 `git tag`
- 附件命名必須遵循 `fast-cli-tool-v{版本號}-win-x64.zip` 格式
- Release notes 使用繁體中文
- 如有未 commit 的變更，先提醒使用者處理
