// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
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
        /// XPuer.Source 实现了源文件管理系统，用于 TypeScript 源文件的解析和多编辑器集成。
        /// </summary>
        /// <remarks>
        /// <code>
        /// 功能特性：
        /// - 源文件解析：自动解析 TypeScript 源文件，分析类继承关系和字段信息
        /// - 多编辑器支持：提供可视化配置选项，允许用户设置代码编辑器工具
        /// 
        /// 使用手册：
        /// 1. 首选项配置
        /// 
        /// 源文件管理工具提供了以下配置选项：
        /// 
        /// 源文件路径：
        /// - 配置键：Puer/Source/Path@Editor
        /// - 默认值：Assets/Scripts/TS
        /// - 功能说明：
        ///   - 设置 TypeScript 源文件的根目录路径
        ///   - 所有的源文件都将从此目录下解析
        ///   - 自动监控目录下的文件变化
        /// 
        /// 编辑器选择：
        /// - 配置键：Puer/Source/Tool@Editor
        /// - 默认值：Auto
        /// - 功能说明：
        ///   - 设置用于打开和编辑源文件的编辑器工具
        ///   - 支持以下编辑器类型：
        ///     - Auto：自动选择系统中可用的编辑器
        ///     - Code：使用 Visual Studio Code
        ///     - Cursor：使用 Cursor 编辑器
        ///     - IDEA：使用 IntelliJ IDEA
        ///   - 根据选择的编辑器自动配置打开方式
        /// 
        /// 2. 文件管理
        /// 
        /// 2.1 自动解析
        /// 系统会自动监控以下情况并触发源文件解析：
        /// - 编辑器启动时
        /// - 导入新的 .ts 或 .mts 文件时
        /// - 修改现有源文件时
        /// 
        /// 解析过程包括：
        /// - 分析类定义和继承关系
        /// - 提取公共字段信息
        /// - 缓存解析结果供运行时使用
        /// 
        /// 2.2 快速访问
        /// 提供多种方式打开源文件：
        /// - 菜单项：
        ///   - Tools/PuerTS/Open Project
        ///   - Assets/PuerTS/Open Project
        ///   - 快捷键：#m
        /// 
        /// 打开文件支持：
        /// - 打开整个项目目录
        /// - 打开单个文件并定位到指定行
        /// - 根据配置的编辑器类型自动选择合适的打开方式
        /// 
        /// 3. 编辑器集成
        /// 
        /// 3.1 Visual Studio Code
        /// - Windows 路径：C:/Users/&lt;用户名&gt;/AppData/Local/Programs/Microsoft VS Code/bin
        /// - macOS 路径：/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code
        /// - 支持功能：
        ///   - 新窗口打开项目
        ///   - 定位到指定文件和行号
        /// 
        /// 3.2 Cursor
        /// - Windows 路径：C:/Users/&lt;用户名&gt;/AppData/Local/Programs/cursor
        /// - macOS 路径：/Applications/Cursor.app/Contents/MacOS/cursor
        /// - 支持功能：
        ///   - 新窗口打开项目
        ///   - 定位到指定文件和行号
        /// 
        /// 3.3 IntelliJ IDEA
        /// - Windows 路径：idea64
        /// - macOS 路径：/Applications/IntelliJ IDEA.app/Contents/MacOS/idea
        /// - 支持功能：
        ///   - 打开项目目录
        ///   - 打开并定位文件
        /// </code>
        /// 更多信息请参考模块文档。
        /// </remarks>
        internal class Source : AssetPostprocessor, XEditor.Event.Internal.OnEditorLoad
        {
            /// <summary>
            /// PuerTS 源文件的首选项设置。
            /// 提供源文件路径和编辑器工具的配置选项。
            /// </summary>
            /// <remarks>
            /// <code>
            /// 配置项说明
            /// 1. 源文件设置
            /// - Path: 源文件根目录路径
            /// - Tool: 编辑器工具选择
            /// 
            /// 2. 编辑器类型
            /// - Auto: 自动选择合适的编辑器
            /// - Code: Visual Studio Code
            /// - Cursor: Cursor 编辑器
            /// - IDEA: IntelliJ IDEA
            /// </code>
            /// </remarks>
            internal class Prefs : EP.U3D.PUER.XPuer.Prefs
            {
                /// <summary>
                /// 编辑器类型枚举。
                /// 定义支持的代码编辑器类型。
                /// </summary>
                internal enum EditorType
                {
                    /// <summary>
                    /// 自动选择编辑器。
                    /// 根据系统环境自动选择可用的编辑器。
                    /// </summary>
                    Auto,

                    /// <summary>
                    /// Visual Studio Code 编辑器。
                    /// 使用 VS Code 打开和编辑源文件。
                    /// </summary>
                    Code,

                    /// <summary>
                    /// Cursor 编辑器。
                    /// 使用 Cursor 打开和编辑源文件。
                    /// </summary>
                    Cursor,

                    /// <summary>
                    /// IntelliJ IDEA 编辑器。
                    /// 使用 IDEA 打开和编辑源文件。
                    /// </summary>
                    IDEA,
                }

                /// <summary>
                /// 源文件路径的配置键。
                /// 用于在编辑器中存储源文件根目录路径。
                /// </summary>
                public const string Path = "Puer/Source/Path@Editor";

                /// <summary>
                /// 源文件路径的默认值。
                /// 默认设置为 Assets/Scripts/TS 目录。
                /// </summary>
                public const string PathDefault = "Assets/Scripts/TS";

                /// <summary>
                /// 编辑器工具的配置键。
                /// 用于在编辑器中存储选择的编辑器类型。
                /// </summary>
                public const string Tool = "Puer/Source/Tool@Editor";

                /// <summary>
                /// 编辑器工具的默认值。
                /// 默认使用自动选择模式。
                /// </summary>
                public static readonly string ToolDefault = EditorType.Auto.ToString();

                /// <summary>
                /// 获取首选项的优先级。
                /// </summary>
                public override int Priority => 304;

                /// <summary>
                /// 在编辑器中可视化首选项设置。
                /// 提供源文件路径和编辑器选择的配置界面。
                /// </summary>
                /// <param name="searchContext">搜索上下文，用于过滤设置项</param>
                public override void OnVisualize(string searchContext)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    foldout = EditorGUILayout.Foldout(foldout, new GUIContent("Source", "Puer Source Options."));
                    if (foldout)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal();
                        Title("Path", "Source Path of Script.");
                        Target.Set(Path, EditorGUILayout.TextField("", Target.GetString(Path, PathDefault)));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        Title("Editor", "Editor Tool of Script.");
                        Enum.TryParse<EditorType>(Target.GetString(Tool, ToolDefault.ToString()), out var editorType);
                        Target.Set(Tool, EditorGUILayout.EnumPopup("", editorType).ToString());
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            /// <summary>
            /// 存储类名的数组。
            /// 缓存解析到的所有类名信息。
            /// </summary>
            internal static string[] Classes;

            /// <summary>
            /// 存储字段信息的字典。
            /// 缓存每个类的字段列表信息。
            /// </summary>
            internal static Dictionary<string, List<string>> Fields;

            /// <summary>
            /// 是否已进行后处理的标志。
            /// 用于控制资源导入后的处理流程。
            /// </summary>
            internal static bool postBatched;

            /// <summary>
            /// 获取事件的优先级。
            /// </summary>
            int XEditor.Event.Callback.Priority => 0;

            /// <summary>
            /// 获取事件是否为单例。
            /// </summary>
            bool XEditor.Event.Callback.Singleton => true;

            /// <summary>
            /// 处理编辑器加载事件。
            /// 在编辑器加载时解析源文件。
            /// </summary>
            /// <param name="args">事件参数</param>
            void XEditor.Event.Internal.OnEditorLoad.Process(params object[] args) { Parse(); }

            /// <summary>
            /// 处理所有资产的后处理。
            /// 监控源文件的变化并触发解析。
            /// </summary>
            /// <param name="importedAssets">导入的资产列表</param>
            /// <param name="deletedAssets">删除的资产列表</param>
            /// <param name="movedAssets">移动的资产列表</param>
            /// <param name="movedFromAssetPaths">移动前的资产路径列表</param>
            internal static void OnPostprocessAllAssets(string[] importedAssets,
                                           string[] deletedAssets,
                                           string[] movedAssets,
                                           string[] movedFromAssetPaths)
            {
                foreach (var asset in importedAssets)
                {
                    if ((asset.EndsWith(".ts") || asset.EndsWith(".mts")) && postBatched == false)
                    {
                        postBatched = true; // Invoke once.
                        XLoom.RunInNext(() =>
                        {
                            postBatched = false;
                            Parse();
                        });
                    }
                }
            }

            /// <summary>
            /// 解析源文件。
            /// 分析 TypeScript 文件，提取类和字段信息。
            /// </summary>
            internal static void Parse()
            {
                static void parseFields(string script, Dictionary<string, string> inherits, Dictionary<string, List<string>> fields, List<string> ffields)
                {
                    var clazz = script.IndexOf("@") > 0 ? script.Split("@")[1] : "";
                    if (fields.TryGetValue(script, out var lfields)) ffields.AddRange(lfields);
                    if (inherits.TryGetValue(clazz, out var super))
                    {
                        foreach (var kvp in fields)
                        {
                            var field = kvp.Key;
                            var idx = field.LastIndexOf("@");
                            if (idx == -1) field = field[(idx + 1)..];
                            if (field == super)
                            {
                                parseFields(kvp.Key, inherits, fields, ffields);
                                break;
                            }
                        }
                    }
                }

                var stime = XTime.GetMillisecond();
                var scripts = new List<string>();
                var fields = new Dictionary<string, List<string>>();
                var inherits = new Dictionary<string, string>();
                var files = new List<string>();
                var root = XPrefs.GetString(Prefs.Path, Prefs.PathDefault);
                XEditor.Utility.CollectFiles(root, files);
                foreach (var file in files)
                {
                    if (file.EndsWith(".ts") || file.EndsWith(".mts"))
                    {
                        var module = XFile.NormalizePath(Path.GetRelativePath(root, file)).Replace(".ts", "").Replace(".mts", "");
                        var lines = File.ReadAllLines(file);
                        var script = "";
                        var skip = false;
                        foreach (var line in lines)
                        {
                            if (line.Contains("class "))
                            {
                                if (line.Contains("export") == false)
                                {
                                    skip = true;
                                    continue;
                                }
                                skip = false;
                                var clazz = line.Replace("class ", "").Replace("export", "");
                                if (clazz.Contains("extends")) clazz = clazz.Split("extends")[0];
                                clazz = clazz.Replace("{", "").Replace("}", "").Replace(" ", "");
                                script = $"{module}@{clazz}";
                                scripts.Add(script);
                                if (line.Contains("extends"))
                                {
                                    var super = line.Split("extends")[1].Replace("{", "").Replace("}", "").Replace(" ", "").Trim();
                                    if (inherits.ContainsKey(clazz)) XLog.Warn("XPuer.Source.Parse: class of {0} was dumplicated in <a href=\"file:///{1}\">{2}</a>.", clazz, Path.GetFullPath(file), file);
                                    else inherits.Add(clazz, super);
                                }
                            }
                            else if (skip) continue;
                            else if (line.Contains("public") && line.Contains(":") && line.Contains("(") == false && line.Trim().StartsWith("//") == false && line.Trim().StartsWith("/*") == false)
                            {
                                var str = line.Replace("public", "").Replace(" ", "").Replace(";", "").Split("=")[0].Trim();
                                var strs = str.Split(":");
                                var name = strs[0];
                                var type = strs[1];
                                if (fields.TryGetValue(script, out var lfields) == false)
                                {
                                    lfields = new List<string>();
                                    fields.Add(script, lfields);
                                }
                                lfields.Add(name);
                                lfields.Add(type);
                            }
                        }
                    }
                }
                Fields = new Dictionary<string, List<string>>();
                foreach (var kvp in fields)
                {
                    var ffields = new List<string>();
                    parseFields(kvp.Key, inherits, fields, ffields);
                    Fields.Add(kvp.Key, ffields);
                }
                Classes = scripts.ToArray();
                XLog.Debug("XPuer.Source.Parse: parsed {0} class(es) at <a href=\"file:///{1}\">{2}</a>, elapsed {3}ms.", Classes.Length, Path.GetFullPath(root), root, XTime.GetMillisecond() - stime);
            }

            /// <summary>
            /// 查找类的索引。
            /// 在已解析的类列表中查找指定类名的索引。
            /// </summary>
            /// <param name="name">要查找的类名</param>
            /// <returns>类的索引，如果未找到则返回 -1</returns>
            internal static int Find(string name)
            {
                if (Classes == null) Parse();
                for (var i = 0; i < Classes.Length; i++)
                {
                    var clazz = Classes[i];
                    if (clazz == name)
                    {
                        return i;
                    }
                }
                return -1;
            }

            /// <summary>
            /// 打开项目。
            /// 使用配置的编辑器打开源文件根目录。
            /// </summary>
            [MenuItem("Tools/PuerTS/Open Project")]
            [MenuItem("Assets/PuerTS/Open Project")]
            [MenuItem("Assets/Open TS Project #m")]
            internal static void Open() { Open(XPrefs.GetString(Prefs.Path, Prefs.PathDefault)); }

            /// <summary>
            /// 打开指定路径的文件或目录。
            /// 使用配置的编辑器打开指定的文件或目录。
            /// </summary>
            /// <param name="path">要打开的文件或目录路径</param>
            /// <param name="line">要打开的行号（仅适用于文件）</param>
            internal static void Open(string path, int line = 0)
            {
                if (!XFile.HasFile(path) && !XFile.HasDirectory(path))
                {
                    XLog.Error("XPuer.Source.Open: cannot found file or directory: {0}", path);
                    return;
                }

                var root = XPrefs.GetString(Prefs.Path, Prefs.PathDefault);
                Enum.TryParse<Prefs.EditorType>(XPrefs.GetString(Prefs.Tool, Prefs.ToolDefault), out var editorType);
                var editorBin = "";

                if (editorType == Prefs.EditorType.Auto)
                {
                    editorBin = XEditor.Cmd.Find("code",
                        $"C:/Users/{Environment.UserName}/AppData/Local/Programs/Microsoft VS Code/bin",
                        "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code");
                    if (!string.IsNullOrEmpty(editorBin)) editorType = Prefs.EditorType.Code;
                    else
                    {
                        editorBin = XEditor.Cmd.Find("cursor",
                            $"C:/Users/{Environment.UserName}/AppData/Local/Programs/cursor",
                            "/Applications/Cursor.app/Contents/MacOS/cursor");
                        if (!string.IsNullOrEmpty(editorBin)) editorType = Prefs.EditorType.Cursor;
                        else
                        {
                            editorBin = XEditor.Cmd.Find("idea64", "/Applications/IntelliJ IDEA.app/Contents/MacOS/idea");
                            if (!string.IsNullOrEmpty(editorBin)) editorType = Prefs.EditorType.IDEA;
                        }
                    }
                }
                else if (editorType == Prefs.EditorType.Code)
                {
                    editorBin = XEditor.Cmd.Find("code",
                        $"C:/Users/{Environment.UserName}/AppData/Local/Programs/Microsoft VS Code/bin",
                        "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code");
                }
                else if (editorType == Prefs.EditorType.Cursor)
                {
                    editorBin = XEditor.Cmd.Find("cursor",
                        $"C:/Users/{Environment.UserName}/AppData/Local/Programs/cursor",
                        "/Applications/Cursor.app/Contents/MacOS/cursor");
                }
                else if (editorType == Prefs.EditorType.IDEA)
                {
                    editorBin = XEditor.Cmd.Find("idea64", "/Applications/IntelliJ IDEA.app/Contents/MacOS/idea");
                }

                if (string.IsNullOrEmpty(editorBin))
                {
                    if (editorType == Prefs.EditorType.Auto && XFile.HasFile(path)) UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, line);
                    else XLog.Error("XPuer.Source.Open: cannot found editor bin of type: {0}", editorType.ToString());
                }
                else
                {
                    if (editorType == Prefs.EditorType.Code || editorType == Prefs.EditorType.Cursor)
                    {
                        if (XFile.HasDirectory(path)) XEditor.Cmd.Run(bin: editorBin, progress: false, args: new string[] { "--new-window", path });
                        else XEditor.Cmd.Run(bin: editorBin, progress: false, args: new string[] { $"{root} --goto {path}:{line}:0" });
                    }
                    else if (editorType == Prefs.EditorType.IDEA)
                    {
                        if (XFile.HasDirectory(path)) XEditor.Cmd.Run(bin: editorBin, progress: false, args: new string[] { path });
                        else XEditor.Cmd.Run(bin: editorBin, progress: false, args: new string[] { $"{root} --line {line} {path}" });
                    }
                    else XLog.Error("XPuer.Source.Open: unsupported editor type: {0}", editorType.ToString());
                }
            }
        }
    }
}