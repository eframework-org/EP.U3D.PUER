// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using System.Linq;
using NUnit.Framework;
using static ET.U3D.PUER.XPuer;

public class TestXPuerSource
{
    [Test]
    public void Parse()
    {
        // 执行解析
        Source.Parse();

        // 验证不同目录的类是否被解析
        Assert.Contains("TestComponent@MyComponent", Source.Classes, "Source.Classes应包含MyComponent类");
        Assert.Contains("TestComponent@MyComponent", Source.Fields.Keys, "Source.Fields应包含MyComponent类的字段");
        Assert.Contains("Nested/TestNested@MyModule", Source.Classes, "Source.Classes应包含MyModule类");
        Assert.Contains("Nested/TestNested@MyModule", Source.Fields.Keys, "Source.Fields应包含MyModule类的字段");

        Assert.IsFalse(Source.Classes.Contains("NoExistClass"), "Source.Classes不应包含不存在类");
        Assert.IsFalse(Source.Fields.Keys.Contains("NoExistClass"), "Source.Fields不应包含不存在类");
    }

    [Test]
    public void Find()
    {
        Source.Parse();
        // 测试查找功能
        var index = Source.Find("Nested/TestNested@MyModule");
        Assert.AreNotEqual(-1, index, "Find应该为已存在的类返回有效索引");

        // 测试查找不存在的类
        var nonExistentIndex = Source.Find("NonExistentClass");
        Assert.AreEqual(-1, nonExistentIndex, "Find应该为不存在的类返回-1");
    }
}
#endif
