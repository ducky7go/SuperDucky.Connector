# 实施任务列表

本文件包含修复构建问题的详细实施步骤。

## 任务概览

| 任务 ID | 任务名称 | 优先级 | 预计复杂度 |
|---------|----------|--------|------------|
| T001 | 更新 PR Build Validation 工作流 | P0 | 低 |
| T002 | 更新 Publish 工作流（如存在） | P0 | 低 |
| T003 | 创建本地构建工具设置脚本 | P1 | 低 |
| T004 | 更新开发文档 | P2 | 低 |
| T005 | 验证 CI 构建成功 | P0 | 中 |
| T006 | 验证本地构建成功 | P1 | 中 |

---

## T001: 更新 PR Build Validation 工作流

**优先级**: P0 (关键)
**状态**: Completed
**负责人**: -

### 描述

在 `.github/workflows/pr-build-validation.yml` 中添加 `dotnet script` 和 `dotnet-ilrepack` 的安装步骤。

### 实施步骤

1. 打开 `.github/workflows/pr-build-validation.yml`
2. 在 "Fetch build dependencies" 步骤（第 44-45 行）之后添加以下内容：

```yaml
- name: Install dotnet script
  run: dotnet tool install --global dotnet-script --version 1.5.0

- name: Install dotnet-ilrepack
  run: dotnet tool install --global dotnet-ilrepack --version 2.0.44

- name: Add tools to PATH
  run: echo "$HOME/.dotnet/tools" >> $GITHUB_PATH
```

3. 验证 YAML 语法正确
4. 提交更改

### 验证标准

- YAML 语法正确
- 步骤位置在 "Fetch build dependencies" 和 "Build mod" 之间
- 版本号与 Ducky.Sdk 一致

### 风险与注意事项

- 确保版本号 (1.5.0, 2.0.44) 与 Ducky.Sdk 兼容
- PATH 设置必须在后续使用工具的步骤之前

---

## T002: 更新 Publish 工作流（如存在）

**优先级**: P0 (关键)
**状态**: Completed
**负责人**: -

### 描述

检查并更新 `.github/workflows/publish.yml`，确保发布流程也包含工具安装步骤。

### 实施步骤

1. 检查 `.github/workflows/publish.yml` 是否存在
2. 如果存在，检查是否包含构建步骤
3. 如果包含构建步骤，添加与 T001 相同的工具安装步骤
4. 如果不存在，标记此任务为不适用

### 验证标准

- Publish 工作流能够成功执行构建
- 工具安装步骤位置正确

---

## T003: 创建本地构建工具设置脚本

**优先级**: P1
**状态**: Completed
**负责人**: -

### 描述

创建 `scripts/setup_build_tools.sh` 脚本，帮助本地开发者快速设置所需的构建工具。

### 实施步骤

1. 创建 `scripts/setup_build_tools.sh` 文件
2. 添加以下内容：

```bash
#!/bin/bash
set -e

echo "🔧 Setting up Ducky.Sdk build tools..."

# Get the directory where this script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_ROOT"

# Install dotnet-script
if ! command -v dotnet-script &> /dev/null; then
    echo "📦 Installing dotnet-script..."
    dotnet tool install --global dotnet-script --version 1.5.0
else
    echo "✅ dotnet-script already installed"
    dotnet-script --version
fi

# Install dotnet-ilrepack
if ! command -v dotnet-ilrepack &> /dev/null; then
    echo "📦 Installing dotnet-ilrepack..."
    dotnet tool install --global dotnet-ilrepack --version 2.0.44
else
    echo "✅ dotnet-ilrepack already installed"
    dotnet-ilrepack --version 2>/dev/null || echo " (version check not available)"
fi

# Check if .dotnet/tools is in PATH
if [[ ":$PATH:" != *":$HOME/.dotnet/tools:"* ]]; then
    echo ""
    echo "⚠️  WARNING: $HOME/.dotnet/tools is not in your PATH"
    echo ""
    echo "Add the following to your ~/.bashrc or ~/.zshrc:"
    echo "  export PATH=\"\$PATH:\$HOME/.dotnet/tools\""
    echo ""
    echo "Then run: source ~/.bashrc (or ~/.zshrc)"
fi

echo ""
echo "✨ Build tools setup complete!"
```

3. 设置可执行权限：`chmod +x scripts/setup_build_tools.sh`

### 验证标准

- 脚本可执行
- 能够检测已安装的工具
- 正确处理 PATH 检查和警告

---

## T004: 更新开发文档

**优先级**: P2
**状态**: Completed
**负责人**: -

### 描述

更新 README.md 或创建 DEVELOPMENT.md，说明构建工具的设置方法。

### 实施步骤

1. 检查 README.md 是否需要更新
2. 在适当位置添加开发环境设置说明：

```markdown
## 开发环境设置

### 前置要求

- .NET SDK (参见 global.json)
- Git

### 构建工具

项目使用 Ducky.Sdk 进行构建，需要以下工具：

```bash
# 方式 1: 自动设置（推荐）
bash scripts/setup_build_tools.sh

# 方式 2: 手动安装
dotnet tool install --global dotnet-script --version 1.5.0
dotnet tool install --global dotnet-ilrepack --version 2.0.44

# 确保 dotnet tools 在 PATH 中
export PATH="$PATH:$HOME/.dotnet/tools"
```

### 构建项目

```bash
# 克隆后获取构建依赖
bash scripts/fetch_build_dependency.sh

# 构建项目
dotnet build SuperDucky.Connector/SuperDucky.Connector.csproj
```
```

3. 如需要，创建 CONTRIBUTING.md 贡献指南

### 验证标准

- 文档清晰易懂
- 包含命令示例
- 说明工具版本要求

---

## T005: 验证 CI 构建成功

**优先级**: P0 (关键)
**状态**: Pending
**负责人**: -
**依赖**: T001, T002

### 描述

提交更改后验证 GitHub Actions CI 构建是否成功。

### 实施步骤

1. 创建特性分支：`git checkout -b fix/build-dependencies`
2. 提交所有更改
3. 推送到远程
4. 创建 Pull Request
5. 观察 PR Build Validation 工作流执行
6. 检查构建日志
7. 如失败，分析日志并修复

### 验证标准

- PR Build Validation 工作流成功完成
- 所有 dotnet script 执行成功
- 无工具缺失错误
- 构建产物正常生成

### 可能的问题

- **问题**: `dotnet: command not found`
  - **解决**: 检查 PATH 设置步骤是否正确

- **问题**: `dotnet-script: No such file or directory`
  - **解决**: 确认工具安装步骤在构建步骤之前

- **问题**: 脚本执行失败
  - **解决**: 检查 Ducky.Sdk 版本兼容性

---

## T006: 验证本地构建成功

**优先级**: P1
**状态**: Pending
**负责人**: -
**依赖**: T003

### 描述

在全新环境中验证本地构建流程。

### 实施步骤

1. （模拟全新环境）移除本地工具：
   ```bash
   dotnet tool uninstall --global dotnet-script
   dotnet tool uninstall --global dotnet-ilrepack
   ```
2. 运行设置脚本：`bash scripts/setup_build_tools.sh`
3. 执行依赖获取：`bash scripts/fetch_build_dependency.sh`
4. 构建项目：
   ```bash
   export CI=true
   dotnet build SuperDucky.Connector/SuperDucky.Connector.csproj
   ```
5. 验证输出

### 验证标准

- 设置脚本成功执行
- 构建无错误完成
- 所有 Ducky.Sdk 脚本库正确执行
- 产物在 `artifacts/Mods/` 目录生成

---

## 额外检查项

在实施完成后，验证以下内容：

- [x] CI 工作流包含工具安装步骤
- [x] Publish 工作流（如存在）包含工具安装步骤
- [x] 设置脚本创建并可执行
- [x] 开发文档已更新
- [ ] CI 构建成功（需创建 PR 验证）
- [ ] 本地构建成功（需开发者验证）
- [ ] 无新增警告或错误

---

## 回滚计划

如果实施后出现问题，回滚步骤：

1. 移除工作流中的工具安装步骤
2. 删除设置脚本（如已创建）
3. 恢复文档更改
4. 创建 issue 记录问题并分析

---

## 时间线建议

| 阶段 | 任务 | 说明 |
|------|------|------|
| Phase 1 | T001, T002 | 修复 CI（高优先级） |
| Phase 2 | T003, T004 | 改善开发体验 |
| Phase 3 | T005, T006 | 验证和测试 |

Phase 1 应优先完成，以解除 CI 阻塞。Phase 2 和 Phase 3 可以在后续迭代中完成。
