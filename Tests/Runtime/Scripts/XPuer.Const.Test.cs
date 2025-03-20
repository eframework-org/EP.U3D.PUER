// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using EP.U3D.PUER;

public class TestXPuerConst
{
    [TestCase("myscript.jsc")]     // 单脚本路径
    [TestCase("myfolder/myscript.jsc")]    // 包含空格
    [TestCase("myfolder/my#script.jsc")]    // 包含#号
    [TestCase("myfolder/my[script].jsc")]   // 包含方括号
    [TestCase("myfolder\\myscript.jsc")]   // 包含反斜杠
    public void Tag(string path)
    {
        var expected = path.Contains("myfolder") ? "myfolder.jsc" : "default.jsc";
        var result = XPuer.Const.GenTag(path);

        // 验证标签是否正确
        Assert.AreEqual(expected, result, "GenTag应返回正确的标签名称");
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public void Mode(bool releaseMode, bool debugMode)
    {
        // 测试模式
        XPuer.Const.bReleaseMode = true;
        XPuer.Const.bDebugMode = true;
        XPuer.Const.releaseMode = releaseMode;
        XPuer.Const.debugMode = debugMode;

        Assert.AreEqual(releaseMode, XPuer.Const.ReleaseMode, "ReleaseMode属性应与设置的值一致");
        Assert.AreEqual(debugMode, XPuer.Const.DebugMode, "DebugMode属性应与设置的值一致");
    }

    [Test]
    public void Path()
    {
        var originLocalPath = XPuer.Const.LocalPath;
        // 测试路径
        var localPath = "localPath";
        XPuer.Const.bLocalPath = true;
        XPuer.Const.localPath = localPath;

        Assert.AreEqual(localPath, XPuer.Const.LocalPath, "LocalPath属性应与设置的路径一致");

        // 恢复本地路径
        XPuer.Const.localPath = originLocalPath;
    }

    [Test]
    public void Escape()
    {
        // 测试排除字符
        Assert.AreEqual("_", XPuer.Const.escapeChars["_"], "下划线应映射为下划线。");
        Assert.AreEqual("", XPuer.Const.escapeChars[" "], "空格应映射为空字符串。");
        Assert.AreEqual("", XPuer.Const.escapeChars["#"], "井号应映射为空字符串。");
        Assert.AreEqual("", XPuer.Const.escapeChars["["], "左方括号应映射为空字符串。");
        Assert.AreEqual("", XPuer.Const.escapeChars["]"], "右方括号应映射为空字符串。");
    }
}
#endif