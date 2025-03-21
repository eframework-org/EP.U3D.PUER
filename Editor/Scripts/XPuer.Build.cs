// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Collections.Generic;
using EP.U3D.UTIL;
using ET.U3D.UTIL;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using static EP.U3D.PUER.XPuer;

namespace ET.U3D.PUER
{
    public partial class XPuer
    {
        /// <summary>
        /// XPuer.Build 提供了脚本的构建工作流，支持 TypeScript 脚本的编译及打包功能。
        /// </summary>
        /// <remarks>
        /// <code>
        /// 功能特性：
        /// - 首选项配置：提供首选项配置以自定义构建流程
        /// - 自动化流程：提供脚本包构建任务的自动化执行
        /// 
        /// 使用手册：
        /// 1. 首选项配置
        /// 
        /// | 配置项 | 配置键 | 默认值 | 功能说明 |
        /// |--------|--------|--------|----------|
        /// | 输入路径 | Puer/Build/Input@Editor | Assets/Temp/TypeScripts | 设置 TypeScript 编译文件的临时目录 |
        /// | 输出路径 | Puer/Build/Output@Editor | Builds/Patch/Scripts/TS | 设置构建输出的目标目录，按照渠道和平台自动组织目录结构 |
        /// | 构建任务 | Puer/Build/Tasks@Editor | ["build-modules", "build-sources"] | 配置需要执行的 NPM 构建任务，按照配置顺序依次执行任务 |
        /// 
        /// 关联配置项：`Puer/AssetUri`、`Puer/LocalUri`
        /// 
        /// 以上配置项均可在 `EFramework/Preferences/Puer/Build` 首选项编辑器中进行可视化配置。
        /// 
        /// 2. 自动化流程
        /// 
        /// 2.1 构建流程
        /// 
        /// 编译脚本 -> 加密脚本 -> 打包脚本 -> 生成清单
        /// 
        /// 2.2 构建产物
        /// 
        /// 在 Puer/Build/Output@Editor 目录下会生成以下文件：
        /// - *.jsc：脚本包文件，格式为 path_to_scripts.jsc
        /// - Manifest.md5：脚本包清单，格式为 名称|MD5|大小
        /// 
        /// 构建产物会在内置构建事件 XEditor.Event.Internal.OnPreprocessBuild 触发时内置于安装包的资源目录下：
        /// 
        /// - 移动平台 (Android/iOS/..)
        ///   &lt;AssetPath&gt;/
        ///   └── &lt;AssetUri&gt;  # 脚本包压缩为 ZIP
        /// 
        /// - 桌面平台 (Windows/macOS/..)
        ///   &lt;输出目录&gt;_Data/
        ///   └── Local/
        ///       └── &lt;LocalUri&gt;  # 脚本包直接部署
        /// </code>
        /// 更多信息请参考模块文档。
        /// </remarks>
        [XEditor.Tasks.Worker(name: "Build Scripts", group: "Puer", priority: 301)]
        public class Build : XEditor.Tasks.Worker,
            XEditor.Event.Internal.OnPreprocessBuild,
            XEditor.Event.Internal.OnPostprocessBuild
        {
            /// <summary>
            /// 发布构建的首选项设置类。
            /// </summary>
            /// <remarks>
            /// 提供以下配置项：
            /// - Input：脚本源文件路径
            /// - Output：脚本输出路径
            /// - Tasks：构建任务列表
            /// </remarks>
            internal class Prefs : EP.U3D.PUER.XPuer.Prefs
            {
                /// <summary>
                /// 输入路径的配置键。
                /// 用于设置脚本源文件的路径。
                /// </summary>
                public const string Input = "Puer/Build/Input@Editor";

                /// <summary>
                /// 输入路径的默认值。
                /// 默认为 "Assets/Temp/TypeScripts"。
                /// </summary>
                public const string InputDefault = "Assets/Temp/TypeScripts";

                /// <summary>
                /// 输出路径的配置键。
                /// 用于设置构建输出的路径。
                /// </summary>
                public const string Output = "Puer/Build/Output@Editor";

                /// <summary>
                /// 输出路径的默认值。
                /// 默认为 "Builds/Patch/Scripts/TS"。
                /// </summary>
                public const string OutputDefault = "Builds/Patch/Scripts/TS";

                /// <summary>
                /// 任务列表的配置键。
                /// 用于设置构建过程中需要执行的任务。
                /// </summary>
                public const string Tasks = "Puer/Build/Tasks@Editor";

                /// <summary>
                /// 任务列表的默认值。
                /// 默认包含 "build-modules" 和 "build-sources" 两个任务。
                /// </summary>
                public static readonly string[] TasksDefault = new string[] { "build-modules", "build-sources" };

                /// <summary>
                /// 获取首选项的部分名称。
                /// </summary>
                public override string Section => "Puer";

                /// <summary>
                /// 获取首选项的优先级。
                /// 数值越小优先级越高，默认为 301。
                /// </summary>
                public override int Priority => 301;

                [SerializeField] internal string[] tasks;

                /// <summary>
                /// 可视化首选项设置。
                /// 在 Unity 编辑器中提供可视化的配置界面。
                /// </summary>
                /// <param name="searchContext">搜索上下文</param>
                public override void OnVisualize(string searchContext)
                {
                    var serialized = new SerializedObject(this);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    foldout = EditorGUILayout.Foldout(foldout, new GUIContent("Build", "Puer Build Options."));
                    if (foldout)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal();
                        Title("Input", "Input Path of Script Source.");
                        Target.Set(Input, EditorGUILayout.TextField("", Target.GetString(Input, InputDefault)));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        Title("Output", "Output Path of Script Bundle.");
                        Target.Set(Output, EditorGUILayout.TextField("", Target.GetString(Output, OutputDefault)));
                        EditorGUILayout.EndHorizontal();

                        tasks = Target.GetStrings(Tasks, TasksDefault);
                        EditorGUILayout.PropertyField(serialized.FindProperty("tasks"), new GUIContent("Tasks"));
                        if (serialized.ApplyModifiedProperties()) Target.Set(Tasks, tasks);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            internal string buildDir; // 构建目录
            internal string tempDir;  // 临时目录

            /// <summary>
            /// 预处理构建步骤，设置输入和输出路径。
            /// </summary>
            /// <param name="report">构建报告对象</param>
            /// <exception cref="ArgumentNullException">当必要的配置路径为空时抛出</exception>
            public override void Preprocess(XEditor.Tasks.Report report)
            {
                var input = XPrefs.GetString(Prefs.Input, Prefs.InputDefault);
                if (string.IsNullOrEmpty(input)) throw new ArgumentNullException("Prefs.Build.Input is empty.");

                var output = XPrefs.GetString(Prefs.Output, Prefs.OutputDefault);
                if (string.IsNullOrEmpty(output)) throw new ArgumentNullException("Prefs.Build.Output is empty.");

                buildDir = XFile.PathJoin(output, XEnv.Channel, XEnv.Platform.ToString());
                if (XFile.HasDirectory(buildDir)) XFile.DeleteDirectory(buildDir);
                XFile.CreateDirectory(buildDir);

                tempDir = input;
                if (XFile.HasDirectory(tempDir)) XFile.DeleteDirectory(tempDir);
                XFile.CreateDirectory(tempDir);
            }

            /// <summary>
            /// 处理构建步骤，执行构建任务。
            /// </summary>
            /// <param name="report">构建报告对象</param>
            public override void Process(XEditor.Tasks.Report report)
            {
                Puerts.Editor.Generator.UnityMenu.GenV1();
                Gen.GenModule();

                var tasks = XPrefs.GetStrings(Prefs.Tasks, Prefs.TasksDefault);
                foreach (var name in tasks)
                {
                    var task = XEditor.Cmd.Run(bin: XEditor.Cmd.Find("npm"), args: new string[] { "run", name });
                    task.Wait();
                    if (task.Result.Code != 0)
                    {
                        report.Error = $"Run {name} error: {task.Result.Error}";
                        return;
                    }
                }

                var files = new List<string>();
                var builds = new List<string>();
                XEditor.Utility.CollectFiles(tempDir, files);
                foreach (var file in files)
                {
                    if (file.EndsWith(".js") && XFile.HasFile(file))
                    {
                        var encryptFile = file + ".bytes";
                        XFile.SaveText(encryptFile, XString.Encrypt(XFile.OpenText(file), XPrefs.GetString(XEnv.Prefs.Secret, XEnv.Prefs.SecretDefault)));
                        builds.Add(encryptFile);
                    }
                }
                AssetDatabase.Refresh();

                var bundleMap = new Dictionary<string, List<string>>();
                foreach (var file in builds)
                {
                    var bundleName = Const.GenTag(Path.GetRelativePath(tempDir, file));
                    if (bundleMap.TryGetValue(bundleName, out var assets) == false)
                    {
                        assets = new List<string>();
                        bundleMap.Add(bundleName, assets);
                    }
                    var asset = Path.GetRelativePath(XEnv.ProjectPath, file);
                    if (assets.Contains(asset) == false) assets.Add(asset);
                }
                var bundleBuilds = new List<AssetBundleBuild>();
                foreach (var kvp in bundleMap)
                {
                    bundleBuilds.Add(new AssetBundleBuild() { assetBundleName = kvp.Key, assetNames = kvp.Value.ToArray() });
                }
                try
                {
                    if (BuildPipeline.BuildAssetBundles(buildDir, bundleBuilds.ToArray(),
                      BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.AssetBundleStripUnityVersion, EditorUserBuildSettings.activeBuildTarget) == null)
                    {
                        report.Error = "BuildPipeline.BuildAssetBundles returns nil";
                    }

                }
                catch (Exception e) { XLog.Panic(e); report.Error = e.Message; }
            }

            /// <summary>
            /// 后处理构建步骤，清理临时目录并生成清单文件。
            /// </summary>
            /// <param name="report">构建报告对象</param>
            public override void Postprocess(XEditor.Tasks.Report report)
            {
                if (XFile.HasDirectory(tempDir)) XFile.DeleteDirectory(tempDir);

                var maniFile = XFile.PathJoin(buildDir, XMani.Default);
                if (XFile.HasFile(maniFile)) XFile.DeleteFile(maniFile);
                var files = new List<string>();
                XEditor.Utility.CollectFiles(buildDir, files);
                var fs = new FileStream(maniFile, FileMode.CreateNew);
                var sw = new StreamWriter(fs);
                for (var i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    if (file.EndsWith(XEnv.Platform.ToString()) || file.EndsWith(".manifest")) XFile.DeleteFile(file);
                    else
                    {
                        var md5 = XFile.FileMD5(file);
                        var value = XFile.NormalizePath(Path.GetRelativePath(buildDir, file));
                        var size = XFile.FileSize(file);
                        sw.WriteLine(value + "|" + md5 + "|" + size);
                    }
                }
                sw.Close();
                fs.Close();
                AssetDatabase.Refresh();
            }

            /// <summary>
            /// 处理构建前事件，复制脚本并生成模块。
            /// </summary>
            /// <param name="args">事件参数</param>
            void XEditor.Event.Internal.OnPreprocessBuild.Process(params object[] args)
            {
                Puerts.Editor.Generator.UnityMenu.ClearAll();
                Puerts.Editor.Generator.UnityMenu.GenV1();

                var srcDir = XFile.PathJoin(XPrefs.GetString(Prefs.Output, Prefs.OutputDefault), XPrefs.GetString(XEnv.Prefs.Channel, XEnv.Prefs.ChannelDefault), XEnv.Platform.ToString());
                if (!XFile.HasDirectory(srcDir))
                {
                    XLog.Warn("XPuer.Build.OnPreprocessBuild: ignore to copy script(s) because of non-exists dir: {0}.", srcDir);
                    return;
                }
                else
                {
                    if (XEnv.Platform == XEnv.PlatformType.Android || XEnv.Platform == XEnv.PlatformType.iOS)
                    {
                        var dstDir = XFile.PathJoin(XEnv.ProjectPath, "Temp", XPrefs.GetString(Prefs.LocalUri, Prefs.LocalUriDefault));
                        var srcZip = XFile.PathJoin(XEnv.ProjectPath, "Temp", XPrefs.GetString(Prefs.AssetUri, Prefs.AssetUriDefault));
                        var dstZip = XFile.PathJoin(XEnv.AssetPath, XPrefs.GetString(Prefs.AssetUri, Prefs.AssetUriDefault));

                        if (XFile.HasDirectory(dstDir)) XFile.DeleteDirectory(dstDir);
                        XFile.CopyDirectory(srcDir, dstDir, ".manifest");
                        XEditor.Utility.ZipDirectory(dstDir, srcZip);
                        XFile.CopyFile(srcZip, dstZip);

                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        XEditor.Event.Decode<BuildReport>(out var report, args);
                        var outputDir = Path.GetDirectoryName(report.summary.outputPath);
                        var outputName = Path.GetFileNameWithoutExtension(report.summary.outputPath);
                        var dstDir = XFile.PathJoin(outputDir, outputName + "_Data", "Local", XPrefs.GetString(Prefs.LocalUri, Prefs.LocalUriDefault));
                        XFile.CopyDirectory(srcDir, dstDir, ".manifest");
                    }
                    XLog.Debug("XPuer.Build.OnPreprocessBuild: copied script(s) from <a href=\"file:///{0}\">{1}</a>.", Path.GetFullPath(srcDir), srcDir);
                }
            }

            /// <summary>
            /// 处理构建后事件，清理临时文件。
            /// </summary>
            /// <param name="args">事件参数</param>
            void XEditor.Event.Internal.OnPostprocessBuild.Process(params object[] args)
            {
                if (XEnv.Platform == XEnv.PlatformType.Android)
                {
                    var dstZip = XFile.PathJoin(XEnv.AssetPath, XPrefs.GetString(Prefs.AssetUri, Prefs.AssetUriDefault));
                    if (XFile.HasFile(dstZip))
                    {
                        XFile.DeleteFile(dstZip);
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
    }
}