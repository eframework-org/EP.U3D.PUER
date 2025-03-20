// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EP.U3D.UTIL;
using ET.U3D.UTIL;

namespace ET.U3D.PUER
{
    public partial class XPuer
    {
        /// <summary>
        /// XPuer.Publish 实现了脚本包的发布工作流，用于将打包好的脚本发布至对象存储服务（OSS）中。
        /// </summary>
        /// <remarks>
        /// <code>
        /// 功能特性：
        /// - 首选项配置：提供首选项配置以自定义发布流程
        /// - 自动化流程：提供脚本包发布任务的自动化执行
        /// 
        /// 使用手册：
        /// 1. 首选项配置
        /// 
        /// | 配置项 | 配置键 | 默认值 | 功能说明 |
        /// |--------|--------|--------|----------|
        /// | 主机地址 | Puer/Publish/Host@Editor | ${Env.OssHost} | OSS 服务地址 |
        /// | 存储桶名 | Puer/Publish/Bucket@Editor | ${Env.OssBucket} | OSS 存储桶名 |
        /// | 访问密钥 | Puer/Publish/Access@Editor | ${Env.OssAccess} | OSS 访问密钥 |
        /// | 秘密密钥 | Puer/Publish/Secret@Editor | ${Env.OssSecret} | OSS 秘密密钥 |
        /// 
        /// 关联配置项：`Puer/LocalUri`、`Puer/RemoteUri`
        /// 
        /// 以上配置项均可在 `EFramework/Preferences/Puer/Publish` 首选项编辑器中进行可视化配置。
        /// 
        /// 2. 自动化流程
        /// 
        /// 2.1 本地环境
        /// 本地开发环境可以使用 MinIO 作为对象存储服务：
        /// 
        /// 1. 安装服务：
        /// # 启动 MinIO 容器
        /// docker run -d --name minio -p 9000:9000 -p 9090:9090 --restart=always \
        ///   -e "MINIO_ACCESS_KEY=admin" -e "MINIO_SECRET_KEY=adminadmin" \
        ///   minio/minio server /data --console-address ":9090" --address ":9000"
        /// 
        /// 2. 服务配置：
        ///   - 控制台：http://localhost:9090
        ///   - API：http://localhost:9000
        ///   - 凭证：
        ///     - Access Key：admin
        ///     - Secret Key：adminadmin
        ///   - 存储：创建 default 存储桶并设置公开访问权限
        /// 
        /// 3. 首选项配置：
        ///   Puer/Publish/Host@Editor = http://localhost:9000
        ///   Puer/Publish/Bucket@Editor = default
        ///   Puer/Publish/Access@Editor = admin
        ///   Puer/Publish/Secret@Editor = adminadmin
        /// 
        /// 2.2 发布流程
        /// 
        /// 发布规则：
        /// 发布时根据清单对比结果进行增量上传：
        /// - 新增文件：文件名@MD5
        /// - 修改文件：文件名@MD5
        /// - 清单文件：Manifest.md5 和 Manifest.md5@yyyy-MM-dd_HH-mm-ss（用于版本回退）
        /// 
        /// 路径说明：
        /// 1. 本地路径
        ///   - 构建目录：${Prefs.Output}/${Channel}/${Platform}
        ///     - 示例：Builds/Patch/Scripts/TS/Default/Windows
        ///   - 临时目录：${Temp}/${Prefs.LocalUri}
        ///     - 示例：Temp/Scripts/TS
        /// 
        /// 2. 远端路径
        ///   - 远端目录：${Prefs.RemoteUri}
        ///     - 配置键：Puer/RemoteUri
        ///     - 默认值：${Prefs.Update/PatchUri}/Scripts/TS/Default/Windows
        ///   - 完整路径：${Alias}/${Bucket}/${Remote}/
        ///     - 示例：myminio/default/Builds/Patch/Scripts/TS/Default/Windows
        /// </code>
        /// 更多信息请参考模块文档。
        /// </remarks>
        [XEditor.Tasks.Worker(name: "Publish Scripts", group: "Puer", runasync: true, priority: 302)]
        public class Publish : XEditor.Oss
        {
            /// <summary>
            /// PuerTS 脚本发布的首选项设置。
            /// 提供了 OSS 服务配置和发布选项的管理功能。
            /// </summary>
            /// <remarks>
            /// <code>
            /// 配置项说明
            /// 1. OSS 服务
            /// - Host: OSS 主机地址
            /// - Bucket: OSS 存储桶名称
            /// - Access: OSS 访问密钥
            /// - Secret: OSS 秘密密钥
            /// 
            /// 2. 默认值
            /// - 支持环境变量替换
            /// - 使用 ${Env.xxx} 格式引用环境变量
            /// </code>
            /// </remarks>
            internal class Prefs : Build.Prefs
            {
                /// <summary>
                /// OSS 主机配置键。
                /// 用于在编辑器中存储 OSS 主机地址。
                /// </summary>
                public const string Host = "Puer/Publish/Host@Editor";

                /// <summary>
                /// OSS 主机默认值。
                /// 支持使用 ${Env.OssHost} 环境变量。
                /// </summary>
                public const string HostDefault = "${Env.OssHost}";

                /// <summary>
                /// OSS 存储桶配置键。
                /// 用于在编辑器中存储 OSS 存储桶名称。
                /// </summary>
                public const string Bucket = "Puer/Publish/Bucket@Editor";

                /// <summary>
                /// OSS 存储桶默认值。
                /// 支持使用 ${Env.OssBucket} 环境变量。
                /// </summary>
                public const string BucketDefault = "${Env.OssBucket}";

                /// <summary>
                /// OSS 访问密钥配置键。
                /// 用于在编辑器中存储 OSS 访问凭证。
                /// </summary>
                public const string Access = "Puer/Publish/Access@Editor";

                /// <summary>
                /// OSS 访问密钥默认值。
                /// 支持使用 ${Env.OssAccess} 环境变量。
                /// </summary>
                public const string AccessDefault = "${Env.OssAccess}";

                /// <summary>
                /// OSS 秘密密钥配置键。
                /// 用于在编辑器中存储 OSS 秘密凭证。
                /// </summary>
                public const string Secret = "Puer/Publish/Secret@Editor";

                /// <summary>
                /// OSS 秘密密钥默认值。
                /// 支持使用 ${Env.OssSecret} 环境变量。
                /// </summary>
                public const string SecretDefault = "${Env.OssSecret}";

                /// <summary>
                /// 获取首选项的分组名称。
                /// </summary>
                public override string Section => "Puer";

                /// <summary>
                /// 获取首选项的优先级。
                /// </summary>
                public override int Priority => 302;

                /// <summary>
                /// 在编辑器中可视化首选项设置。
                /// 提供 OSS 配置的可视化编辑界面。
                /// </summary>
                /// <param name="searchContext">搜索上下文，用于过滤设置项</param>
                public override void OnVisualize(string searchContext)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    foldout = EditorGUILayout.Foldout(foldout, new GUIContent("Publish", "Scripts Publish Options."));
                    if (foldout)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal();
                        Title("Host", "Oss Host Name");
                        Target.Set(Host, EditorGUILayout.TextField("", Target.GetString(Host, HostDefault)));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        Title("Bucket", "Oss Bucket Name");
                        Target.Set(Bucket, EditorGUILayout.TextField("", Target.GetString(Bucket, BucketDefault)));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        Title("Access", "Oss Access Key");
                        Target.Set(Access, EditorGUILayout.TextField("", Target.GetString(Access, AccessDefault)));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        Title("Secret", "Oss Secret Key");
                        Target.Set(Secret, EditorGUILayout.TextField("", Target.GetString(Secret, SecretDefault)));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            /// <summary>
            /// 预处理发布操作。
            /// 初始化 OSS 配置并准备发布环境。
            /// </summary>
            /// <param name="report">发布过程的报告对象</param>
            public override void Preprocess(XEditor.Tasks.Report report)
            {
                Host = XPrefs.GetString(Prefs.Host, Prefs.HostDefault).Eval(XPrefs.Asset, XEnv.Vars);
                Bucket = XPrefs.GetString(Prefs.Bucket, Prefs.BucketDefault).Eval(XPrefs.Asset, XEnv.Vars);
                Access = XPrefs.GetString(Prefs.Access, Prefs.AccessDefault).Eval(XPrefs.Asset, XEnv.Vars);
                Secret = XPrefs.GetString(Prefs.Secret, Prefs.SecretDefault).Eval(XPrefs.Asset, XEnv.Vars);
                base.Preprocess(report);
                Local = XFile.PathJoin(Temp, XPrefs.GetString(Prefs.LocalUri, Prefs.LocalUriDefault));
                Remote = XPrefs.GetString(Prefs.RemoteUri, Prefs.RemoteUriDefault).Eval(XPrefs.Asset, XEnv.Vars);
            }

            /// <summary>
            /// 执行发布处理。
            /// 比对本地和远程文件，执行增量更新。
            /// </summary>
            /// <param name="report">发布过程的报告对象</param>
            public override void Process(XEditor.Tasks.Report report)
            {
                var root = XFile.PathJoin(XPrefs.GetString(Prefs.Output, Prefs.OutputDefault), XEnv.Channel, XEnv.Platform.ToString());

                var remoteMani = new XMani.Manifest();
                var tempFile = Path.GetTempFileName();
                var task = XEditor.Cmd.Run(bin: Bin, args: new string[] { "get", $"\"{Alias}/{Bucket}/{Remote}/{XMani.Default}\"", tempFile });
                task.Wait();
                if (task.Result.Code != 0)
                {
                    XLog.Warn("XPuer.Publish.Process: get remote mainifest failed: {0}", task.Result.Error);
                }
                else
                {
                    remoteMani.Read(tempFile);
                    if (!string.IsNullOrEmpty(remoteMani.Error)) XLog.Warn("XPuer.Publish.Process: parse remote mainifest failed: {0}", remoteMani.Error);
                }

                var localMani = new XMani.Manifest();
                localMani.Read(XFile.PathJoin(root, XMani.Default));
                if (!string.IsNullOrEmpty(localMani.Error)) XLog.Warn("XPuer.Publish.Process: parse local mainifest failed: {0}", remoteMani.Error);
                else
                {
                    var diff = remoteMani.Compare(localMani);
                    var files = new List<string[]>();
                    for (var i = 0; i < diff.Added.Count; i++) { files.Add(new string[] { XFile.PathJoin(root, diff.Added[i].Name), diff.Added[i].MD5 }); }
                    for (var i = 0; i < diff.Modified.Count; i++) { files.Add(new string[] { XFile.PathJoin(root, diff.Modified[i].Name), diff.Modified[i].MD5 }); }
                    if (diff.Added.Count > 0 || diff.Modified.Count > 0)
                    {
                        var maniFile = XFile.PathJoin(root, XMani.Default);
                        files.Add(new string[] { maniFile, "" });
                        files.Add(new string[] { maniFile, XTime.Format(XTime.GetTimestamp(), "yyyy-MM-dd_HH-mm-ss") });
                    }
                    if (files.Count == 0)
                    {
                        XLog.Debug("XPuer.Publish.Process: diff files is zero, no need to publish.");
                        return;
                    }
                    else
                    {
                        foreach (var kvp in files)
                        {
                            var file = kvp[0];
                            var md5 = kvp[1];
                            var src = file;
                            var dst = XFile.PathJoin(Local, Path.GetRelativePath(root, file));
                            if (string.IsNullOrEmpty(md5) == false) dst += "@" + md5; // file@md5
                            var dir = Path.GetDirectoryName(dst);
                            if (XFile.HasDirectory(dir) == false) XFile.CreateDirectory(dir);
                            XFile.CopyFile(src, dst);
                        }
                    }

                    base.Process(report);
                }
            }
        }
    }
}