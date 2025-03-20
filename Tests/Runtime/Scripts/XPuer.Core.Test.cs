// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS

using System.Collections;
using EP.U3D.PUER;
using UnityEngine.TestTools;
using UnityEngine;
using Puerts;
using EP.U3D.UTIL;
using NUnit.Framework;
using static EP.U3D.PUER.XPuer;
using System.IO;

public class TestXPuerCore
{
    public class MyHandler : MonoBehaviour, IHandler
    {
        public bool IsPreInit = false;
        public bool IsVMStart = false;
        public bool IsPostInit = false;

        ILoader IHandler.Loader
        {
            get
            {
                var packagePath = ET.U3D.UTIL.XEditor.Utility.FindPackage().assetPath;
                var tsDir = Path.Combine(XEnv.ProjectPath, packagePath, "Tests/Runtime/TypeScripts");
                var loader = new Puerts.TSLoader.TSLoader();
                loader.UseRuntimeLoader(new DefaultLoader());
                loader.UseRuntimeLoader(new NodeModuleLoader(tsDir));
                return loader;
            }
        }

        IEnumerator IHandler.OnPreInit()
        {
            IsPreInit = true;
            yield return null;
        }

        IEnumerator IHandler.OnVMStart()
        {
            VM.UsingAction<JSObject, string, object[]>();
            VM.UsingAction<JSObject, string, object, int>();
            VM.UsingAction<bool>();
            VM.UsingAction<float>();
            VM.UsingAction<string, bool>();
            VM.UsingAction<string, bool>();
            IsVMStart = true;
            yield return null;
        }

        IEnumerator IHandler.OnPostInit()
        {
            IsPostInit = true;
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator Initialize()
    {
        XPrefs.Asset.Set(Prefs.DebugWait, false); // 测试时关闭等待调试器

        Const.releaseMode = false;
        bool[] debugModes = { false, true };
        foreach (var debugMode in debugModes)
        {
            var isPreInit = false;
            var isVMStart = false;
            var isPostInit = false;

            var handler = new MyHandler();
            XPuer.Event.Reg(XPuer.EventType.OnPreInit, () => isPreInit = true);
            XPuer.Event.Reg(XPuer.EventType.OnVMStart, () => isVMStart = true);
            XPuer.Event.Reg(XPuer.EventType.OnPostInit, () => isPostInit = true);

            Const.debugMode = debugMode;
            Const.debugWait = debugMode;
            var stime = Time.realtimeSinceStartup;
            yield return XPuer.Initialize(handler);

            // 测试属性是否正确初始化
            var xpuer = GameObject.Find("[XPuer]");
            Assert.IsTrue(xpuer != null, "应创建XPuer游戏对象");
            Assert.IsTrue(xpuer.GetComponent<XPuer>() != null, "XPuer对象应包含XPuer组件");
            Assert.IsTrue(VM.debugPort == (debugMode ? Const.DebugPort : -1), "调试端口应正确设置");
            Assert.IsTrue(handler.IsPreInit, "handler的PreInit事件应被调用");
            Assert.IsTrue(handler.IsVMStart, "handler的VMStart事件应被调用");
            Assert.IsTrue(handler.IsPostInit, "handler的PostInit事件应被调用");
            Assert.IsTrue(isPreInit, "OnPreInit事件应被触发");
            Assert.IsTrue(isVMStart, "OnVMStart事件应被触发");
            Assert.IsTrue(isPostInit, "OnPostInit事件应被触发");

            // 测试 NewObject
            var newObject = NewObject(VM.ExecuteModule("TestComponent").Get<JSObject>("MyComponent"), null);
            Assert.IsTrue(newObject.Get<JSObject>("TestFunc") != null, "创建的JS对象应包含testFun方法");

            // 测试 FuncApply
            var obj = new GameObject("TestObj");
            LogAssert.Expect(LogType.Log, "TestObj");
            FuncApply(newObject, "TestFunc", new object[] { obj });

            // 测试 InitField
            // 测试字符串类型
            InitField(newObject, "testField", "testValue", 1 << 2);
            Assert.AreEqual("testValue", newObject.Get<string>("testField"), "字符串类型字段应被正确初始化");
            // 测试值类型
            InitField(newObject, "numField", 123, 1 << 2);
            Assert.AreEqual(123, newObject.Get<int>("numField"), "数值类型字段应被正确初始化");
            // 测试数组类型
            string[] strArray = new string[] { "aa", "bb", "cc" };
            InitField(newObject, "arrayField", strArray, 1 << 2);
            var resultArray = newObject.Get<JSObject>("arrayField");
            Assert.AreEqual("aa", resultArray.Get<string[]>("0")[0], "数组类型字段应被正确初始化");
        }
    }
}
#endif
