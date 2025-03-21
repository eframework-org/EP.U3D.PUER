import { XLog } from "EP.U3D.UTIL"

export class MyComponent extends CS.EP.U3D.PUER.PuerBehaviour {
    //测试用属性
    public TestProp: number = 0

    public TestNumber: number = 0
    public TestString: string = ""
    public TestBoolean: boolean = false

    //测试用方法
    public TestFunc(obj: CS.UnityEngine.GameObject) {
        XLog.Info("TestFunc: {0}", obj.name)
    }

    public Awake() {
        XLog.Info("TestComponent Awake")
    }

    public OnEnable() {
        XLog.Info("TestComponent OnEnable")
    }

    public Start() {
        XLog.Info("TestComponent Start")
    }

    public OnDisable() {
        XLog.Info("TestComponent OnDisable")
    }

    public OnDestroy() {
        XLog.Info("TestComponent OnDestroy")
    }

    public Update() {
        XLog.Info("TestComponent Update")
    }

    public LateUpdate() {
        XLog.Info("TestComponent LateUpdate")
    }

    public FixedUpdate() {
        XLog.Info("TestComponent FixedUpdate")
    }

    public OnTriggerEnter(other: CS.UnityEngine.Collider) {
        XLog.Info("TestComponent OnTriggerEnter: {0}", other.name)
    }

    public OnTriggerExit(other: CS.UnityEngine.Collider) {
        XLog.Info("TestComponent OnTriggerExit: {0}", other.name)
    }

    public OnTriggerStay(other: CS.UnityEngine.Collider) {
        XLog.Info("TestComponent OnTriggerStay: {0}", other.name)
    }

    public OnCollisionEnter(other: CS.UnityEngine.Collision) {
        XLog.Info("TestComponent OnCollisionEnter: {0}", other.gameObject.name)
    }

    public OnCollisionExit(other: CS.UnityEngine.Collision) {
        XLog.Info("TestComponent OnCollisionExit: {0}", other.gameObject.name)
    }

    public OnCollisionStay(other: CS.UnityEngine.Collision) {
        XLog.Info("TestComponent OnCollisionStay: {0}", other.gameObject.name)
    }
}