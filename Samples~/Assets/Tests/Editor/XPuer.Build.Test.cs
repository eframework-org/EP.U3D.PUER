// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using EP.U3D.UTIL;
using ET.U3D.UTIL;
using UnityEngine.TestTools;
using static ET.U3D.PUER.XPuer;

public class TestXPuerBuild : IPrebuildSetup
{
    void IPrebuildSetup.Setup()
    {
        // 创建处理器
        var handler = new Build() { ID = "Test/Build Test Scripts" };

        var buildDir = XFile.PathJoin(XPrefs.GetString(Build.Prefs.Output, Build.Prefs.OutputDefault), XPrefs.GetString(XEnv.Prefs.Channel, XEnv.Prefs.ChannelDefault), XEnv.Platform.ToString());
        var manifestFile = XFile.PathJoin(buildDir, XMani.Default);

        var report = XEditor.Tasks.Execute(handler);

        Assert.AreEqual(XEditor.Tasks.Result.Succeeded, report.Result, "资源构建应当成功");
        Assert.IsTrue(XFile.HasFile(manifestFile), "资源清单应当生成成功");

        var manifest = new XMani.Manifest();
        Assert.IsTrue(manifest.Read(manifestFile)(), "资源清单应当读取成功");

        foreach (var file in manifest.Files)
        {
            var path = XFile.PathJoin(buildDir, file.Name);
            Assert.IsTrue(XFile.HasFile(path), "文件应当存在于本地：" + file.Name);
            Assert.AreEqual(XFile.FileMD5(path), file.MD5, "文件MD5应当一致：" + file.Name);
            Assert.AreEqual(XFile.FileSize(path), file.Size, "文件大小应当一致：" + file.Name);
        }
    }

    [Test]
    public void Process() { }
}
#endif
