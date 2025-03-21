import { XLog } from "EP.U3D.UTIL"
import { Vector3, Vector4, Color } from "UnityEngine"
import { PuerBehaviour } from "EP.U3D.PUER"

export class MyComponent extends PuerBehaviour {
    public TestNumber: number
    public TestString: string
    public TestBoolean: boolean
    public TestVector3: Vector3
    public TestVector4: Vector4
    public TestColor: Color
    public TestObject: PuerBehaviour
    public TestNumberArray: number[]
    public TestObjectArray: PuerBehaviour[]


    public Awake() {
        XLog.Info("TestComponent Awake: {0}", this.name)
    }

    public OnEnable() {
        XLog.Info("TestComponent OnEnable: {0}", this.name)
    }

    public Start() {
        XLog.Info("TestComponent Start: {0}", this.name)
    }

    public OnDisable() {
        XLog.Info("TestComponent OnDisable: {0}", this.name)
    }

    public OnDestroy() {
        XLog.Info("TestComponent OnDestroy: {0}", this.name)
    }

    public Update() {
        XLog.Info("TestComponent Update: {0}", this.name)
    }

    public LateUpdate() {
        XLog.Info("TestComponent LateUpdate: {0}", this.name)
    }

    public FixedUpdate() {
        XLog.Info("TestComponent FixedUpdate: {0}", this.name)
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

export class MyComponent2 extends PuerBehaviour {

}
