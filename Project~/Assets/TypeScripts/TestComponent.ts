export class MyComponent extends CS.EP.U3D.PUER.PuerBehaviour {
    //测试用属性
    public TestProp: number = 0

    public TestNumber: number = 0
    public TestString: string = ""
    public TestBoolean: boolean = false

    //测试用方法
    public TestFunc(obj: CS.UnityEngine.GameObject) {
        CS.UnityEngine.Debug.Log(obj.name);
    }

    public Awake() {
        CS.UnityEngine.Debug.Log("TestComponent Awake")
    }

    public OnEnable() {
        CS.UnityEngine.Debug.Log("TestComponent OnEnable")
    }

    public Start() {
        CS.UnityEngine.Debug.Log("TestComponent Start")
    }

    public OnDisable() {
        CS.UnityEngine.Debug.Log("TestComponent OnDisable")
    }

    public OnDestroy() {
        CS.UnityEngine.Debug.Log("TestComponent OnDestroy")
    }

    public Update() {
        CS.UnityEngine.Debug.Log("TestComponent Update")
    }

    public LateUpdate() {
        CS.UnityEngine.Debug.Log("TestComponent LateUpdate")
    }

    public FixedUpdate() {
        CS.UnityEngine.Debug.Log("TestComponent FixedUpdate")
    }

    public OnTriggerEnter(other: CS.UnityEngine.Collider) {
        CS.UnityEngine.Debug.Log("TestComponent OnTriggerEnter")
    }

    public OnTriggerExit(other: CS.UnityEngine.Collider) {
        CS.UnityEngine.Debug.Log("TestComponent OnTriggerExit")
    }

    public OnTriggerStay(other: CS.UnityEngine.Collider) {
        CS.UnityEngine.Debug.Log("TestComponent OnTriggerStay")
    }

    public OnCollisionEnter(other: CS.UnityEngine.Collision) {
        CS.UnityEngine.Debug.Log("TestComponent OnCollisionEnter")
    }

    public OnCollisionExit(other: CS.UnityEngine.Collision) {
        CS.UnityEngine.Debug.Log("TestComponent OnCollisionExit")
    }

    public OnCollisionStay(other: CS.UnityEngine.Collision) {
        CS.UnityEngine.Debug.Log("TestComponent OnCollisionStay")
    }
}