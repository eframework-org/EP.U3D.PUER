// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS

using NUnit.Framework;
using static EP.U3D.PUER.XPuer;

public class TestXPuerPrefs
{
    [Test]
    public void Keys()
    {
        Assert.AreEqual(Prefs.ReleaseMode, "Puer/ReleaseMode");
        Assert.AreEqual(Prefs.DebugMode, "Puer/DebugMode");
        Assert.AreEqual(Prefs.DebugWait, "Puer/DebugWait");
        Assert.AreEqual(Prefs.DebugPort, "Puer/DebugPort");
        Assert.AreEqual(Prefs.AssetUri, "Puer/AssetUri");
        Assert.AreEqual(Prefs.LocalUri, "Puer/LocalUri");
        Assert.AreEqual(Prefs.RemoteUri, "Puer/RemoteUri");
    }

    [Test]
    public void Defaults()
    {
        Assert.AreEqual(Prefs.ReleaseModeDefault, false);
        Assert.AreEqual(Prefs.DebugModeDefault, false);
        Assert.AreEqual(Prefs.DebugWaitDefault, true);
        Assert.AreEqual(Prefs.DebugPortDefault, 9222);
        Assert.AreEqual(Prefs.AssetUriDefault, "Patch@Scripts@TS.zip");
        Assert.AreEqual(Prefs.LocalUriDefault, "Scripts/TS");
        Assert.AreEqual(Prefs.RemoteUriDefault, "${Prefs.Update/PatchUri}/Scripts/TS");
    }
}
#endif
