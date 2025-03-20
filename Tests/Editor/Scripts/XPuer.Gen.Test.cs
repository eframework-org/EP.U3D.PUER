// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using ET.U3D.PUER;
using Puerts;
using EP.U3D.UTIL;
using System.Linq;
using TestNamespace;

/// <summary>
/// XPuer.Gen 的单元测试类，验证生成和模块管理功能的正确性。
/// </summary>
public class TestXPuerGen
{
    [Test]
    public void GenDir()
    {
        var genDir = XPuer.Gen.GenDir;

        // 验证结果
        Assert.IsTrue(Directory.Exists(genDir), "如果目录不存在，GenDir应创建该目录");
    }

    [Test]
    public void UpmExports()
    {
        var exports = XPuer.Gen.UpmExports();

        // 验证结果
        Assert.IsTrue(exports.ContainsKey("ep.u3d.puer"), "UpmExports应包含ep.u3d.puer包");
        Assert.IsFalse(exports.ContainsKey("ep.u3d.puer2"), "UpmExports不应包含不存在的包");
    }

    [Test]
    public void DynamicBindings()
    {
        // 保存原始 Bindings
        var originalBindings = XPuer.Gen.Bindings;
        try
        {
            // 添加测试类型到 Bindings
            XPuer.Gen.Bindings = new List<Type>(XPuer.Gen.Bindings) { typeof(TestClass) };
            var bindings = XPuer.Gen.DynamicBindings;

            // 验证结果
            Assert.IsTrue(bindings.Any(type => type.Namespace == "TestNamespace"), "动态绑定类型应包含TestNamespace命名空间");
            Assert.IsFalse(bindings.Any(type => type.Namespace == "TestNamespace2"), "动态绑定类型不应包含不存在的命名空间");
        }
        finally
        {
            // 恢复原始 Bindings
            XPuer.Gen.Bindings = originalBindings;
        }
    }

    [Test]
    public void DoFilter()
    {
        // 保存原始 Bindings
        var originalExcludeMembers = XPuer.Gen.ExcludeMembers;
        var originalExcludeMemberMap = XPuer.Gen.mExcludeMemberMap;
        try
        {
            XPuer.Gen.mExcludeMemberMap = null;
            XPuer.Gen.ExcludeMembers = new List<List<string>> { new List<string> { "TestNamespace.TestClass", "ExcludeMember" } };

            // 测试正常方法和过时方法
            var memberInfo1 = typeof(TestClass).GetMethod("NormalMethod");
            var memberInfo2 = typeof(TestClass).GetMethod("ObsoleteMethod");
            var memberInfo3 = typeof(TestClass).GetMember("ExcludeMember");
            var bindingMode1 = XPuer.Gen.DoFilter(memberInfo1);
            var bindingMode2 = XPuer.Gen.DoFilter(memberInfo2);
            var bindingMode3 = XPuer.Gen.DoFilter(memberInfo3[0]);

            // 验证结果
            Assert.AreEqual(BindingMode.FastBinding, bindingMode1, "DoFilter应返回正确的绑定模式");
            Assert.AreEqual(BindingMode.DontBinding, bindingMode2, "DoFilter应返回正确的绑定模式");
            Assert.AreEqual(BindingMode.DontBinding, bindingMode3, "DoFilter应返回正确的绑定模式");
        }
        finally
        {
            XPuer.Gen.ExcludeMembers = originalExcludeMembers;
            XPuer.Gen.mExcludeMemberMap = originalExcludeMemberMap;
        }
    }

    [Test]
    public void IsExcluded()
    {
        var originalExcluded = XPuer.Gen.ExcludeTypes;
        try
        {
            XPuer.Gen.ExcludeTypes = new List<string> { "System.Tuple" };
            // 测试排除和非排除类型
            var noExcludedType = typeof(JsEnv);
            var excludedType = typeof(System.Tuple);

            // 验证结果
            Assert.IsFalse(XPuer.Gen.IsExcluded(noExcludedType), "IsExcluded对非排除类型应返回false");
            Assert.IsTrue(XPuer.Gen.IsExcluded(excludedType), "IsExcluded对排除类型应返回true");
        }
        finally
        {
            XPuer.Gen.ExcludeTypes = originalExcluded;
        }
    }

    [Test]
    public void GetTypeName()
    {
        // 测试基本类型
        var intType = typeof(int);
        var intTypeName = XPuer.Gen.GetTypeName(intType);
        Assert.AreEqual(intTypeName, "Int32", "GetTypeName应返回int的正确类型名称");

        // 测试数组类型
        var intArrayType = typeof(int[]);
        var intArrayTypeName = XPuer.Gen.GetTypeName(intArrayType);
        Assert.AreEqual(intArrayTypeName, "Int32[]", "GetTypeName应返回int数组的正确类型名称");

        // 测试多维数组类型
        var intMultiArrayType = typeof(int[,]);
        var intMultiArrayTypeName = XPuer.Gen.GetTypeName(intMultiArrayType);
        Assert.AreEqual(intMultiArrayTypeName, "Int32[,]", "GetTypeName应返回int多维数组的正确类型名称");

        // 测试泛型类型
        var listType = typeof(List<string>);
        var listTypeName = XPuer.Gen.GetTypeName(listType);
        Assert.AreEqual(listTypeName, "List<System.String>", "GetTypeName应返回泛型List<string>的正确类型名称");

        // 测试嵌套泛型类型
        var nestedType = typeof(Dictionary<int, List<string>>);
        var nestedTypeName = XPuer.Gen.GetTypeName(nestedType);
        Assert.AreEqual(nestedTypeName, "Dictionary<System.Int32,System.Collections.Generic.List<System.String>>", "GetTypeName应返回嵌套泛型类型的正确类型名称");

        // 测试空类型
        var voidType = typeof(void);
        var voidTypeName = XPuer.Gen.GetTypeName(voidType);
        Assert.AreEqual(voidTypeName, "Void", "GetTypeName应返回void的正确类型名称");

        // 测试嵌套类型
        var nestedGenericType = typeof(Dictionary<int, List<Dictionary<string, int>>>);
        var nestedGenericTypeName = XPuer.Gen.GetTypeName(nestedGenericType);
        Assert.AreEqual(nestedGenericTypeName, "Dictionary<System.Int32,System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String,System.Int32>>>", "GetTypeName应返回深度嵌套泛型类型的正确类型名称");
    }

    [Test]
    public void GetFriendlyName()
    {
        // 测试基本类型
        var intType = typeof(int);
        var intFriendlyName = XPuer.Gen.GetFriendlyName(intType);
        Assert.AreEqual(intFriendlyName, "System.Int32", "GetFriendlyName应返回int的正确友好名称");

        // 测试数组类型
        var intArrayType = typeof(int[]);
        var intArrayFriendlyName = XPuer.Gen.GetFriendlyName(intArrayType);
        Assert.AreEqual(intArrayFriendlyName, "System.Int32[]", "GetFriendlyName应返回int数组的正确友好名称");

        // 测试多维数组类型
        var intMultiArrayType = typeof(int[,]);
        var intMultiArrayFriendlyName = XPuer.Gen.GetFriendlyName(intMultiArrayType);
        Assert.AreEqual(intMultiArrayFriendlyName, "System.Int32[,]", "GetFriendlyName应返回int多维数组的正确友好名称");

        // 测试泛型类型
        var listType = typeof(List<string>);
        var listFriendlyName = XPuer.Gen.GetFriendlyName(listType);
        Assert.AreEqual(listFriendlyName, "System.Collections.Generic.List<System.String>", "GetFriendlyName应返回泛型List<string>的正确友好名称");

        // 测试嵌套类型
        var nestedType = typeof(Dictionary<int, List<string>>);
        var nestedFriendlyName = XPuer.Gen.GetFriendlyName(nestedType);
        Assert.AreEqual(nestedFriendlyName, "System.Collections.Generic.Dictionary<System.Int32,System.Collections.Generic.List<System.String>>", "GetFriendlyName应返回嵌套泛型类型的正确友好名称");

        // 测试空类型
        var nullType = typeof(void);
        var nullFriendlyName = XPuer.Gen.GetFriendlyName(nullType);
        Assert.AreEqual(nullFriendlyName, "System.Void", "GetFriendlyName应返回void的正确友好名称");
    }

    [Test]
    public void LinkModule()
    {
        // 准备package.json
        var packageJsonPath = XFile.PathJoin(XEnv.ProjectPath, "package.json");
        var tempPackageJsonPath = XFile.PathJoin(XEnv.ProjectPath, ".package.json");
        var needRestore = false;
        if (!XFile.HasFile(tempPackageJsonPath))
        {
            needRestore = true;
            var packagePath = ET.U3D.UTIL.XEditor.Utility.FindPackage().assetPath;
            var tsDir = XFile.PathJoin(packagePath, "Tests/Runtime/TypeScripts");
            if (XFile.HasFile(packageJsonPath)) XFile.CopyFile(packageJsonPath, tempPackageJsonPath);
            XFile.CopyFile(XFile.PathJoin(tsDir, "package.json"), packageJsonPath);
        }
        // 待删除模块
        var deleteModule = "node_modules/.puer/EP.U3D.PUER";

        // 测试ToModule
        var modules = XPuer.Gen.ToModule();
        Assert.IsTrue(modules.Contains(deleteModule), "模块列表应包含EP.U3D.PUER模块");
        modules.Remove(deleteModule);

        // 测试EP.U3D.PUER是否被删除
        XPuer.Gen.LinkModule(modules);
        var fixedJson = File.ReadAllText(packageJsonPath);
        Assert.IsFalse(fixedJson.Contains(deleteModule), "处理后的package.json不应包含已删除的模块");

        if (needRestore)
        {
            // 恢复package.json
            if (XFile.HasFile(tempPackageJsonPath))
            {
                XFile.CopyFile(tempPackageJsonPath, packageJsonPath);
                XFile.DeleteFile(tempPackageJsonPath);
            }
        }
    }

    [Test]
    public void IsMatch()
    {
        // 通配符模式应匹配任意参数列表
        var wildcardList = new List<string[]> { new string[] { "*" } };
        Assert.IsTrue(XPuer.Gen.IsMatch(wildcardList, new string[] { "UnityEngine.Vector3", "System.Single" }));

        // 完全相同的参数列表应匹配
        var exactList = new List<string[]> { new string[] { "UnityEngine.Vector3", "System.Single" } };
        Assert.IsTrue(XPuer.Gen.IsMatch(exactList, new string[] { "UnityEngine.Vector3", "System.Single" }));

        // 不同长度的参数列表不应匹配
        var differentLengthList = new List<string[]> { new string[] { "UnityEngine.Vector3" } };
        Assert.IsFalse(XPuer.Gen.IsMatch(differentLengthList, new string[] { "UnityEngine.Vector3", "System.Single" }));

        // 内容不同的参数列表不应匹配
        var differentContentList = new List<string[]> { new string[] { "UnityEngine.Vector3", "System.Int32" } };
        Assert.IsFalse(XPuer.Gen.IsMatch(differentContentList, new string[] { "UnityEngine.Vector3", "System.Single" }));

        // 多个模式中有一个匹配应返回true
        var multiplePatternList = new List<string[]> {
                new string[] { "UnityEngine.Vector3", "System.Int32" },
                new string[] { "UnityEngine.Vector3", "System.Single" }
            };
        Assert.IsTrue(XPuer.Gen.IsMatch(multiplePatternList, new string[] { "UnityEngine.Vector3", "System.Single" }));

        // 空参数列表应匹配空模式
        var emptyList = new List<string[]> { new string[0] };
        Assert.IsTrue(XPuer.Gen.IsMatch(emptyList, new string[0]));

        // 空的参数列表集合应返回false
        var emptyParamtersList = new List<string[]>();
        Assert.IsFalse(XPuer.Gen.IsMatch(emptyParamtersList, new string[] { "UnityEngine.Vector3" }));
    }
}

namespace TestNamespace
{
    // 测试DynamicBindings
    public class TestClass
    {
        public string ExcludeMember = "ExcludeMember";

        public void NormalMethod()
        {
            // 测试一般方法
        }

        [Obsolete("this is a obsolete method", true)]
        public void ObsoleteMethod()
        {
            // 测试过时的方法
        }
    }
}
#endif
