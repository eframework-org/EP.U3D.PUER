export class MyModule {

    public testProp: number //测试用属性

    private static obj: any
    public static get Obj() { return MyModule.obj }
    protected static set Obj(value) { MyModule.obj = value }

    public Start(go: any) {
        MyModule.Obj = go
    }

    public OnGUI() { }

    public Update() { }

    public LateUpdate() { }

    public FixedUpdate() { }

    public OnDestroy() { }
}