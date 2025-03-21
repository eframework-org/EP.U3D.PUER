// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Text.RegularExpressions;
using EP.U3D.PUER;
using EP.U3D.UTIL;
using NUnit.Framework;
using Puerts;
using UnityEngine;
using UnityEngine.TestTools;
using static ET.U3D.PUER.XPuer;
using System.Collections.Generic;
using System.Text;

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

    [TestCase(true)]
    [TestCase(false)]
    public void Property(bool isDynamic)
    {
        PuerBehaviour puerComp = null;
        GameObject obj = null;
        if (isDynamic)
        {
            obj = new GameObject();
            PuerBehaviour.Add(obj, "", myComponent);
            puerComp = obj.GetComponent<PuerBehaviour>();
        }
        else
        {
            obj = GameObject.Instantiate(Resources.Load<GameObject>("Bundle/Prefab/MyComponent"));
            puerComp = obj.GetComponent<PuerBehaviour>();
            Assert.AreEqual(1, puerComp.Fields.Find(f => f.Key == "TestProp").BValue[0], "应当包含TestProp属性");

            // Assert.IsNotNull(puerComp.Fields.Find(f => f.Key == "TestProp"), "应当包含TestProp属性");
            // Assert.IsNotNull(puerComp.Fields.Find(f => f.Type == "number"), "应当包含number类型");
            // Assert.IsNotNull(puerComp.Fields.Find(f => f.Type == "string"), "应当包含string类型");
            // Assert.IsNotNull(puerComp.Fields.Find(f => f.Type == "boolean"), "应当包含boolean类型");
        }

        Assert.IsNotNull(puerComp.JProxy, "PuerBehaviour的JProxy不应为空");
        Assert.IsNotNull(puerComp.JType, "PuerBehaviour的JType不应为空");
        Assert.IsNotNull(puerComp.JType.Type, "PuerBehaviour的JType.Type不应为空");

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

    [Test]
    public void ValueFields()
    {
        // 创建测试对象
        var obj = new GameObject("TestObject");
        PuerBehaviour.Add(obj, "", myComponent);
        var puerComp = obj.GetComponent<PuerBehaviour>();

        try
        {
            // 测试BValue - 数值类型
            var numField = new PuerBehaviour.Field { Key = "TestNumber", Type = "number", BValue = BitConverter.GetBytes(42.5) };
            puerComp.Fields.Add(numField);
            Assert.AreEqual(42.5, BitConverter.ToDouble(numField.BValue, 0), "BValue应正确存储数值类型");

            // 测试BValue - 布尔类型
            var boolField = new PuerBehaviour.Field { Key = "TestBool", Type = "boolean", BValue = BitConverter.GetBytes(true) };
            puerComp.Fields.Add(boolField);
            Assert.IsTrue(BitConverter.ToBoolean(boolField.BValue, 0), "BValue应正确存储布尔类型");

            // 测试BValue - Vector3类型
            var vectorValue = new Vector3(1, 2, 3);
            var vectorField = new PuerBehaviour.Field { Key = "TestVector", Type = "Vector3", BValue = XObject.ToByte(vectorValue) };
            puerComp.Fields.Add(vectorField);
            Assert.AreEqual(vectorValue, XObject.FromByte<Vector3>(vectorField.BValue), "BValue应正确存储Vector3类型");

            // 测试BValue - 字符串类型
            var stringValue = "测试字符串";
            var stringField = new PuerBehaviour.Field { Key = "TestString", Type = "string", BValue = Encoding.UTF8.GetBytes(stringValue) };
            puerComp.Fields.Add(stringField);
            Assert.AreEqual(stringValue, Encoding.UTF8.GetString(stringField.BValue), "BValue应正确存储字符串类型");

            // 测试OValue - Unity对象类型
            var objValue = new GameObject("ChildObject");
            var objField = new PuerBehaviour.Field { Key = "TestObj", Type = "GameObject", OValue = objValue };
            puerComp.Fields.Add(objField);
            Assert.AreEqual(objValue, objField.OValue, "OValue应正确存储Unity对象");

            // 测试LBValue - 数值数组类型
            var numArrayField = new PuerBehaviour.Field
            {
                Key = "TestNumberArray",
                Type = "number",
                BTArray = true,
                BLBValue = true,
                LBValue = new List<PuerBehaviour.Byte>()
            };
            numArrayField.LBValue.Add(new PuerBehaviour.Byte(BitConverter.GetBytes(1.5)));
            numArrayField.LBValue.Add(new PuerBehaviour.Byte(BitConverter.GetBytes(2.5)));
            numArrayField.LBValue.Add(new PuerBehaviour.Byte(BitConverter.GetBytes(3.5)));
            puerComp.Fields.Add(numArrayField);

            Assert.AreEqual(3, numArrayField.LBValue.Count, "LBValue列表长度应为3");
            Assert.AreEqual(1.5, BitConverter.ToDouble(numArrayField.LBValue[0].Data, 0), "LBValue应正确存储数值数组第一个元素");
            Assert.AreEqual(2.5, BitConverter.ToDouble(numArrayField.LBValue[1].Data, 0), "LBValue应正确存储数值数组第二个元素");
            Assert.AreEqual(3.5, BitConverter.ToDouble(numArrayField.LBValue[2].Data, 0), "LBValue应正确存储数值数组第三个元素");

            // 测试LOValue - 对象数组类型
            var obj1 = new GameObject("Obj1");
            var obj2 = new GameObject("Obj2");
            var objArrayField = new PuerBehaviour.Field
            {
                Key = "TestObjArray",
                Type = "GameObject",
                BTArray = true,
                BLBValue = false,
                LOValue = new List<UnityEngine.Object>()
            };
            objArrayField.LOValue.Add(obj1);
            objArrayField.LOValue.Add(obj2);
            puerComp.Fields.Add(objArrayField);

            Assert.AreEqual(2, objArrayField.LOValue.Count, "LOValue列表长度应为2");
            Assert.AreEqual(obj1, objArrayField.LOValue[0], "LOValue应正确存储对象数组第一个元素");
            Assert.AreEqual(obj2, objArrayField.LOValue[1], "LOValue应正确存储对象数组第二个元素");

            // 测试字段重置
            var fieldToReset = numArrayField;
            fieldToReset.Reset();
            Assert.AreEqual("", fieldToReset.Type, "重置后Type应为空字符串");
            Assert.IsNull(fieldToReset.OValue, "重置后OValue应为null");
            Assert.IsNull(fieldToReset.LOValue, "重置后LOValue应为null");
            Assert.IsNull(fieldToReset.LBValue, "重置后LBValue应为null");
            Assert.IsFalse(fieldToReset.BTArray, "重置后BTArray应为false");
            Assert.IsFalse(fieldToReset.BLBValue, "重置后BLBValue应为false");
            Assert.AreEqual(16, fieldToReset.BValue.Length, "重置后BValue长度应为16");
        }
        finally
        {
            // 清理对象
            GameObject.DestroyImmediate(obj);
        }
    }

    [Test]
    public void InitFieldTest()
    {
        // 创建测试对象
        var obj = new GameObject("TestObject");
        PuerBehaviour.Add(obj, "", myComponent);
        var puerComp = obj.GetComponent<PuerBehaviour>();

        try
        {
            // 测试数值类型初始化
            object numberValue;
            puerComp.InitField(0, "testNumber", "number", BitConverter.GetBytes(123.456), null, out numberValue);
            Assert.AreEqual(123.456, numberValue, "InitField应正确解析number类型");

            // 测试布尔类型初始化
            object boolValue;
            puerComp.InitField(0, "testBool", "boolean", BitConverter.GetBytes(true), null, out boolValue);
            Assert.IsTrue((bool)boolValue, "InitField应正确解析boolean类型");

            // 测试Vector2类型初始化
            var vector2 = new Vector2(1.1f, 2.2f);
            object vector2Value;
            puerComp.InitField(0, "testVector2", "Vector2", XObject.ToByte(vector2), null, out vector2Value);
            Assert.AreEqual(vector2, vector2Value, "InitField应正确解析Vector2类型");

            // 测试Vector3类型初始化
            var vector3 = new Vector3(1.1f, 2.2f, 3.3f);
            object vector3Value;
            puerComp.InitField(0, "testVector3", "Vector3", XObject.ToByte(vector3), null, out vector3Value);
            Assert.AreEqual(vector3, vector3Value, "InitField应正确解析Vector3类型");

            // 测试Vector4类型初始化
            var vector4 = new Vector4(1.1f, 2.2f, 3.3f, 4.4f);
            object vector4Value;
            puerComp.InitField(0, "testVector4", "Vector4", XObject.ToByte(vector4), null, out vector4Value);
            Assert.AreEqual(vector4, vector4Value, "InitField应正确解析Vector4类型");

            // 测试Color类型初始化
            var color = new Color(0.1f, 0.2f, 0.3f, 0.4f);
            object colorValue;
            puerComp.InitField(0, "testColor", "Color", XObject.ToByte(color), null, out colorValue);
            Assert.AreEqual(color, colorValue, "InitField应正确解析Color类型");

            // 测试字符串类型初始化
            var str = "测试字符串初始化";
            object strValue;
            puerComp.InitField(0, "testString", "string", Encoding.UTF8.GetBytes(str), null, out strValue);
            Assert.AreEqual(str, strValue, "InitField应正确解析string类型");

            // 测试Unity对象类型初始化
            var objValue = new GameObject("TestUnityObj");
            object unityObjValue;
            puerComp.InitField(0, "testObj", "GameObject", null, objValue, out unityObjValue);
            Assert.AreEqual(objValue, unityObjValue, "InitField应正确解析Unity对象类型");

            // 测试PuerBehaviour对象类型初始化
            var childObj = new GameObject("TestPuerObj");
            PuerBehaviour.Add(childObj, "", myComponent);
            var childComp = childObj.GetComponent<PuerBehaviour>();
            object puerObjValue;
            puerComp.InitField(0, "testPuerObj", "MyComponent", null, childComp, out puerObjValue);
            Assert.IsNotNull(puerObjValue, "InitField应正确解析PuerBehaviour对象类型");

            // 测试无效类型初始化
            object invalidValue;
            puerComp.InitField(0, "testInvalid", "InvalidType", new byte[16], null, out invalidValue);
            Assert.IsNull(invalidValue, "对于无效的类型，InitField应返回null");
        }
        finally
        {
            // 清理对象
            GameObject.DestroyImmediate(obj);
        }
    }
}
#endif
