using EP.U3D.UTIL;
using ET.U3D.UTIL;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Reflection;
using Puerts;
using ET.U3D.PUER;

namespace Plugins.Puer.Editor
{
    [XEditor.Const]
    [InitializeOnLoad]
    public class Configurate
    {
        static Configurate()
        {
            XPuer.Gen.Types = new List<Type>() {
                typeof(Handheld)
            };

            XPuer.Gen.Namespaces = new List<string>() {
                "System",
                "UnityEngine",
            };

            XPuer.Gen.Bindings = new List<Type>() {
                // Unity Engine
                typeof(IEnumerator),
                typeof(Coroutine),
                typeof(UnityEventBase),
                typeof(UnityEvent),
                typeof(UnityEvent<int>),
                typeof(UnityEvent<int,int>),
                typeof(UnityEvent<int,int,int>),
                typeof(UnityAction),
                typeof(UnityAction<int>),
                typeof(UnityAction<int,int>),
                typeof(UnityAction<int,int,int>),
                typeof(Mathf),

                // C# System
                typeof(Array),
                typeof(Guid),
                typeof(Hashtable),
                typeof(Delegate),
                typeof(DateTime),
                typeof(System.Object),
                typeof(List<int>),
                typeof(Dictionary<int,int>),

                // C# System.Reflection
                typeof(Type),
                typeof(MemberInfo),
                typeof(MethodBase),
                typeof(MethodInfo),
                typeof(ConstructorInfo),
                typeof(PropertyInfo),
                typeof(FieldInfo),
                typeof(ParameterInfo),

                // C# System.IO
                typeof(File),
                typeof(Directory),
                typeof(FileInfo),
                typeof(DirectoryInfo),
                typeof(FileSystemInfo),
                typeof(Path),
                typeof(Stream),

                // C# System.Text
                typeof(string),
                typeof(System.Text.Encoding),

                // Custom Types
                typeof(Vector3),
                typeof(Vector2),
                typeof(Color),
                typeof(JsEnv),
                typeof(ILoader),
                typeof(Enum),
            };

            XPuer.Gen.BaseTypes = new List<string> {
                "System.Void", "System.Tuple", "System.ValueType",
                "System.String", "System.Single", "System.Double", "System.Boolean",
                "System.Byte", "System.SByte", "System.Int16", "System.Int32",
                "System.Int64", "System.UInt16", "System.UInt32", "System.UInt64", "System.ArgIterator",
                "System.SpanExtensions", "System.TypedReference", "System.StringBuilderExt"};

            XPuer.Gen.ExcludeAssemblys = new List<string>{
                "UnityEditor.dll",
                "Assembly-CSharp-Editor.dll",
                "com.tencent.puerts.core.Editor.dll",
                "ICSharpCode.SharpZipLib.dll",
                "Utility.Editor.dll"};

            XPuer.Gen.ExcludeTypes = new List<string>(XPuer.Gen.BaseTypes) {
                "UnityEditor.GUI",
                "UnityEditor.GUI.ArrayDrawer",
                "UnityEditor.GUI.DisplayUtility",
                "UnityEditor.GUI.DrawerInfo",
                "UnityEditor.GUI.Display",
                "UnityEditor.GUI.Drawer",
                "UnityEditor.GUI.Element",
                "UnityEditor.GUI.TSProperties_CustomEditor",
                "UnityEngine.LightingSettings",
                "UnityEngine.InputManagerEntry",
                "UnityEngine.iPhone",
                "UnityEngine.iPhoneTouch",
                "UnityEngine.iPhoneKeyboard",
                "UnityEngine.iPhoneInput",
                "UnityEngine.iPhoneAccelerationEvent",
                "UnityEngine.iPhoneUtils",
                "UnityEngine.iPhoneSettings",
                "UnityEngine.AndroidInput",
                "UnityEngine.AndroidJavaProxy",
                "UnityEngine.BitStream",
                "UnityEngine.ADBannerView",
                "UnityEngine.ADInterstitialAd",
                "UnityEngine.RemoteNotification",
                "UnityEngine.LocalNotification",
                "UnityEngine.NotificationServices",
                "UnityEngine.MasterServer",
                "UnityEngine.Network",
                "UnityEngine.NetworkView",
                "UnityEngine.ParticleSystemRenderer",
                "UnityEngine.ParticleSystem.CollisionEvent",
                "UnityEngine.ProceduralPropertyDescription",
                "UnityEngine.ProceduralTexture",
                "UnityEngine.ProceduralMaterial",
                "UnityEngine.ProceduralSystemRenderer",
                "UnityEngine.TerrainData",
                "UnityEngine.HostData",
                "UnityEngine.RPC",
                "UnityEngine.AnimationInfo",
                "UnityEngine.UI.IMask",
                "UnityEngine.Caching",
                "UnityEngine.Handheld",
                "UnityEngine.UI.DefaultControls",
                "UnityEngine.TextureMipmapLimitGroups",
                "UnityEngine.MeshRenderer",
                "UnityEngine.AnimationClipPair", //Obsolete
                "UnityEngine.CacheIndex", //Obsolete
                "UnityEngine.SerializePrivateVariables", //Obsolete
                "UnityEngine.Networking.NetworkTransport", //Obsolete
                "UnityEngine.Networking.ChannelQOS", //Obsolete
                "UnityEngine.Networking.ConnectionConfig", //Obsolete
                "UnityEngine.Networking.HostTopology", //Obsolete
                "UnityEngine.Networking.GlobalConfig", //Obsolete
                "UnityEngine.Networking.ConnectionSimulatorConfig", //Obsolete
                "UnityEngine.Networking.DownloadHandlerMovieTexture", //Obsolete
                "AssetModificationProcessor", //Obsolete
                "AddressablesPlayerBuildProcessor", //Obsolete
                "UnityEngine.WWW", //Obsolete
                "UnityEngine.EventSystems.TouchInputModule", //Obsolete
                "UnityEngine.MovieTexture", //Obsolete[ERROR]
                "UnityEngine.NetworkPlayer", //Obsolete[ERROR]
                "UnityEngine.NetworkViewID", //Obsolete[ERROR]
                "UnityEngine.NetworkMessageInfo", //Obsolete[ERROR]
                "UnityEngine.UI.BaseVertexEffect", //Obsolete[ERROR]
                "UnityEngine.UI.IVertexModifier", //Obsolete[ERROR]
                "UnityEngine.Camera.RenderRequest", //Obsolete[ERROR]
                "UnityEngine.ArticulationBody", //Obsolete[ERROR]
                "UnityEngine.PhysicMaterial", //Obsolete[ERROR]
                //Windows Obsolete[ERROR]
                "UnityEngine.EventProvider",
                "UnityEngine.UI.GraphicRebuildTracker",
                "UnityEngine.GUI.GroupScope",
                "UnityEngine.GUI.ScrollViewScope",
                "UnityEngine.GUI.ClipScope",
                "UnityEngine.GUILayout.HorizontalScope",
                "UnityEngine.GUILayout.VerticalScope",
                "UnityEngine.GUILayout.AreaScope",
                "UnityEngine.GUILayout.ScrollViewScope",
                "UnityEngine.GUIElement",
                "UnityEngine.GUILayer",
                "UnityEngine.GUIText",
                "UnityEngine.GUITexture",
                "UnityEngine.ClusterInput",
                "UnityEngine.ClusterNetwork",
                "UnityEngine.QualitySetting",
                "UnityEngine.AudioSource",
                "UnityEngine.InputRegistering",
                "UnityEngine.QualitySettings",
                "UnityEngine.ClusterSerialization",

                 //System
                "System.Tuple",
                "System.Double",
                "System.Single",
                "System.ArgIterator",
                "System.SpanExtensions",
                "System.TypedReference",
                "System.StringBuilderExt",
                "System.IO.Stream",
                "System.Net.HttpListenerTimeoutManager",
                "System.Net.Sockets.SocketAsyncEventArgs",
                "System.ActivationContext",
                "System.AppDomainSetup",
                "System.AppDomainManager",
                "System.AppDomain",
                "System.ApplicationIdentity",
                "System.Activator",
                "System.CultureAwareComparer",
                "System.IAppDomainSetup",
                "System.OrdinalComparer",
                "System._AppDomain",
                "System.StringExtensions",
                "System.String",
            };

            XPuer.Gen.ExcludeMembers = new List<List<string>>() {
                new List<string>(){"UnityEngine.Material", "IsChildOf","UnityEngine.Material"},
                new List<string>(){"UnityEngine.Material", "RevertAllPropertyOverrides"},
                new List<string>(){"UnityEngine.Material", "IsPropertyOverriden","System.Int32"},
                new List<string>(){"UnityEngine.Material", "IsPropertyOverriden","System.String"},
                new List<string>(){"UnityEngine.Material", "IsPropertyLocked","System.Int32"},
                new List<string>(){"UnityEngine.Material", "IsPropertyLocked","System.String"},
                new List<string>(){"UnityEngine.Material", "IsPropertyLockedByAncestor","System.Int32"},
                new List<string>(){"UnityEngine.Material", "IsPropertyLockedByAncestor","System.String"},
                new List<string>(){"UnityEngine.Material", "SetPropertyLock","System.Int32","System.Boolean"},
                new List<string>(){"UnityEngine.Material", "SetPropertyLock","System.String","System.Boolean"},
                new List<string>(){"UnityEngine.Material", "ApplyPropertyOverride","UnityEngine.Material","System.Int32","System.Boolean"},
                new List<string>(){"UnityEngine.Material", "ApplyPropertyOverride","UnityEngine.Material","System.String","System.Boolean"},
                new List<string>(){"UnityEngine.Material", "RevertPropertyOverride","System.Int32"},
                new List<string>(){"UnityEngine.Material", "RevertPropertyOverride","System.String"},
                new List<string>(){"UnityEngine.Material", "parent"},
                new List<string>(){"UnityEngine.Material", "isVariant"},
                new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
                new List<string>(){"UnityEngine.WWW", "movie"},
#if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
#endif
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.Light", "shadowRadius"},
                new List<string>(){"UnityEngine.Light", "shadowAngle"},
                new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
                new List<string>(){"UnityEngine.Light", "SetLightDirty"},
                new List<string>(){"UnityEngine.LightProbeGroup", "dering"},
                new List<string>(){"UnityEngine.LightProbeGroup", "probePositions"},
                new List<string>(){"UnityEngine.WWW", "MovieTexture"},
                new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
#if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
#endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
                new List<string>(){"UnityEngine.AnimatorControllerParameter", "name"},
                new List<string>(){"UnityEngine.AudioSettings", "GetSpatializerPluginNames"},
                new List<string>(){"UnityEngine.AudioSettings", "SetSpatializerPluginName", "System.String"},
                new List<string>(){"UnityEngine.QualitySettings", "streamingMipmapsRenderersPerFrame"},
                new List<string>(){"UnityEngine.Input", "IsJoystickPreconfigured", "System.String"},
                new List<string>(){"UnityEngine.ParticleSystemForceField", "FindAll"},
                new List<string>(){"UnityEngine.Texture", "imageContentsHash"},
                new List<string>(){"UnityEngine.UI.Graphic", "OnRebuildRequested"},
                new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},
                new List<string>(){"UnityEngine.DrivenRectTransformTracker", "StartRecordingUndo"},
                new List<string>(){"UnityEngine.DrivenRectTransformTracker", "StopRecordingUndo"},
                new List<string>(){"UnityEngine.Terrain", "bakeLightProbesForTrees"},
                new List<string>(){"UnityEngine.Terrain", "deringLightProbesForTrees"},
                new List<string>(){"UnityEngine.GUIStyleState", "scaledBackgrounds"},
                new List<string>(){"UnityEngine.Caching", "SetNoBackupFlag", "UnityEngine.CachedAssetBundle"},
                new List<string>(){"UnityEngine.Caching", "SetNoBackupFlag", "System.String", "UnityEngine.Hash128"},
                new List<string>(){"UnityEngine.Caching", "ResetNoBackupFlag", "UnityEngine.CachedAssetBundle"},
                new List<string>(){"UnityEngine.Caching", "ResetNoBackupFlag", "System.String", "UnityEngine.Hash128"},
#if UNITY_ANDROID
                new List<string>(){"UnityEngine.Handheld", "SetActivityIndicatorStyle", "UnityEngine.iOS.ActivityIndicatorStyle"},
#endif
#if UNITY_IOS
                new List<string>(){"UnityEngine.Handheld", "SetActivityIndicatorStyle", "UnityEngine.AndroidActivityIndicatorStyle"},
#endif
                //System.IO
                new List<string>(){"System.IO.FileInfo", "GetAccessControl"},
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.Directory", "GetAccessControl", "System.String"},
                new List<string>(){"System.IO.Directory", "GetAccessControl", "System.String", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.Directory", "SetAccessControl", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.Directory", "CreateDirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.File", "SetAccessControl", "System.String", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.File", "GetAccessControl", "System.String"},
                new List<string>(){"System.IO.File", "GetAccessControl", "System.String", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.File", "Create", "System.String", "System.Int32", "System.IO.FileOptions", "System.Security.AccessControl.FileSecurity"},
                //System.Stream
                new List<string>(){"System.IO.FileStream", ".ctor", "System.String", "System.IO.FileMode", "System.Security.AccessControl.FileSystemRights", "System.IO.FileShare", "System.Int32", "System.IO.FileOptions"},
                new List<string>(){"System.IO.FileStream", ".ctor", "System.String", "System.IO.FileMode", "System.Security.AccessControl.FileSystemRights", "System.IO.FileShare", "System.Int32", "System.IO.FileOptions", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.FileStream", "GetAccessControl"},
                new List<string>(){"System.IO.FileStream", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.Stream", "Read", "System.Span<System.Byte>"},
                new List<string>(){"System.IO.Stream", "Write", "System.ReadOnlySpan<System.Byte>"},
                new List<string>(){"System.IO.Stream", "ReadAsync", "System.Memory<System.Byte>"},
                new List<string>(){"System.IO.Stream", "ReadAsync", "System.Memory<System.Byte>", "System.Threading.CancellationToken"},
                new List<string>(){"System.IO.Stream", "WriteAsync", "System.ReadOnlyMemory<System.Byte>"},
                new List<string>(){"System.IO.Stream", "WriteAsync", "System.ReadOnlyMemory<System.Byte>", "System.Threading.CancellationToken"},
                //System.Type
                new List<string>(){"System.Type", "MakeGenericSignatureType", "System.Type", "System.Type[]" },
                new List<string>(){"System.Type", "IsCollectible" },
                //System
                new List<string>(){"System.Net.WebProxy", "CreateDefaultProxy" },
                new List<string>(){"System.Threading.Thread", "CurrentContext"},
                new List<string>(){"System.MarshalByRefObject", "CreateObjRef", "System.Type"},
                new List<string>(){"System.Reflection.FieldInfo", "GetValueDirect", "System.TypedReference"},
                new List<string>(){"System.Reflection.FieldInfo", "SetValueDirect", "System.TypedReference", "System.Object"},
                new List<string>(){"System.Reflection.IntrospectionExtensions", "GetTypeInfo", "System.Type"},
                new List<string>(){"NUnit.Compatibility.AdditionalTypeExtensions", "IsCastableFrom", "System.Type", "System.Type"},
                //Puerts
                new List<string>(){"Puerts.JsEnv", "debugPort"},
                new List<string>(){"Puerts.JsEnv", "OnJsEnvCreate"},
                new List<string>(){"Puerts.JsEnv", "OnJsEnvDispose"},
            };
        }
    }
}
