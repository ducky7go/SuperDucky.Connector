# Change: 补充 NuGet 包信息 (Add NuGet Package Metadata)

**Status**: ExecutionCompleted

## Why

当前 `SuperDucky.Connector` 模组的 `info.ini` 文件仅包含基础元数据字段，未包含 NuGet 打包规范所需的扩展元数据字段。这导致打包为 NuGet 包时元数据不完整，降低包的可发现性和可信度。

The `info.ini` file currently contains only basic metadata fields, lacking the extended metadata required by the NuGet mod packaging specification. This results in incomplete NuGet package metadata, reducing package discoverability and credibility.

## What Changes

- 在 `SuperDucky.Connector/assets/info.ini` 中添加扩展元数据字段
  - `author` / `authors` - 作者信息
  - `license` - 许可证类型
  - `homepage` / `projectUrl` - 项目主页地址
  - `repository.type` 和 `repository.url` - 代码仓库信息
  - `tags` - NuGet 包标签

## Impact

- Affected specs: `nuget-packaging` (NEW)
- Affected code: `SuperDucky.Connector/assets/info.ini`
- No runtime behavior changes (metadata only used during packaging)
- No impact to existing CI/CD workflows
