// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using EP.U3D.PUER;
using EP.U3D.UTIL;
using NUnit.Framework;
using Puerts;
using UnityEngine;
using UnityEngine.TestTools;

public class TestXPuerComp
{
    private JSObject myComponent;

    [OneTimeSetUp]
    public void Init()
    {
        var loader = new Puerts.TSLoader.TSLoader();
        loader.UseRuntimeLoader(new DefaultLoader());
        loader.UseRuntimeLoader(new NodeModuleLoader(XEnv.ProjectPath));
        XPuer.VM = new JsEnv(loader, -1);
        XPuer.VM.UsingAction<JSObject, string, object[]>();
        XPuer.VM.UsingAction<JSObject, string, object, int>();

        var puerModule = XPuer.VM.ExecuteModule("EP.U3D.PUER");
        XPuer.NewObject = puerModule.Get<Func<JSObject, object[], JSObject>>("NewObject");
        XPuer.FuncApply = puerModule.Get<Func<JSObject, string, object[], object>>("FuncApply");
        XPuer.InitField = puerModule.Get<Action<JSObject, string, object, int>>("InitField");
        myComponent = XPuer.VM.ExecuteModule("TestComponent").Get<JSObject>("MyComponent");
    }

    [Test]
    public void Property()
    {
        // 准备测试对象
        var obj = GameObject.Instantiate(Resources.Load<GameObject>("Bundle/Prefab/MyComponent"));
        var puerComp = obj.GetComponent<PuerBehaviour>();
        var childPuerComp = obj.transform.Find("Child2").GetComponent<PuerBehaviour>();

        // 测试值类型类型
        Assert.AreEqual(1, BitConverter.ToDouble(puerComp.Fields.Find(f => f.Key == "TestNumber").BValue, 0), "应当包含number类型");
        Assert.AreEqual("test string", Encoding.UTF8.GetString(puerComp.Fields.Find(f => f.Key == "TestString").BValue).Replace("\0", ""), "应当包含string类型");
        Assert.AreEqual(true, BitConverter.ToBoolean(puerComp.Fields.Find(f => f.Key == "TestBoolean").BValue, 0), "应当包含boolean类型");
        Assert.AreEqual(new Vector3(3, 3, 3), XObject.FromByte<Vector3>(puerComp.Fields.Find(f => f.Key == "TestVector3").BValue), "应当包含Vector3类型");
        Assert.AreEqual(new Vector4(4.4f, 4.4f, 4.4f, 4.4f), XObject.FromByte<Vector4>(puerComp.Fields.Find(f => f.Key == "TestVector4").BValue), "应当包含Vector4类型");
        Assert.AreEqual(new Color(1, 0, 0, 0), XObject.FromByte<Color>(puerComp.Fields.Find(f => f.Key == "TestColor").BValue), "应当包含Color类型");
        // 测试引用类型
        Assert.AreEqual(childPuerComp, puerComp.Fields.Find(f => f.Key == "TestObject").OValue, "应当包含PuerBehaviour对象类型");

        // 测试值数组类型
        var numArray = puerComp.Fields.Find(f => f.Key == "TestNumberArray");
        var numValues = new List<double>();
        foreach (var item in numArray.LBValue)
        {
            numValues.Add(BitConverter.ToDouble(item.Data, 0));
        }
        Assert.AreEqual(2, numValues.Count, "数值数组长度应为2");
        Assert.AreEqual(2, numValues[0], "第一个元素应为2");
        Assert.AreEqual(3, numValues[1], "第二个元素应为3");

        // 测试引用数组类型
        var objArray = puerComp.Fields.Find(f => f.Key == "TestObjectArray");
        Assert.AreEqual(1, objArray.LOValue.Count, "对象数组长度应为1");
        Assert.AreEqual(childPuerComp, objArray.LOValue[0], "第一个元素应为childPuerComp");

        Assert.IsNotNull(puerComp.JProxy, "PuerBehaviour的JProxy不应为空");
        Assert.IsNotNull(puerComp.JType, "PuerBehaviour的JType不应为空");

        // 清理对象
        if (obj) GameObject.Destroy(obj);
    }

    [UnityTest]
    public IEnumerator Lifecycle()
    {
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent Awake: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnEnable: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent Start: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent Update: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent LateUpdate: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent FixedUpdate: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnDisable: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnDestroy: .*"));

        var obj = new GameObject();
        PuerBehaviour.Add(obj, "", myComponent);
        yield return null;
        PuerBehaviour.OnUpdate();
        PuerBehaviour.OnUpdate();
        PuerBehaviour.OnLateUpdate();
        PuerBehaviour.OnFixedUpdate();
        obj.SetActive(false);
        GameObject.DestroyImmediate(obj);
    }

    [UnityTest]
    public IEnumerator Physics()
    {
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnCollisionEnter: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnCollisionStay: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnCollisionExit: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnTriggerEnter: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnTriggerStay: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnTriggerExit: .*"));

        // 准备两个正方体
        var cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var rig = cube1.AddComponent<Rigidbody>();
        rig.isKinematic = false;
        rig.useGravity = false;
        PuerBehaviour.Add(cube1, "", myComponent);
        cube1.transform.position = new Vector3(0, 2, 0);
        var cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // 测试碰撞事件
        cube1.transform.position = new Vector3(0, 1, 0);
        yield return new WaitForSeconds(0.5f);

        cube1.transform.position = new Vector3(0, 2, 0);
        yield return new WaitForSeconds(0.5f);

        // 测试触发事件
        cube1.GetComponent<Collider>().isTrigger = true;
        cube1.transform.position = new Vector3(0, 1, 0);
        yield return new WaitForSeconds(0.5f);

        cube1.transform.position = new Vector3(0, 2, 0);
        yield return new WaitForSeconds(0.5f);

        // 清理对象
        GameObject.DestroyImmediate(cube1);
        GameObject.DestroyImmediate(cube2);
    }

    [TestCase(0, 1, 1, 1, 1, "当前节点")]
    [TestCase(-1, 1, 2, 3, 2, "父节点")]
    [TestCase(1, 4, 2, 1, 1, "子节点")]
    public void Get(int depth, int rootResult, int child1Result, int grandChildResult, int child2Result, string _)
    {
        // 创建测试层级结构
        var root = new GameObject("Root");
        var child1 = new GameObject("Child1");
        var child2 = new GameObject("Child2");
        var grandChild = new GameObject("GrandChild");

        try
        {
            // 建立层级关系
            child1.transform.SetParent(root.transform);
            child2.transform.SetParent(root.transform);
            grandChild.transform.SetParent(child1.transform);

            // 添加测试组件
            PuerBehaviour.Add(root, "", myComponent);
            PuerBehaviour.Add(child1, "", myComponent);
            PuerBehaviour.Add(child2, "", myComponent);
            PuerBehaviour.Add(grandChild, "", myComponent);

            // 在不同节点上测试指定深度
            PuerBehaviour[] result;

            // 测试Root
            result = PuerBehaviour.InternalGets(root, myComponent, depth, true);
            Assert.AreEqual(rootResult, result.Length);

            // 测试Child1
            result = PuerBehaviour.InternalGets(child1, myComponent, depth, true);
            Assert.AreEqual(child1Result, result.Length);

            // 测试GrandChild
            result = PuerBehaviour.InternalGets(grandChild, myComponent, depth, true);
            Assert.AreEqual(grandChildResult, result.Length);

            // 测试Child2
            result = PuerBehaviour.InternalGets(child2, myComponent, depth, true);
            Assert.AreEqual(child2Result, result.Length);

            // 测试includeInactive参数
            child1.SetActive(false);

            // 使用includeInactive=false
            result = PuerBehaviour.InternalGets(root, myComponent, depth, false);
            if (depth == 0) Assert.AreEqual(1, result.Length, "includeInactive=false时，当前节点查询结果不受影响");
            else if (depth == -1) Assert.AreEqual(1, result.Length, "includeInactive=false时，父节点查询不应包含非激活节点");
            else if (depth == 1) Assert.IsTrue(result.Length < 4, "includeInactive=false时，子节点查询不应包含非激活节点");

            // 测试空参数
            LogAssert.Expect(LogType.Error, new Regex(@".*error caused by nil root."));
            result = PuerBehaviour.InternalGets(null, myComponent, depth, true);
            Assert.IsNull(result, "传入空对象时应返回null");

            LogAssert.Expect(LogType.Error, new Regex(@".*error caused by nil type."));
            result = PuerBehaviour.InternalGets(root, null, depth, true);
            Assert.IsNull(result, "传入空类型时应返回null");
        }
        finally
        {
            // 清理对象
            GameObject.DestroyImmediate(root);
        }
    }
}
#endif
