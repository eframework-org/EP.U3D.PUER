// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EP.U3D.UTIL;
using EP.U3D.PUER;

namespace ET.U3D.PUER
{
    public partial class XPuer
    {
        /// <summary>
        /// XPuer.Inspect 实现了 PuerBehaviour 组件的检视器界面，用于组件的可视化编辑和类型检查。
        /// </summary>
        /// <remarks>
        /// <code>
        /// 功能特性：
        /// - 类型支持：支持编辑 number、boolean、string、Vector2/3/4、Color、Object 等多种数据类型
        /// - 数组管理：提供数组类型的动态添加、删除和编辑功能
        /// - 运行时同步：实现编辑器修改与运行时脚本实例的自动同步
        /// - 类型检查：支持组件引用的类型检查和自动匹配
        /// - 搜索过滤：提供脚本类型的快速搜索和过滤功能
        /// 
        /// 使用手册：
        /// 1. 脚本选择
        /// - 使用下拉列表选择要绑定的脚本类型
        /// - 支持搜索框快速过滤可用的脚本类型
        /// - 提供 "Edit" 按钮直接打开源文件进行编辑
        /// 
        /// 2. 属性编辑
        /// 
        /// 2.1 基础类型
        /// 支持以下基础类型的编辑：
        /// - number：数值类型，使用 DoubleField 编辑
        /// - boolean：布尔类型，使用 Toggle 编辑
        /// - string：字符串类型，使用 TextField 编辑
        /// 
        /// 2.2 Unity 类型
        /// 支持以下 Unity 内置类型的编辑：
        /// - Vector2：二维向量，使用 Vector2Field 编辑
        /// - Vector3：三维向量，使用 Vector3Field 编辑
        /// - Vector4：四维向量，使用 Vector4Field 编辑
        /// - Color：颜色类型，使用 ColorField 编辑
        /// 
        /// 2.3 引用类型
        /// 支持以下引用类型的编辑：
        /// - UnityEngine.Object：Unity 对象引用
        /// - PuerBehaviour：PuerTS 组件引用
        /// 
        /// 3. 数组操作
        /// 
        /// 3.1 数组管理
        /// - 使用折叠面板显示数组内容
        /// - 支持通过输入框或加减按钮调整数组长度
        /// - 提供数组元素的添加和删除功能
        /// 
        /// 3.2 元素编辑
        /// - 支持所有基础类型的数组元素编辑
        /// - 支持所有 Unity 类型的数组元素编辑
        /// - 支持所有引用类型的数组元素编辑
        /// - 提供每个元素的单独删除按钮
        /// 
        /// 4. 运行时同步
        /// 
        /// 4.1 值类型同步
        /// - 自动将编辑器中的修改同步到运行时实例
        /// - 支持基础类型和 Unity 类型的实时同步
        /// - 使用二进制序列化确保数据一致性
        /// 
        /// 4.2 引用类型同步
        /// - 自动同步对象引用的修改
        /// - 支持组件引用的动态更新
        /// - 在类型变更时自动处理引用关系
        /// </code>
        /// 更多信息请参考模块文档。
        /// </remarks>
        [CustomEditor(typeof(PuerBehaviour), true)]
        [CanEditMultipleObjects]
        internal class Inspect : Editor
        {
            /// <summary>
            /// 帮助文本。
            /// 说明支持的数据类型和使用方法。
            /// </summary>
            internal readonly string helpText =
                "[Type] number, boolean, string, UnityEngine.Vector2/3/4, UnityEngine.Color, UnityEngine.Object, PuerBehaviour.\n" +
                "[Usage] PuerBehaviour.AddComponent to add component dynamicly.";

            /// <summary>
            /// 缓存的所有类型列表。
            /// 用于类型查找和引用检查。
            /// </summary>
            internal static List<Type> allTypes = null;

            /// <summary>
            /// 所有检查器实例的列表。
            /// 用于管理多个检查器的状态。
            /// </summary>
            internal static List<Inspect> allInspectors;

            /// <summary>
            /// PuerBehaviour 实例的展开状态映射。
            /// 记录每个实例中数组类型属性的展开状态。
            /// </summary>
            internal static readonly Dictionary<PuerBehaviour, Dictionary<string, bool>> allExpands = new();

            /// <summary>
            /// 当前正在编辑的 PuerBehaviour 实例。
            /// </summary>
            internal PuerBehaviour activeInstance;

            /// <summary>
            /// 当前选中的脚本索引。
            /// </summary>
            internal int selectedScript = -1;

            /// <summary>
            /// 脚本搜索文本。
            /// 用于过滤可用的脚本列表。
            /// </summary>
            internal string searchText;

            /// <summary>
            /// 初始化检查器。
            /// 设置活动实例并注册到检查器列表。
            /// </summary>
            internal void OnEnable()
            {
                activeInstance = target as PuerBehaviour;
                allInspectors ??= new List<Inspect>();
                if (!allInspectors.Contains(this)) allInspectors.Add(this);
                selectedScript = Source.Find(activeInstance.Clazz);
            }

            /// <summary>
            /// 禁用检查器。
            /// 从检查器列表中移除当前实例。
            /// </summary>
            internal void OnDisable()
            {
                if (allInspectors != null && allInspectors.Contains(this)) allInspectors.Remove(this);
            }

            /// <summary>
            /// 销毁检查器。
            /// 清理当前实例的展开状态记录。
            /// </summary>
            internal void OnDestroy()
            {
                if (activeInstance) allExpands.Remove(activeInstance);
            }

            /// <summary>
            /// 绘制检查器界面。
            /// 提供脚本选择、属性编辑和数组操作的界面。
            /// </summary>
            public override void OnInspectorGUI()
            {
                if (activeInstance == null) return;
                EditorGUILayout.HelpBox(helpText, MessageType.Info);

                if (allTypes == null)
                {
                    allTypes = new List<Type>();
                    allTypes.AddRange(TypeCache.GetTypesDerivedFrom<object>());
                }

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();

                if (selectedScript == -1 || selectedScript >= Source.Classes.Length) selectedScript = Source.Find(activeInstance.Clazz);
                else if (Source.Classes[selectedScript] != activeInstance.Clazz) selectedScript = Source.Find(activeInstance.Clazz);

                if (!Application.isPlaying) selectedScript = EditorGUILayout.IntPopup(selectedScript, Source.Classes, null, GUILayout.Height(15));
                else selectedScript = EditorGUILayout.IntPopup(selectedScript, Source.Classes, null, GUILayout.Height(15));

                if (GUILayout.Button(new GUIContent("Edit"), GUILayout.Height(18), GUILayout.Width(60)))
                {
                    if (selectedScript != -1)
                    {
                        var strs = Source.Classes[selectedScript].Split("@");
                        var module = strs[0];
                        var type = strs[1];
                        var script = XFile.PathJoin(XPrefs.GetString(Source.Prefs.Path, Source.Prefs.PathDefault), module + ".ts");
                        var idx = 0;
                        if (XFile.HasFile(script))
                        {
                            var lines = File.ReadAllLines(script);
                            for (var i = 0; i < lines.Length; i++)
                            {
                                var line = lines[i];
                                if (line.Contains($"class {type}"))
                                {
                                    idx = i; break;
                                }
                            }
                            Source.Open(script, idx + 1);
                        }
                    }
                }

                if (Application.isPlaying == false)
                {
                    var hint = false;
                    if (string.IsNullOrEmpty(searchText))
                    {
                        searchText = "Search";
                        hint = true;
                    }
                    if (hint) GUI.color = Color.gray;
                    searchText = EditorGUILayout.TextField("", searchText, GUILayout.Width(60));
                    if (hint) GUI.color = Color.white;
                    if (searchText == "Search") searchText = string.Empty;
                }
                GUILayout.EndHorizontal();

                if (Application.isPlaying == false && !string.IsNullOrEmpty(searchText))
                {
                    for (int i = 0; i < Source.Classes.Length; i++)
                    {
                        var str = Source.Classes[i];
                        if (str.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (GUILayout.Button(new GUIContent(str)))
                            {
                                selectedScript = i;
                                searchText = "";
                            }
                        }
                    }
                }

                GUILayout.EndVertical();

                if (selectedScript == -1)
                {
                    EditorGUILayout.HelpBox("Please select a script or remove this component.", MessageType.Error);
                }
                else
                {
                    var clazz = Source.Classes[selectedScript];
                    activeInstance.Clazz = clazz;
                    if (Source.Fields.TryGetValue(clazz, out var fields))
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);

                        for (var i = 0; i < fields.Count;)
                        {
                            var name = fields[i];
                            var type = fields[i + 1];
                            var arr = false;
                            if (type.Contains("[") || type.Contains("Array<"))
                            {
                                arr = true;
                                type = type.Replace("[", "").Replace("]", "").Replace("Array<", "").Replace(">", "").Trim();
                            }
                            var ret = activeInstance.Fields.Find((ele) => { return ele.Key == name; });
                            if (ret == null)
                            {
                                ret = new PuerBehaviour.Field { Key = name };
                                activeInstance.Fields.Add(ret);
                            }
                            if (type != ret.Type) ret.Reset();
                            ret.Type = type;
                            ret.BTArray = arr;
                            ret.BLBValue = type == "number" || type == "boolean" ||
                                type == "Vector2" || type == "UnityEngine.Vector2" ||
                                 type == "Vector3" || type == "UnityEngine.Vector3" ||
                                  type == "Vector4" || type == "UnityEngine.Vector4" ||
                                  type == "Color" || type == "UnityEngine.Color" ||
                                  type == "string";
                            i += 2;
                        }
                        for (int i = 0; i < activeInstance.Fields.Count;)
                        {
                            var field = activeInstance.Fields[i];
                            if (fields.Contains(field.Key) == false)
                            {
                                activeInstance.Fields.Remove(field);
                            }
                            else
                            {
                                i++;
                                if (field.BTArray)
                                {
                                    int count = field.BLBValue ? (field.LBValue != null ? field.LBValue.Count : 0) : (field.LOValue != null ? field.LOValue.Count : 0);

                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Space(10);
                                    if (allExpands.TryGetValue(activeInstance, out var expands) == false)
                                    {
                                        expands = new Dictionary<string, bool>();
                                        allExpands.Add(activeInstance, expands);
                                    }

                                    expands.TryGetValue(field.Key, out var expand);
                                    expand = EditorGUILayout.Foldout(expand, $"{field.Key}[{count}]");
                                    expands[field.Key] = expand;
                                    EditorGUILayout.EndHorizontal();

                                    if (expand)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        GUILayout.Space(20);
                                        GUILayout.BeginVertical(EditorStyles.helpBox);
                                        EditorGUILayout.BeginHorizontal();
                                        GUILayout.Label("Length", GUILayout.Width(70));
                                        var ncount = Mathf.Clamp(EditorGUILayout.IntField(count), 0, ushort.MaxValue);
                                        if (GUILayout.Button(string.Empty, "OL Minus", GUILayout.Width(20)) && count > 0) ncount = count - 1;
                                        if (GUILayout.Button(string.Empty, "OL Plus", GUILayout.Width(20)) && count < ushort.MaxValue) ncount = count + 1;
                                        if (ncount != count)
                                        {
                                            if (ncount > count)
                                            {
                                                if (field.BLBValue && field.LBValue == null) field.LBValue = new List<PuerBehaviour.Byte>();
                                                else if (!field.BLBValue && field.LOValue == null) field.LOValue = new List<UnityEngine.Object>();
                                                for (var j = count; j < ncount; j++)
                                                {
                                                    if (field.BLBValue)
                                                    {
                                                        var data = new PuerBehaviour.Byte();
                                                        field.LBValue.Add(data);
                                                        if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, data, 1);
                                                    }
                                                    else
                                                    {
                                                        field.LOValue.Add(null);
                                                        if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, null, 1);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                for (var j = ncount; j < count; j++)
                                                {
                                                    if (field.BLBValue) field.LBValue.RemoveAt(j);
                                                    else field.LOValue.RemoveAt(j);
                                                    if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, null, 3 << 2 | j);
                                                }
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();

                                        for (var j = 0; j < ncount; j++)
                                        {
                                            if (j == 0) GUILayout.Space(5);
                                            EditorGUILayout.BeginHorizontal();
                                            GUILayout.Label($"Element {j}", GUILayout.Width(70));
                                            if (field.Type == "number")
                                            {
                                                var v = BitConverter.ToDouble(field.LBValue[j].Data, 0);
                                                v = EditorGUILayout.DoubleField(v);
                                                var d = BitConverter.GetBytes(v);
                                                field.LBValue[j].Data = d;
                                                if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 2 << 2 | j);
                                            }
                                            else if (field.Type == "boolean")
                                            {
                                                var v = BitConverter.ToBoolean(field.LBValue[j].Data, 0);
                                                v = EditorGUILayout.Toggle(v);
                                                var d = BitConverter.GetBytes(v);
                                                field.LBValue[j].Data = d;
                                                if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 2 << 2 | j);
                                            }
                                            else if (field.Type == "Vector2" || field.Type == "UnityEngine.Vector2")
                                            {
                                                var v = XObject.FromByte<Vector2>(field.LBValue[j].Data);
                                                v = EditorGUILayout.Vector2Field("", v);
                                                var d = XObject.ToByte(v);
                                                field.LBValue[j].Data = d;
                                                if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 2 << 2 | j);
                                            }
                                            else if (field.Type == "Vector3" || field.Type == "UnityEngine.Vector3")
                                            {
                                                var v = XObject.FromByte<Vector3>(field.LBValue[j].Data);
                                                v = EditorGUILayout.Vector3Field("", v);
                                                var d = XObject.ToByte(v);
                                                field.LBValue[j].Data = d;
                                                if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 2 << 2 | j);
                                            }
                                            else if (field.Type == "Vector4" || field.Type == "UnityEngine.Vector4")
                                            {
                                                var v = XObject.FromByte<Vector4>(field.LBValue[j].Data);
                                                v = EditorGUILayout.Vector4Field("", v);
                                                var d = XObject.ToByte(v);
                                                field.LBValue[j].Data = d;
                                                if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 2 << 2 | j);
                                            }
                                            else if (field.Type == "Color" || field.Type == "UnityEngine.Color")
                                            {
                                                var v = XObject.FromByte<Color>(field.LBValue[j].Data);
                                                v = EditorGUILayout.ColorField("", v);
                                                var d = XObject.ToByte(v);
                                                field.LBValue[j].Data = d;
                                                if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 2 << 2 | j);
                                            }
                                            else if (field.Type == "string")
                                            {
                                                var v = Encoding.UTF8.GetString(field.LBValue[j].Data);
                                                v = EditorGUILayout.TextField(field.Key, v);
                                                var d = Encoding.UTF8.GetBytes(v);
                                                field.LBValue[j].Data = d;
                                                if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 2 << 2 | j);
                                            }
                                            else
                                            {
                                                Type ftype = null;
                                                for (int k = 0; k < allTypes.Count; k++)
                                                {
                                                    var type = allTypes[k];
                                                    if (type != null && type.Name == field.Type)
                                                    {
                                                        ftype = type;
                                                        allTypes.RemoveAt(k);
                                                        allTypes.Insert(0, type);
                                                        break;
                                                    }
                                                }
                                                if (ftype != null && ftype.IsSubclassOf(typeof(UnityEngine.Object)))
                                                {
                                                    field.LOValue[j] = EditorGUILayout.ObjectField("", field.LOValue[j], ftype, true);
                                                    if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, field.LOValue[j], 2 << 2 | j);
                                                }
                                                else
                                                {
                                                    var v = field.LOValue[j] as PuerBehaviour;
                                                    v = EditorGUILayout.ObjectField("", v, typeof(PuerBehaviour), true) as PuerBehaviour;
                                                    var b = false;
                                                    if (v)
                                                    {
                                                        if (v.Clazz.EndsWith(field.Type))
                                                        {
                                                            field.LOValue[j] = v;
                                                            b = true;
                                                            if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, field.LOValue[j], 2 << 2 | j);
                                                        }
                                                        else
                                                        {
                                                            var vv = v.GetComponents<PuerBehaviour>();
                                                            foreach (var tv in vv)
                                                            {
                                                                if (tv.Clazz.EndsWith(field.Type))
                                                                {
                                                                    field.LOValue[j] = tv;
                                                                    b = true;
                                                                    if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, field.LOValue[j], 2 << 2 | j);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (!b) field.LOValue[j] = null;
                                                }
                                            }
                                            if (GUILayout.Button(string.Empty, "WinBtnClose"))
                                            {
                                                if (field.BLBValue) field.LBValue.RemoveAt(j);
                                                else field.LOValue.RemoveAt(j);
                                                if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, null, 3 << 2 | j);

                                            }
                                            EditorGUILayout.EndHorizontal();
                                        }

                                        EditorGUILayout.EndVertical();
                                        EditorGUILayout.EndHorizontal();
                                    }
                                }
                                else
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(10);
                                    if (field.Type == "number")
                                    {
                                        var v = BitConverter.ToDouble(field.BValue, 0);
                                        v = EditorGUILayout.DoubleField(field.Key, v);
                                        field.BValue = BitConverter.GetBytes(v);
                                        if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 0);
                                    }
                                    else if (field.Type == "boolean")
                                    {
                                        var v = BitConverter.ToBoolean(field.BValue, 0);
                                        v = EditorGUILayout.Toggle(field.Key, v);
                                        field.BValue = BitConverter.GetBytes(v);
                                        if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 0);
                                    }
                                    else if (field.Type == "Vector2" || field.Type == "UnityEngine.Vector2")
                                    {
                                        var v = XObject.FromByte<Vector2>(field.BValue);
                                        v = EditorGUILayout.Vector2Field(field.Key, v);
                                        field.BValue = XObject.ToByte(v);
                                        if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 0);
                                    }
                                    else if (field.Type == "Vector3" || field.Type == "UnityEngine.Vector3")
                                    {
                                        var v = XObject.FromByte<Vector3>(field.BValue);
                                        v = EditorGUILayout.Vector3Field(field.Key, v);
                                        field.BValue = XObject.ToByte(v);
                                        if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 0);
                                    }
                                    else if (field.Type == "Vector4" || field.Type == "UnityEngine.Vector4")
                                    {
                                        var v = XObject.FromByte<Vector4>(field.BValue);
                                        v = EditorGUILayout.Vector4Field(field.Key, v);
                                        field.BValue = XObject.ToByte(v);
                                        if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 0);
                                    }
                                    else if (field.Type == "Color" || field.Type == "UnityEngine.Color")
                                    {
                                        var v = XObject.FromByte<Color>(field.BValue);
                                        v = EditorGUILayout.ColorField(field.Key, v);
                                        field.BValue = XObject.ToByte(v);
                                        if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 0);
                                    }
                                    else if (field.Type == "string")
                                    {
                                        var v = Encoding.UTF8.GetString(field.BValue);
                                        v = EditorGUILayout.TextField(field.Key, v);
                                        field.BValue = Encoding.UTF8.GetBytes(v);
                                        if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, v, 0);
                                    }
                                    else
                                    {
                                        Type ftype = null;
                                        for (int j = 0; j < allTypes.Count; j++)
                                        {
                                            var type = allTypes[j];
                                            if (type != null && type.Name == field.Type)
                                            {
                                                ftype = type;
                                                allTypes.RemoveAt(j);
                                                allTypes.Insert(0, type);
                                                break;
                                            }
                                        }
                                        if (ftype != null && ftype.IsSubclassOf(typeof(UnityEngine.Object)))
                                        {
                                            field.OValue = EditorGUILayout.ObjectField(field.Key, field.OValue, ftype, true);
                                            if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, field.OValue, 0);
                                        }
                                        else
                                        {
                                            var v = field.OValue as PuerBehaviour;
                                            v = EditorGUILayout.ObjectField(field.Key, v, typeof(PuerBehaviour), true) as PuerBehaviour;
                                            var b = false;
                                            if (v)
                                            {
                                                if (v.Clazz.EndsWith(field.Type))
                                                {
                                                    field.OValue = v;
                                                    b = true;
                                                    if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, field.OValue, 0);
                                                }
                                                else
                                                {
                                                    var vv = v.GetComponents<PuerBehaviour>();
                                                    foreach (var tv in vv)
                                                    {
                                                        if (tv.Clazz.EndsWith(field.Type))
                                                        {
                                                            field.OValue = tv;
                                                            b = true;
                                                            if (Application.isPlaying) EP.U3D.PUER.XPuer.InitField(activeInstance.JProxy, field.Key, field.OValue, 0);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (!b) field.OValue = null;
                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                        }

                        GUILayout.EndVertical();
                    }
                }
                if (GUI.changed) EditorUtility.SetDirty(target);
            }
        }
    }
}