// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Puerts;
using EP.U3D.UTIL;

namespace EP.U3D.PUER
{
    /// <summary>
    /// XPuer.Comp 提供了 MonoBehaviour 的 TypeScript 扩展实现，用于管理组件的生命周期、序列化和事件系统。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性：
    /// - 支持 TypeScript 类的序列化：通过 Field 类提供字段序列化能力
    /// - 实现 Unity 生命周期管理：支持 Awake、Start、Update 等标准事件
    /// - 提供物理事件系统：支持 OnTrigger 和 OnCollision 系列事件
    /// - 支持动态组件添加：通过 Add 方法实现运行时组件创建
    /// - 实现组件查找机制：支持在父子层级中查找指定类型组件
    /// 
    /// 使用手册：
    /// 1. 组件管理
    /// 
    /// 1.1 添加组件
    /// 通过 Add 方法在运行时动态添加组件到游戏对象：
    /// // parentObj: 父节点对象
    /// // path: 节点路径，空字符串表示当前节点
    /// // type: TypeScript 类型对象
    /// var comp = PuerBehaviour.Add(parentObj, path, type);
    /// 
    /// 1.2 获取组件
    /// 提供多种方式获取组件实例：
    /// // 获取当前节点组件
    /// PuerBehaviour comp = PuerBehaviour.Get(gameObject, type);
    /// 
    /// // 获取父节点组件（包含自身）
    /// PuerBehaviour parent = PuerBehaviour.GetInParent(gameObject, type, includeInactive);
    /// 
    /// // 获取子节点组件（包含自身）
    /// PuerBehaviour child = PuerBehaviour.GetInChildren(gameObject, type, includeInactive);
    /// 
    /// // 获取多个组件
    /// PuerBehaviour[] comps = PuerBehaviour.Gets(gameObject, type);
    /// PuerBehaviour[] parents = PuerBehaviour.GetsInParent(gameObject, type, includeInactive);
    /// PuerBehaviour[] children = PuerBehaviour.GetsInChildren(gameObject, type, includeInactive);
    /// 
    /// 2. 序列化系统
    /// 
    /// 2.1 字段序列化
    /// 支持以下类型的序列化：
    /// public class Field
    /// {
    ///     public string Key;                     // 字段名称
    ///     public string Type;                    // 字段类型
    ///     public UnityEngine.Object OValue;      // Object 类型值
    ///     public byte[] BValue;                  // 值类型数据
    ///     public List&lt;UnityEngine.Object&gt; LOValue; // Object 数组
    ///     public List&lt;Byte&gt; LBValue;            // 值类型数组
    /// }
    /// 
    /// 3. 生命周期
    /// 
    /// 3.1 标准事件
    /// 自动检测并调用 TypeScript 类中定义的生命周期方法：
    /// - Awake：组件初始化时
    /// - Start：首次启用时
    /// - OnEnable：启用时
    /// - OnDisable：禁用时
    /// - Update：每帧更新时
    /// - LateUpdate：所有更新完成后
    /// - FixedUpdate：固定时间间隔
    /// - OnDestroy：销毁时
    /// 
    /// 3.2 物理事件
    /// 自动包装并转发物理碰撞事件：
    /// - OnTriggerEnter：触发器进入
    /// - OnTriggerStay：触发器停留
    /// - OnTriggerExit：触发器退出
    /// - OnCollisionEnter：碰撞开始
    /// - OnCollisionStay：碰撞持续
    /// - OnCollisionExit：碰撞结束
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    [AddComponentMenu("Puer/Puer Behaviour")]
    public class PuerBehaviour : MonoBehaviour
    {
        #region 数据结构定义
        /// <summary>
        /// 序列化字节类，用于存储二进制数据。
        /// </summary>
        [Serializable]
        public class Byte
        {
            /// <summary>
            /// 字节数据数组。
            /// </summary>
            public byte[] Data;

            /// <summary>
            /// 构造一个新的字节对象。
            /// </summary>
            /// <param name="data">初始字节数据，如果为 null 则创建长度为 16 的数组</param>
            public Byte(byte[] data = null)
            {
                if (data == null) Data = new byte[16];
                else Data = data;
            }
        }

        /// <summary>
        /// 序列化字段类，用于存储组件的字段数据。
        /// </summary>
        [Serializable]
        public class Field
        {
            /// <summary>
            /// 字段名称。
            /// </summary>
            public string Key;

            /// <summary>
            /// 字段类型。
            /// </summary>
            public string Type;

            /// <summary>
            /// UnityEngine.Object 类型的数据。
            /// </summary>
            public UnityEngine.Object OValue;

            /// <summary>
            /// 值类型数据，使用字节数组存储。
            /// 最大支持 16 字节的结构体（如 Vector4）。
            /// </summary>
            public byte[] BValue = new byte[16];

            /// <summary>
            /// UnityEngine.Object 数组类型的数据。
            /// </summary>
            public List<UnityEngine.Object> LOValue;

            /// <summary>
            /// 值类型数组数据。
            /// </summary>
            public List<Byte> LBValue;

            /// <summary>
            /// 标识字段是否为数组类型。
            /// </summary>
            public bool BTArray = false;

            /// <summary>
            /// 标识数组元素是否为值类型。
            /// </summary>
            public bool BLBValue = false;

            /// <summary>
            /// 重置字段的所有值为默认状态。
            /// </summary>
            public void Reset()
            {
                Type = "";
                OValue = null;
                BValue = new byte[16];
                LOValue = null;
                LBValue = null;
                BTArray = false;
                BLBValue = false;
            }
        }

        /// <summary>
        /// JavaScript 类型包装器，用于管理 TypeScript 类的实例和其生命周期方法。
        /// </summary>
        public class JSType
        {
            public JSObject Type;
            public bool BAwake;
            public bool BOnEnable;
            public bool BStart;
            public bool BOnDisable;
            public bool BUpdate;
            public bool BLateUpdate;
            public bool BFixedUpdate;
            public bool BOnDestroy;
            public bool BOnTriggerEnter;
            public bool BOnTriggerStay;
            public bool BOnTriggerExit;
            public bool BOnCollisionEnter;
            public bool BOnCollisionStay;
            public bool BOnCollisionExit;

            /// <summary>
            /// 构造一个新的 JavaScript 类型包装器。
            /// </summary>
            /// <param name="type">JavaScript 类型对象</param>
            /// <param name="obj">JavaScript 对象实例</param>
            public JSType(JSObject type, JSObject obj)
            {
                Type = type;
                BAwake = obj.Get<Action>("Awake") != null;
                BOnEnable = obj.Get<Action>("OnEnable") != null;
                BStart = obj.Get<Action>("Start") != null;
                BOnDisable = obj.Get<Action>("OnDisable") != null;
                BUpdate = obj.Get<Action>("Update") != null;
                BLateUpdate = obj.Get<Action>("LateUpdate") != null;
                BFixedUpdate = obj.Get<Action>("FixedUpdate") != null;
                BOnDestroy = obj.Get<Action>("OnDestroy") != null;
                BOnTriggerEnter = obj.Get<Action<Collider>>("OnTriggerEnter") != null;
                BOnTriggerStay = obj.Get<Action<Collider>>("OnTriggerStay") != null;
                BOnTriggerExit = obj.Get<Action<Collider>>("OnTriggerExit") != null;
                BOnCollisionEnter = obj.Get<Action<Collision>>("OnCollisionEnter") != null;
                BOnCollisionStay = obj.Get<Action<Collision>>("OnCollisionStay") != null;
                BOnCollisionExit = obj.Get<Action<Collision>>("OnCollisionExit") != null;
            }
        }

        /// <summary>
        /// 碰撞进入事件包装器。
        /// </summary>
        internal class CollisionEnterWrap : MonoBehaviour
        {
            internal Action<Collision> Func;

            internal void OnCollisionEnter(Collision collision) { if (!enabled) return; Func?.Invoke(collision); }

            internal static CollisionEnterWrap Get(GameObject obj, Action<Collision> func)
            {
                var wrap = obj.GetComponent<CollisionEnterWrap>();
                if (wrap == null) wrap = obj.AddComponent<CollisionEnterWrap>();
                if (func != null) wrap.Func = func;
                return wrap;
            }
        }

        /// <summary>
        /// 碰撞退出事件包装器。
        /// </summary>
        internal class CollisionExitWrap : MonoBehaviour
        {
            internal Action<Collision> Func;

            internal void OnCollisionExit(Collision collision) { if (!enabled) return; Func?.Invoke(collision); }

            internal static CollisionExitWrap Get(GameObject obj, Action<Collision> func)
            {
                var wrap = obj.GetComponent<CollisionExitWrap>();
                if (wrap == null) wrap = obj.AddComponent<CollisionExitWrap>();
                if (func != null) wrap.Func = func;
                return wrap;
            }
        }

        /// <summary>
        /// 碰撞持续事件包装器。
        /// </summary>
        internal class CollisionStayWrap : MonoBehaviour
        {
            internal Action<Collision> Func;

            internal void OnCollisionStay(Collision collision) { if (!enabled) return; Func?.Invoke(collision); }

            internal static CollisionStayWrap Get(GameObject obj, Action<Collision> func)
            {
                var wrap = obj.GetComponent<CollisionStayWrap>();
                if (wrap == null) wrap = obj.AddComponent<CollisionStayWrap>();
                if (func != null) wrap.Func = func;
                return wrap;
            }
        }

        /// <summary>
        /// 触发器进入事件包装器。
        /// </summary>
        internal class TriggerEnterWrap : MonoBehaviour
        {
            internal Action<Collider> Func;

            internal void OnTriggerEnter(Collider other) { if (!enabled) return; Func?.Invoke(other); }

            internal static TriggerEnterWrap Get(GameObject obj, Action<Collider> func)
            {
                var wrap = obj.GetComponent<TriggerEnterWrap>();
                if (wrap == null) wrap = obj.AddComponent<TriggerEnterWrap>();
                if (func != null) wrap.Func = func;
                return wrap;
            }
        }

        /// <summary>
        /// 触发器退出事件包装器。
        /// </summary>
        internal class TriggerExitWrap : MonoBehaviour
        {
            internal Action<Collider> Func;

            internal void OnTriggerExit(Collider other) { if (!enabled) return; Func?.Invoke(other); }

            internal static TriggerExitWrap Get(GameObject obj, Action<Collider> func)
            {
                var wrap = obj.GetComponent<TriggerExitWrap>();
                if (wrap == null) wrap = obj.AddComponent<TriggerExitWrap>();
                if (func != null) wrap.Func = func;
                return wrap;
            }
        }

        /// <summary>
        /// 触发器持续事件包装器。
        /// </summary>
        internal class TriggerStayWrap : MonoBehaviour
        {
            internal Action<Collider> Func;

            internal void OnTriggerStay(Collider other) { if (!enabled) return; Func?.Invoke(other); }

            internal static TriggerStayWrap Get(GameObject obj, Action<Collider> func)
            {
                var wrap = obj.GetComponent<TriggerStayWrap>();
                if (wrap == null) wrap = obj.AddComponent<TriggerStayWrap>();
                if (func != null) wrap.Func = func;
                return wrap;
            }
        }
        #endregion

        #region 成员字段定义
        /// <summary>
        /// 类型全称。
        /// </summary>
        public string Clazz;

        /// <summary>
        /// 序列化字段列表。
        /// </summary>
        public List<Field> Fields = new();

        /// <summary>
        /// 是否已初始化。
        /// </summary>
        public int Inited { get; set; }

        /// <summary>
        /// 是否初始化成功。
        /// </summary>
        public bool InitOK { get; set; }

        /// <summary>
        /// JavaScript 对象实例。
        /// </summary>
        public JSObject JProxy { get; set; }

        /// <summary>
        /// JavaScript 类型信息。
        /// </summary>
        public JSType JType { get; set; }

        /// <summary>
        /// 动态类型对象。
        /// </summary>
        public static JSObject DType = null;

        internal static Dictionary<string, JSType> staticTypes = new();
        internal static Dictionary<JSObject, JSType> dynamicTypes = new();
        internal static List<PuerBehaviour> enableList = new();
        internal static List<Action> OnUpdateEvent = new();
        internal static List<Action> onLateUpdateEvent = new();
        internal static List<Action> onFixedUpdateEvent = new();
        internal bool physicsWrapped;
        internal TriggerEnterWrap onTriggerEnter;
        internal TriggerStayWrap onTriggerStay;
        internal TriggerExitWrap onTriggerExit;
        internal CollisionEnterWrap onCollisionEnter;
        internal CollisionStayWrap onCollisionStay;
        internal CollisionExitWrap onCollisionExit;
        #endregion

        #region 反序列化实现
        /// <summary>
        /// 初始化动态组件。
        /// 当组件是通过动态方式添加时，使用此方法进行初始化。
        /// </summary>
        internal void InitDynamic()
        {
            if (Inited > 1) { }
            else
            {
                Inited = 2;
                try
                {
                    CtorProxy(DType);
                    object p = null;
                    InitProxy(ref p, Inited);
                    InitOK = true;
                }
                catch (Exception e)
                {
                    XLog.Panic(e, $"PuerBehaviour.InitDynamic: init component({name}) of type: {DType} error.");
                    enabled = false;
                    return;
                }
                finally { DType = null; }
            }
        }

        /// <summary>
        /// 初始化静态组件。
        /// 当组件是通过预制体或场景中的静态方式添加时，使用此方法进行初始化。
        /// </summary>
        /// <param name="proxy">代理对象的引用</param>
        /// <param name="init">初始化状态</param>
        internal void InitStatic(ref object proxy, int init)
        {
            if (Inited == init) { proxy = JProxy; }
            else
            {
                Inited = init;
                CtorProxy();
                InitProxy(ref proxy, init);
                InitOK = true;
            }
        }

        /// <summary>
        /// 构造代理对象。
        /// 创建并初始化 JavaScript 类型的实例。
        /// </summary>
        internal void CtorProxy()
        {
            try
            {
                if (staticTypes.TryGetValue(Clazz, out var ele) == false)
                {
                    var idx = Clazz.LastIndexOf("@");
                    if (idx < 0) throw new Exception($"Invalid class: {Clazz}");
                    var module = Clazz[..idx];
                    var type = Clazz[(idx + 1)..];
                    var jsmodule = XPuer.VM.ExecuteModule(module) ?? throw new Exception($"Require module of class: {Clazz} error.");
                    var jstype = jsmodule.Get<JSObject>(type);
                    JProxy = XPuer.NewObject(jstype, new object[] { this }) ?? throw new Exception($"Try to create instance of class: {Clazz} error.");
                    ele = new JSType(jstype, JProxy);
                    staticTypes.Add(Clazz, ele);
                }
                JType = ele;
                JProxy ??= XPuer.NewObject(JType.Type, new object[] { this });
            }
            catch (Exception e) { throw new Exception($"Ctor class: {Clazz} error.", e); }
        }

        /// <summary>
        /// 构造代理对象（重载）。
        /// 使用指定的 JavaScript 类型创建实例。
        /// </summary>
        /// <param name="type">JavaScript 类型对象</param>
        internal void CtorProxy(JSObject type)
        {
            if (dynamicTypes.TryGetValue(type, out var ele) == false)
            {
                JProxy = XPuer.NewObject(type, new object[] { this }) ?? throw new Exception($"try to create instance of class: {type} error.");
                ele = new JSType(type, JProxy);
                dynamicTypes.Add(type, ele);
            }
            JType = ele;
            if (JProxy == null) JProxy = XPuer.NewObject(JType.Type, new object[] { this });
        }

        /// <summary>
        /// 初始化代理对象。
        /// 设置组件的字段值并同步到 JavaScript 实例。
        /// </summary>
        /// <param name="proxy">代理对象的引用</param>
        /// <param name="init">初始化状态</param>
        internal void InitProxy(ref object proxy, int init)
        {
            var fname = string.Empty;
            try
            {
                if (Fields != null && Fields.Count > 0)
                {
                    for (int i = 0; i < Fields.Count; i++)
                    {
                        var field = Fields[i];
                        fname = field.Key;
                        if (field.BTArray)
                        {
                            if (field.BLBValue)
                            {
                                for (int j = 0; j < field.LBValue.Count; j++)
                                {
                                    InitField(init, field.Key, field.Type, field.LBValue[j].Data, null, out object fvalue);
                                    XPuer.InitField(JProxy, field.Key, fvalue, 1 << 2);
                                }
                            }
                            else
                            {
                                for (int j = 0; j < field.LOValue.Count; j++)
                                {
                                    var ovalue = field.LOValue[j];
                                    if (ovalue)
                                    {
                                        InitField(init, field.Key, field.Type, null, ovalue, out object fvalue);
                                        XPuer.InitField(JProxy, field.Key, fvalue, 1 << 2);
                                    }
                                }
                            }
                        }
                        else
                        {
                            InitField(init, field.Key, field.Type, field.BValue, field.OValue, out object fvalue);
                            XPuer.InitField(JProxy, field.Key, fvalue, 0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                XLog.Panic(e, $"PuerBehaviour.InitProxy: init component({name}) of clazz: {Clazz}, field: {fname} error.");
                enabled = false;
                return;
            }
            proxy = JProxy;
        }

        /// <summary>
        /// 初始化字段。
        /// 根据字段类型解析并设置字段值。
        /// </summary>
        /// <param name="init">初始化状态</param>
        /// <param name="key">字段名称</param>
        /// <param name="stype">字段类型</param>
        /// <param name="bvalue">字节数组值</param>
        /// <param name="ovalue">Unity 对象值</param>
        /// <param name="fvalue">输出字段值</param>
        internal void InitField(int init, string key, string stype, byte[] bvalue, UnityEngine.Object ovalue, out object fvalue)
        {
            fvalue = null;
            if (stype == "number")
            {
                fvalue = BitConverter.ToDouble(bvalue, 0);
            }
            else if (stype == "boolean")
            {
                fvalue = BitConverter.ToBoolean(bvalue, 0);
            }
            else if (stype == "Vector2" || stype == "UnityEngine.Vector2")
            {
                fvalue = XObject.FromByte<Vector2>(bvalue);
            }
            else if (stype == "Vector3" || stype == "UnityEngine.Vector3")
            {
                fvalue = XObject.FromByte<Vector3>(bvalue);
            }
            else if (stype == "Vector4" || stype == "UnityEngine.Vector4")
            {
                fvalue = XObject.FromByte<Vector4>(bvalue);
            }
            else if (stype == "Color" || stype == "UnityEngine.Color")
            {
                fvalue = XObject.FromByte<Color>(bvalue);
            }
            else if (stype == "string")
            {
                fvalue = Encoding.UTF8.GetString(bvalue);
            }
            else
            {
                if (ovalue)
                {
                    if (ovalue is PuerBehaviour)
                    {
                        var c = ovalue as PuerBehaviour;
                        c.InitStatic(ref fvalue, init);
                    }
                    else
                    {
                        fvalue = ovalue;
                    }
                }
            }
            if (fvalue == null) XLog.Warn("PuerBehaviour.InitField: parse {0}.{1}({2}) of component {3} error", Clazz, key, stype, name);
        }
        #endregion

        #region 生命周期实现
        /// <summary>
        /// Unity 生命周期方法：Awake。
        /// 在组件首次创建时调用，负责初始化组件。
        /// </summary>
        protected virtual void Awake()
        {
            if (DType != null) // From AddComponent
            {
                if (Inited != 2) InitDynamic();
            }
            else // From Prefab
            {
                object p = null;
                if (Inited != 2) InitStatic(ref p, 2);
            }
            if (InitOK && JType.BAwake) XPuer.FuncApply(JProxy, "Awake", null);
        }

        /// <summary>
        /// Unity 生命周期方法：OnDestroy。
        /// 在组件被销毁时调用。
        /// </summary>
        protected virtual void OnDestroy() { if (InitOK && JType.BOnDestroy) XPuer.FuncApply(JProxy, "OnDestroy", null); }

        /// <summary>
        /// Unity 生命周期方法：Start。
        /// 在组件启用后的第一帧调用。
        /// </summary>
        protected virtual void Start() { if (InitOK && JType.BStart) XPuer.FuncApply(JProxy, "Start", null); }

        /// <summary>
        /// Unity 生命周期方法：OnEnable。
        /// 在组件被启用时调用。
        /// </summary>
        protected virtual void OnEnable()
        {
            if (InitOK)
            {
                if (!physicsWrapped)
                {
                    physicsWrapped = true;
                    if (JType.BOnTriggerEnter)
                    {
                        if (onTriggerEnter == null) onTriggerEnter = TriggerEnterWrap.Get(gameObject, DoTriggerEnter);
                    }
                    if (JType.BOnTriggerStay)
                    {
                        if (onTriggerStay == null) onTriggerStay = TriggerStayWrap.Get(gameObject, DoTriggerStay);
                    }
                    if (JType.BOnTriggerExit)
                    {
                        if (onTriggerExit == null) onTriggerExit = TriggerExitWrap.Get(gameObject, DoTriggerExit);
                    }
                    if (JType.BOnCollisionEnter)
                    {
                        if (onCollisionEnter == null) onCollisionEnter = CollisionEnterWrap.Get(gameObject, DoCollisionEnter);
                    }
                    if (JType.BOnCollisionStay)
                    {
                        if (onCollisionStay == null) onCollisionStay = CollisionStayWrap.Get(gameObject, DoCollisionStay);
                    }
                    if (JType.BOnCollisionExit)
                    {
                        if (onCollisionExit == null) onCollisionExit = CollisionExitWrap.Get(gameObject, DoCollisionExit);
                    }
                }
                if (onTriggerEnter) onTriggerEnter.enabled = true;
                if (onTriggerStay) onTriggerStay.enabled = true;
                if (onTriggerExit) onTriggerExit.enabled = true;
                if (onCollisionEnter) onCollisionEnter.enabled = true;
                if (onCollisionStay) onCollisionStay.enabled = true;
                if (onCollisionExit) onCollisionExit.enabled = true;
                enableList.Add(this);
                if (JType.BOnEnable) XPuer.FuncApply(JProxy, "OnEnable", null);
            }
            else { enabled = false; }
        }

        /// <summary>
        /// Unity 生命周期方法：OnDisable。
        /// 在组件被禁用时调用。
        /// </summary>
        protected virtual void OnDisable()
        {
            if (InitOK)
            {
                if (onTriggerEnter) onTriggerEnter.enabled = false;
                if (onTriggerStay) onTriggerStay.enabled = false;
                if (onTriggerExit) onTriggerExit.enabled = false;
                if (onCollisionEnter) onCollisionEnter.enabled = false;
                if (onCollisionStay) onCollisionStay.enabled = false;
                if (onCollisionExit) onCollisionExit.enabled = false;
                enableList.Remove(this);
                OnUpdateEvent.Remove(DUpdate);
                onLateUpdateEvent.Remove(DLateUpdate);
                onFixedUpdateEvent.Remove(DFixedUpdate);
                if (InitOK && JType.BOnDisable) XPuer.FuncApply(JProxy, "OnDisable", null);
            }
        }

        /// <summary>
        /// 静态 Update 方法。
        /// 处理所有启用组件的 Update 事件。
        /// </summary>
        public static void OnUpdate()
        {
            try
            {
                for (int i = 0; i < OnUpdateEvent.Count; i++) OnUpdateEvent[i].Invoke();
            }
            catch (Exception e) { XLog.Panic(e); }
            if (enableList.Count > 0)
            {
                for (int i = 0; i < enableList.Count; i++)
                {
                    var comp = enableList[i];
                    if (comp == null && comp.InitOK == false) continue;
                    OnUpdateEvent.Remove(comp.DUpdate);
                    onLateUpdateEvent.Remove(comp.DLateUpdate);
                    onFixedUpdateEvent.Remove(comp.DFixedUpdate);
                    if (comp.JType.BUpdate) OnUpdateEvent.Add(comp.DUpdate);
                    if (comp.JType.BLateUpdate) onLateUpdateEvent.Add(comp.DLateUpdate);
                    if (comp.JType.BFixedUpdate) onFixedUpdateEvent.Add(comp.DFixedUpdate);
                }
                enableList.Clear();
            }
        }

        /// <summary>
        /// 实例 Update 方法。
        /// 处理单个组件的 Update 事件。
        /// </summary>
        internal void DUpdate() { if (InitOK && JType.BUpdate) XPuer.FuncApply(JProxy, "Update", null); }

        /// <summary>
        /// 静态 LateUpdate 方法。
        /// 处理所有启用组件的 LateUpdate 事件。
        /// </summary>
        public static void OnLateUpdate() { for (int i = 0; i < onLateUpdateEvent.Count; i++) onLateUpdateEvent[i].Invoke(); }

        /// <summary>
        /// 实例 LateUpdate 方法。
        /// 处理单个组件的 LateUpdate 事件。
        /// </summary>
        internal void DLateUpdate() { if (InitOK && JType.BLateUpdate) XPuer.FuncApply(JProxy, "LateUpdate", null); }

        /// <summary>
        /// 静态 FixedUpdate 方法。
        /// 处理所有启用组件的 FixedUpdate 事件。
        /// </summary>
        public static void OnFixedUpdate() { for (int i = 0; i < onFixedUpdateEvent.Count; i++) onFixedUpdateEvent[i].Invoke(); }

        /// <summary>
        /// 实例 FixedUpdate 方法。
        /// 处理单个组件的 FixedUpdate 事件。
        /// </summary>
        internal void DFixedUpdate() { if (InitOK && JType.BFixedUpdate) XPuer.FuncApply(JProxy, "FixedUpdate", null); }

        /// <summary>
        /// 处理触发器进入事件。
        /// </summary>
        /// <param name="other">进入触发器的碰撞体</param>
        internal void DoTriggerEnter(Collider other) { if (InitOK && JType.BOnTriggerEnter) XPuer.FuncApply(JProxy, "OnTriggerEnter", new object[] { other }); }

        /// <summary>
        /// 处理触发器停留事件。
        /// </summary>
        /// <param name="other">停留在触发器内的碰撞体</param>
        internal void DoTriggerStay(Collider other) { if (InitOK && JType.BOnTriggerStay) XPuer.FuncApply(JProxy, "OnTriggerStay", new object[] { other }); }

        /// <summary>
        /// 处理触发器退出事件。
        /// </summary>
        /// <param name="other">退出触发器的碰撞体</param>
        internal void DoTriggerExit(Collider other) { if (InitOK && JType.BOnTriggerExit) XPuer.FuncApply(JProxy, "OnTriggerExit", new object[] { other }); }

        /// <summary>
        /// 处理碰撞开始事件。
        /// </summary>
        /// <param name="collision">碰撞信息</param>
        internal void DoCollisionEnter(Collision collision) { if (InitOK && JType.BOnCollisionEnter) XPuer.FuncApply(JProxy, "OnCollisionEnter", new object[] { collision }); }

        /// <summary>
        /// 处理碰撞持续事件。
        /// </summary>
        /// <param name="collision">碰撞信息</param>
        internal void DoCollisionStay(Collision collision) { if (InitOK && JType.BOnCollisionStay) XPuer.FuncApply(JProxy, "OnCollisionStay", new object[] { collision }); }

        /// <summary>
        /// 处理碰撞结束事件。
        /// </summary>
        /// <param name="collision">碰撞信息</param>
        internal void DoCollisionExit(Collision collision) { if (InitOK && JType.BOnCollisionExit) XPuer.FuncApply(JProxy, "OnCollisionExit", new object[] { collision }); }
        #endregion

        #region 公共接口实现
        /// <summary>
        /// 添加组件到指定游戏对象。
        /// </summary>
        /// <param name="parentObj">父节点对象</param>
        /// <param name="path">节点路径</param>
        /// <param name="type">组件类型</param>
        /// <returns>创建的 JavaScript 对象实例</returns>
        public static JSObject Add(UnityEngine.Object parentObj, string path, JSObject type)
        {
            if (type == null)
            {
                XLog.Error("PuerBehaviour.Add: missing type argument");
                return null;
            }
            var root = parentObj.GetTransform(path);
            if (root == null || root.gameObject == null)
            {
                XLog.Error("PuerBehaviour.Add: error caused by nil root.");
                return null;
            }
            DType = type;
            var comp = root.gameObject.AddComponent<PuerBehaviour>();
            return comp.JProxy;
        }

        /// <summary>
        /// 获取当前节点中的组件。
        /// </summary>
        /// <param name="obj">节点对象</param>
        /// <param name="type">组件类型</param>
        /// <returns>找到的组件实例</returns>
        public static PuerBehaviour Get(UnityEngine.Object obj, JSObject type)
        {
            var rets = InternalGets(obj, type, 0, false);
            return rets.Length > 0 ? rets[0] : null;
        }

        /// <summary>
        /// 获取父节点中的组件（包括自身）。
        /// </summary>
        /// <param name="obj">节点对象</param>
        /// <param name="type">组件类型</param>
        /// <param name="includeInactive">是否包含未激活的组件</param>
        /// <returns>找到的组件实例</returns>
        public static PuerBehaviour GetInParent(UnityEngine.Object obj, JSObject type, bool includeInactive)
        {
            var rets = InternalGets(obj, type, -1, includeInactive);
            return rets.Length > 0 ? rets[0] : null;
        }

        /// <summary>
        /// 获取子节点中的组件（包括自身）。
        /// </summary>
        /// <param name="obj">节点对象</param>
        /// <param name="type">组件类型</param>
        /// <param name="includeInactive">是否包含未激活的组件</param>
        /// <returns>找到的组件实例</returns>
        public static PuerBehaviour GetInChildren(UnityEngine.Object obj, JSObject type, bool includeInactive)
        {
            var rets = InternalGets(obj, type, 1, includeInactive);
            return rets.Length > 0 ? rets[0] : null;
        }

        /// <summary>
        /// 获取当前节点中的组件集。
        /// </summary>
        /// <param name="obj">节点对象</param>
        /// <param name="type">组件类型</param>
        /// <returns>找到的组件实例数组</returns>
        public static PuerBehaviour[] Gets(UnityEngine.Object obj, JSObject type) { return InternalGets(obj, type, 0, false); }

        /// <summary>
        /// 获取父节点中的组件集（包括自身）。
        /// </summary>
        /// <param name="obj">节点对象</param>
        /// <param name="type">组件类型</param>
        /// <param name="includeInactive">是否包含未激活的组件</param>
        /// <returns>找到的组件实例数组</returns>
        public static PuerBehaviour[] GetsInParent(UnityEngine.Object obj, JSObject type, bool includeInactive) { return InternalGets(obj, type, -1, includeInactive); }

        /// <summary>
        /// 获取子节点中的组件集。
        /// </summary>
        /// <param name="obj">节点对象</param>
        /// <param name="type">组件类型</param>
        /// <param name="includeInactive">是否包含未激活的组件</param>
        /// <returns>找到的组件实例数组</returns>
        public static PuerBehaviour[] GetsInChildren(UnityEngine.Object obj, JSObject type, bool includeInactive) { return InternalGets(obj, type, 1, includeInactive); }

        /// <summary>
        /// 获取指定类型的 PuerBehaviour 组件。
        /// </summary>
        /// <param name="obj">要搜索的对象</param>
        /// <param name="type">要匹配的 JavaScript 类型</param>
        /// <param name="depth">搜索深度，-1 表示父级，0 表示当前，正数表示子级</param>
        /// <param name="includeInactive">是否包括非激活的对象</param>
        /// <returns>匹配的 PuerBehaviour 组件数组</returns>
        internal static PuerBehaviour[] InternalGets(UnityEngine.Object obj, JSObject type, int depth, bool includeInactive)
        {
            if (type == null)
            {
                XLog.Error("PuerBehaviour.InternalGets: error caused by nil type.");
                return null;
            }
            var root = obj.GetTransform();
            if (root == null || root.gameObject == null)
            {
                XLog.Error("PuerBehaviour.InternalGets: error caused by nil root.");
                return null;
            }

            var rets = new List<PuerBehaviour>();
            PuerBehaviour[] coms;
            if (depth == -1) coms = root.GetComponentsInParent<PuerBehaviour>(includeInactive);
            else if (depth == 0) coms = root.GetComponents<PuerBehaviour>();
            else coms = root.GetComponentsInChildren<PuerBehaviour>(includeInactive);
            for (int i = 0; i < coms.Length; i++)
            {
                var com = coms[i];
                if (com)
                {
                    if (com.JType == null) com.CtorProxy();
                    if (com.JType != null)
                    {
                        PuerBehaviour ret = null;
                        if (com.JType.Type == type) ret = com;
                        else
                        {
                            // todo: search in parents
                        }
                        if (ret)
                        {
                            object p = null;
                            if (com.Inited == 0) com.InitStatic(ref p, 1);
                            if (com.InitOK) rets.Add(com);
                            else { }
                        }
                    }
                }
            }
            return rets.ToArray();
        }
        #endregion
    }
}