# 修复构建问题

## 状态
ExecutionCompleted

## 类型
Bug Fix

## 概述

修复 SuperDucky.Connector 项目构建过程中的依赖恢复问题，确保 `dotnet script` 和 `ilrepack` 工具在构建前正确安装和初始化。

## 背景

当前项目 SuperDucky.Connector 使用 Ducky.Sdk 作为构建依赖。Ducky.Sdk 的构建系统依赖于：
- **dotnet script** - 用于执行 CSX 构建脚本
- **dotnet-ilrepack** - 用于合并程序集

参考项目 `/home/newbe36524/repos/newbe36524/Ducky.Sdk` 已经成功解决了类似问题，其 GitHub Actions 工作流包含了针对这些工具的显式安装步骤。

当前 SuperDucky.Connector 的 CI/CD 配置（`.github/workflows/pr-build-validation.yml`）缺少这些步骤，导致构建失败。

## 问题

### 根本原因
1. **dotnet script 未安装** - Ducky.Sdk 的 `Ducky.Sdk.Orchestration.targets` 使用 `dotnet script` 命令执行多个 CSX 脚本库，但构建环境中未预装此工具
2. **ilrepack 未安装** - 当 `EnableILRepack=true` 时，构建流程需要 `dotnet-ilrepack` 工具来合并程序集

### 失败场景
- GitHub Actions CI pipeline 失败
- 本地全新环境构建失败（未手动安装 dotnet script）
- CI=true 环境下构建失败

## 解决方案

### 方案 1: 在 CI 工作流中添加工具安装步骤（推荐）

在 `.github/workflows/pr-build-validation.yml` 的 "Fetch build dependencies" 步骤后添加：

```yaml
- name: Install dotnet script
  run: dotnet tool install --global dotnet-script --version 1.5.0

- name: Install dotnet-ilrepack
  run: dotnet tool install --global dotnet-ilrepack --version 2.0.44

- name: Add tools to PATH
  run: echo "$HOME/.dotnet/tools" >> $GITHUB_PATH
```

### 方案 2: 创建本地构建辅助脚本

创建 `scripts/setup_build_tools.sh` 用于本地开发环境准备：

```bash
#!/bin/bash
set -e

echo "Setting up build tools..."

# Install dotnet-script
if ! command -v dotnet-script &> /dev/null; then
    echo "Installing dotnet-script..."
    dotnet tool install --global dotnet-script --version 1.5.0
else
    echo "dotnet-script already installed"
fi

# Install dotnet-ilrepack
if ! command -v dotnet-ilrepack &> /dev/null; then
    echo "Installing dotnet-ilrepack..."
    dotnet tool install --global dotnet-ilrepack --version 2.0.44
else
    echo "dotnet-ilrepack already installed"
fi

echo "Build tools setup complete!"
```

### 实施步骤（见 tasks.md）

详细的分步实施计划请参考 [tasks.md](./tasks.md)。

## 影响范围

### 受影响组件
- `.github/workflows/pr-build-validation.yml` - GitHub Actions PR 验证工作流
- `.github/workflows/publish.yml` - 发布工作流（如存在）
- `scripts/` - 新增构建工具设置脚本

### 不受影响组件
- Ducky.Sdk 包本身
- 项目源代码
- 本地已配置环境的开发者

## 风险评估

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| dotnet script 版本兼容性 | 可能导致脚本执行失败 | 使用与 Ducky.Sdk 测试通过的版本 (1.5.0) |
| ilrepack 版本兼容性 | 程序集合并可能失败 | 使用与 Ducky.Sdk 一致的版本 (2.0.44) |
| PATH 配置问题 | 工具可能不可用 | 在工作流中显式添加到 PATH |
| 本地开发环境差异 | 开发者可能遇到不同问题 | 提供设置脚本和文档 |

## 成功标准

1. CI/CD pipeline 能够成功完成构建
2. PR 合并前自动验证通过
3. 本地全新克隆项目可以一键构建（执行设置脚本后）
4. 所有 Ducky.Sdk 构建脚本正常执行
5. ILRepack 程序集合并成功（如启用）

## 替代方案

### 方案 A: 在 Directory.Build.props 中添加自动安装
在项目文件中添加 MSBuild Target 自动安装工具。缺点是每次构建都会检查，增加构建时间。

### 方案 B: 使用 Docker 容器构建
创建包含所有预装工具的 Docker 镜像。优点是环境一致性好，缺点是增加本地开发复杂度。

### 方案 C: 依赖全局工具安装
要求所有开发者手动安装工具。缺点是开发者体验差，容易遗漏。

**推荐采用方案 1 + 方案 2 的组合**：在 CI 中自动安装，为开发者提供便利脚本。

## 相关资源

- Ducky.Sdk 参考配置: `/home/newbe36524/repos/newbe36524/Ducky.Sdk/.github/workflows/build_dotnet.yml:34-36`
- Ducky.Sdk 构建系统: `/home/newbe36524/repos/newbe36524/Ducky.Sdk/Sdk/SDKlibs/Ducky.Sdk/Ducky.Sdk.Orchestration.targets`
- 当前 CI 配置: `.github/workflows/pr-build-validation.yml`
- dotnet-script: https://github.com/dotnet-script/dotnet-script
- dotnet-ilrepack: https://github.com/gluck/il-repack

## 审批状态

| 角色 | 姓名 | 状态 |
|------|------|------|
| 提案人 | - | Pending |
| 审批人 | - | Pending |
| 实施人 | - | Pending |

## 变更历史

| 日期 | 版本 | 变更内容 | 作者 |
|------|------|----------|------|
| 2026-01-06 | 1.0 | 初始提案 | - |
