// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using UnityEngine;
using EP.U3D.UTIL;

namespace EP.U3D.PUER
{
    public partial class XPuer
    {
        /// <summary>
        /// XPuer.Prefs 提供了运行时的首选项管理，用于控制运行模式、调试选项和资源路径等配置项。
        /// </summary>
        /// <remarks>
        /// <code>
        /// 功能特性：
        /// - 运行模式配置：提供发布模式和调试模式的切换
        /// - 调试选项管理：支持调试等待和端口设置
        /// - 资源路径配置：支持配置内置、本地和远端资源路径
        /// - 可视化配置界面：在 Unity 编辑器中提供直观的设置面板
        /// 
        /// 使用手册：
        /// 1. 运行模式
        /// 
        /// | 配置项 | 配置键 | 默认值 | 功能说明 |
        /// |--------|--------|--------|----------|
        /// | 发布模式 | Puer/ReleaseMode | false | 控制是否启用发布模式，启用后将禁用所有调试相关功能，用于生产环境部署 |
        /// | 调试模式 | Puer/DebugMode | false | 控制是否启用调试模式，仅在非发布模式下可用，启用后可连接调试器进行调试 |
        /// 
        /// 2. 调试选项
        /// 
        /// | 配置项 | 配置键 | 默认值 | 功能说明 |
        /// |--------|--------|--------|----------|
        /// | 调试等待 | Puer/DebugWait | true | 控制是否等待调试器连接，仅在调试模式下可用，启用后程序将等待调试器连接后再继续执行 |
        /// | 调试端口 | Puer/DebugPort | 9222 | 设置调试器连接的端口号，仅在调试模式下可用，确保端口未被其他程序占用 |
        /// 
        /// 3. 资源路径
        /// 
        /// | 配置项 | 配置键 | 默认值 | 功能说明 |
        /// |--------|--------|--------|----------|
        /// | 内置资源 | Puer/AssetUri | Patch@Scripts@TS.zip | 设置脚本包的内置路径，用于打包时将资源内置于安装包内 |
        /// | 本地资源 | Puer/LocalUri | Scripts/TS | 设置脚本包的本地路径，用于运行时的加载 |
        /// | 远端资源 | Puer/RemoteUri | ${Prefs.Update/PatchUri}/Scripts/TS | 设置脚本包的远端路径，用于资源的下载 |
        /// </code>
        /// 更多信息请参考模块文档。
        /// </remarks>
        public class Prefs : XPrefs.IPanel
        {
            /// <summary>
            /// 发布模式的配置键。
            /// 用于控制是否启用发布模式。
            /// </summary>
            public const string ReleaseMode = "Puer/ReleaseMode";

            /// <summary>
            /// 发布模式的默认值。
            /// 默认为 false，表示不启用发布模式。
            /// </summary>
            public const bool ReleaseModeDefault = false;

            /// <summary>
            /// 调试模式的配置键。
            /// 用于控制是否启用调试模式。
            /// </summary>
            public const string DebugMode = "Puer/DebugMode";

            /// <summary>
            /// 调试模式的默认值。
            /// 默认为 false，表示不启用调试模式。
            /// </summary>
            public const bool DebugModeDefault = false;

            /// <summary>
            /// 调试等待的配置键。
            /// 用于控制是否等待调试器连接。
            /// </summary>
            public const string DebugWait = "Puer/DebugWait";

            /// <summary>
            /// 调试等待的默认值。
            /// 默认为 true，表示等待调试器连接。
            /// </summary>
            public const bool DebugWaitDefault = true;

            /// <summary>
            /// 调试端口的配置键。
            /// 用于设置调试器连接的端口号。
            /// </summary>
            public const string DebugPort = "Puer/DebugPort";

            /// <summary>
            /// 调试端口的默认值。
            /// 默认为 9222。
            /// </summary>
            public const int DebugPortDefault = 9222;

            /// <summary>
            /// 资源 URI 的配置键。
            /// 用于设置脚本资源的打包路径。
            /// </summary>
            public const string AssetUri = "Puer/AssetUri";

            /// <summary>
            /// 资源 URI 的默认值。
            /// 默认为 "Patch@Scripts@TS.zip"。
            /// </summary>
            public const string AssetUriDefault = "Patch@Scripts@TS.zip";

            /// <summary>
            /// 本地 URI 的配置键。
            /// 用于设置脚本资源的本地路径。
            /// </summary>
            public const string LocalUri = "Puer/LocalUri";

            /// <summary>
            /// 本地 URI 的默认值。
            /// 默认为 "Scripts/TS"。
            /// </summary>
            public const string LocalUriDefault = "Scripts/TS";

            /// <summary>
            /// 远程 URI 的配置键。
            /// 用于设置脚本资源的远程路径。
            /// </summary>
            public const string RemoteUri = "Puer/RemoteUri";

            /// <summary>
            /// 远程 URI 的默认值。
            /// 默认为 "${Prefs.Update/PatchUri}/Scripts/TS"。
            /// </summary>
            public const string RemoteUriDefault = "${Prefs.Update/PatchUri}/Scripts/TS";

            /// <summary>
            /// 获取首选项的部分名称。
            /// </summary>
            public override string Section => "Puer";

            /// <summary>
            /// 获取首选项的工具提示。
            /// </summary>
            public override string Tooltip => "Preferences of Puer.";

            /// <summary>
            /// 获取首选项的优先级。
            /// 数值越小优先级越高，默认为 300。
            /// </summary>
            public override int Priority => 300;

#if UNITY_EDITOR
            [SerializeField] protected bool foldout;

            /// <summary>
            /// 可视化首选项设置。
            /// 在 Unity 编辑器中提供可视化的配置界面。
            /// </summary>
            /// <param name="searchContext">搜索上下文</param>
            /// <remarks>
            /// 提供以下配置项：
            /// - Release：发布模式开关
            /// - Debug：调试模式开关（仅在非发布模式下可用）
            /// - Wait：调试等待开关（仅在调试模式下可用）
            /// - Port：调试端口设置（仅在调试模式下可用）
            /// - Asset：脚本资源的打包路径
            /// - Local：脚本资源的本地路径
            /// - Remote：脚本资源的远程路径
            /// </remarks>
            public override void OnVisualize(string searchContext)
            {
                var releaseMode = Target.GetBool(ReleaseMode, ReleaseModeDefault);
                var debugMode = Target.GetBool(DebugMode, DebugModeDefault);

                UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);

                UnityEditor.EditorGUILayout.BeginHorizontal();
                Title("Release", "Switch to Release Mode.");
                releaseMode = UnityEditor.EditorGUILayout.Toggle(releaseMode);
                Target.Set(ReleaseMode, releaseMode);

                var ocolor = GUI.color;
                if (releaseMode) GUI.color = Color.gray;
                Title("Debug", "Switch to Debug Mode.");
                debugMode = UnityEditor.EditorGUILayout.Toggle(debugMode);
                if (!releaseMode) Target.Set(DebugMode, debugMode);
                GUI.color = ocolor;
                UnityEditor.EditorGUILayout.EndHorizontal();

                if (!releaseMode && debugMode)
                {
                    UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                    UnityEditor.EditorGUILayout.BeginHorizontal();

                    Title("Wait", "Wait for the debugger to attach before running.");
                    var debugWait = UnityEditor.EditorGUILayout.Toggle(Target.GetBool(DebugWait, DebugWaitDefault));
                    Target.Set(DebugWait, debugWait);

                    Title("Port", "Debugger Listen Port.");
                    var debugPort = UnityEditor.EditorGUILayout.IntField(Target.GetInt(DebugPort, DebugPortDefault));
                    Target.Set(DebugPort, debugPort);

                    UnityEditor.EditorGUILayout.EndHorizontal();
                    UnityEditor.EditorGUILayout.EndVertical();
                }

                UnityEditor.EditorGUILayout.BeginHorizontal();
                Title("Asset", "Asset Uri of Scripts.");
                var assetFile = UnityEditor.EditorGUILayout.TextField("", Target.GetString(AssetUri, AssetUriDefault));
                Target.Set(AssetUri, assetFile);
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                Title("Local", "Local Uri of Scripts.");
                var localPath = UnityEditor.EditorGUILayout.TextField("", Target.GetString(LocalUri, LocalUriDefault));
                Target.Set(LocalUri, localPath);
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                Title("Remote", "Remote Uri of Scripts.");
                var remoteUri = UnityEditor.EditorGUILayout.TextField("", Target.GetString(RemoteUri, RemoteUriDefault));
                Target.Set(RemoteUri, remoteUri);
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.EndVertical();
            }
#endif
        }
    }
}