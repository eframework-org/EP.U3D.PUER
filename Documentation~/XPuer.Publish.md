# XPuer.Publish

[![Version](https://img.shields.io/npm/v/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)
[![Downloads](https://img.shields.io/npm/dm/ep.u3d.puer)](https://www.npmjs.com/package/ep.u3d.puer)

XPuer.Publish 实现了脚本包的发布工作流，用于将打包好的脚本发布至对象存储服务（OSS）中。

## 功能特性

- 首选项配置：提供首选项配置以自定义发布流程
- 自动化流程：提供脚本包发布任务的自动化执行

## 使用手册

### 1. 首选项配置

| 配置项 | 配置键 | 默认值 | 功能说明 |
|--------|--------|--------|----------|
| 主机地址 | `Puer/Publish/Host@Editor` | `${Env.OssHost}` | OSS 服务地址 |
| 存储桶名 | `Puer/Publish/Bucket@Editor` | `${Env.OssBucket}` | OSS 存储桶名 |
| 访问密钥 | `Puer/Publish/Access@Editor` | `${Env.OssAccess}` | OSS 访问密钥 |
| 秘密密钥 | `Puer/Publish/Secret@Editor` | `${Env.OssSecret}` | OSS 秘密密钥 |

关联配置项：`Puer/LocalUri`、`Puer/RemoteUri`

以上配置项均可在 `EFramework/Preferences/Puer/Publish` 首选项编辑器中进行可视化配置。

### 2. 自动化流程

#### 2.1 本地环境

本地开发环境可以使用 MinIO 作为对象存储服务：

1. 安装服务：

```bash
# 启动 MinIO 容器
docker run -d --name minio -p 9000:9000 -p 9090:9090 --restart=always \
  -e "MINIO_ACCESS_KEY=admin" -e "MINIO_SECRET_KEY=adminadmin" \
  minio/minio server /data --console-address ":9090" --address ":9000"
```

2. 服务配置：
   - 控制台：http://localhost:9090
   - API：http://localhost:9000
   - 凭证：
     - Access Key：admin
     - Secret Key：adminadmin
   - 存储：创建 `default` 存储桶并设置公开访问权限

3. 首选项配置：
   ```
   Puer/Publish/Host@Editor = http://localhost:9000
   Puer/Publish/Bucket@Editor = default
   Puer/Publish/Access@Editor = admin
   Puer/Publish/Secret@Editor = adminadmin
   ```

#### 2.2 发布流程

```mermaid
stateDiagram-v2
    direction LR
    读取发布配置 --> 获取远端清单
    获取远端清单 --> 对比本地清单
    对比本地清单 --> 发布差异文件
```

##### 发布规则
发布时根据清单对比结果进行增量上传：
- 新增文件：`文件名@MD5`
- 修改文件：`文件名@MD5`
- 清单文件：`Manifest.md5` 和 `Manifest.md5@yyyy-MM-dd_HH-mm-ss`（用于版本回退）

##### 路径说明
1. 本地路径
   - 构建目录：`${Prefs.Output}/${Channel}/${Platform}`
     - 示例：`Builds/Patch/Scripts/TS/Default/Windows`
   - 临时目录：`${Temp}/${Prefs.LocalUri}`
     - 示例：`Temp/Scripts/TS`

2. 远端路径
   - 远端目录：`${Prefs.RemoteUri}`
     - 配置键：`Puer/RemoteUri`
     - 默认值：`${Prefs.Update/PatchUri}/Scripts/TS/Default/Windows`
   - 完整路径：`${Alias}/${Bucket}/${Remote}/`
     - 示例：`myminio/default/Builds/Patch/Scripts/TS/Default/Windows`

## 常见问题

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE)