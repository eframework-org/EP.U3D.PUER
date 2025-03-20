// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

//#region Core
Object.defineProperty(globalThis, 'polyfill:csharp', { value: globalThis.CS, enumerable: true, configurable: false, writable: false })
Object.defineProperty(globalThis, 'polyfill:puerts', { value: globalThis.puer, enumerable: true, configurable: false, writable: false })

puer.$typeof = function (cls) {
    if (cls.__p_innerType) return cls.__p_innerType
    return cls
}

export function NewObject(type, args) {
    let nargs = []
    if (args) {
        for (let i = 0; i < args.Length; i++) {
            nargs.push(args.get_Item(i))
        }
    }
    let obj = new type(...nargs)
    return obj
}

export function FuncApply(obj, method, args) {
    if (!obj || !(method in obj)) return
    let func = obj[method]
    if (typeof (func) !== "function") return
    let nargs = []
    if (args) {
        for (let i = 0; i < args.Length; i++) {
            nargs.push(args.get_Item(i))
        }
    }
    return func.apply(obj, nargs)
}

export function InitField(obj, field, value, arr) {
    if (!obj || !field) return
    if (arr > 0) {
        let act = (arr >> 2) & 0x3
        let idx = arr & 0x3
        if (act == 1) { // add
            let list = obj[field]
            if (list == null) {
                list = []
                obj[field] = list
            }
            list.push(value)
        } else if (act == 2 && idx >= 0) { // modify
            let list = obj[field]
            if (list) list[idx] = value
        } else if (act == 3 && idx >= 0) { // remove
            let list = obj[field]
            if (list) list.splice(idx, 1)
        }
    } else {
        obj[field] = value
    }
}
//#endregion

//#region Mono
const PuerBehaviour = CS.EP.U3D.PUER.PuerBehaviour
CS.EP.U3D.PUER.PuerBehaviour = new Proxy(class {
    constructor(proxy) {
        this.CProxy = proxy
        return new Proxy(this, {
            get(target, prop) {
                if (prop == "__p_innerType") return
                if (prop in target) {
                    return target[prop]
                }
                if (prop in PuerBehaviour.prototype) {
                    return PuerBehaviour.prototype[prop]
                }
            }
        })
    }

    //#region properties
    get transform() { return this.CProxy.transform }

    get gameObject() { return this.CProxy.gameObject }

    get name() { return this.CProxy.name }

    get enabled() { return this.CProxy.enabled } //undefined
    set enabled(v) { this.CProxy.enabled = v }

    get isActiveAndEnabled() { return this.CProxy.isActiveAndEnabled } //undefined

    get useGUILayout() { return this.CProxy.useGUILayout } //undefined
    set useGUILayout(v) { this.CProxy.useGUILayout = v }

    get tag() { return this.CProxy.tag }
    set tag(v) { this.CProxy.tag = v }

    get hideFlags() { return this.CProxy.hideFlags }
    set hideFlags(v) { this.CProxy.hideFlags = v }
    //#endregion

    //#region not supported methods
    StartCoroutine(methodName = null, obj = null) { throw new Error("NotImplementedException:Please use async/await instead.") }

    StopCoroutine(methodName) { throw new Error("NotImplementedException:Please use async/await instead.") }

    StopAllCoroutines() { throw new Error("NotImplementedException:Please use async/await instead.") }

    static FindObjectsOfTypeAll(type) { return new Error("Obsolete:Please use Resources.FindObjectsOfTypeAll instead.") }

    static FindObjectsOfTypeIncludingAssets(type) { return new Error("Obsolete:Please use Resources.FindObjectsOfTypeAll instead.") }

    static FindSceneObjectsOfType(type) { return new Error("Obsolete:use Object.FindObjectsByType instead.") }

    Invoke(methodName, time) { return new Error("NotImplementedException:Not supported yet") }

    InvokeRepeating(methodName, time, repeatRate) { return new Error("NotImplementedException:Not supported yet") }

    CancelInvoke(methodName) { return new Error("NotImplementedException:Not supported yet") }

    IsInvoking(methodName) { return new Error("NotImplementedException:Not supported yet") }

    SendMessageUpwards(methodName, value = null, options = CS.UnityEngine.SendMessageOptions.RequireReceiver) { return new Error("NotImplementedException:Not supported yet") }

    SendMessage(methodName, value = null, options = CS.UnityEngine.SendMessageOptions.RequireReceiver) { return new Error("NotImplementedException:Not supported yet") }

    BroadcastMessage(methodName, value = null, options = CS.UnityEngine.SendMessageOptions.RequireReceiver) { return new Error("NotImplementedException:Not supported yet") }
    //#endregion

    //#region static methods
    static Instantiate(original, positionOrParent, rotation, parent) {
        // 检查参数数量和类型
        if (arguments.length === 1) return CS.UnityEngine.Object.Instantiate(original)
        else if (arguments.length === 2) return CS.UnityEngine.Object.Instantiate(original, positionOrParent)
        else if (arguments.length === 3) return CS.UnityEngine.Object.Instantiate(original, positionOrParent, rotation)
        else if (arguments.length === 4) return CS.UnityEngine.Object.Instantiate(original, positionOrParent, rotation, parent)
    }

    static Destroy(obj, delay) {
        if (arguments.length === 1) return CS.UnityEngine.Object.Destroy(obj)
        else if (arguments.length === 2) return CS.UnityEngine.Object.Destroy(obj, delay)
    }

    static DestroyImmediate(obj, allowDestroyingAssets) {
        if (arguments.length === 1) return CS.UnityEngine.Object.DestroyImmediate(obj)
        else if (arguments.length === 2) return CS.UnityEngine.Object.DestroyImmediate(obj, allowDestroyingAssets)
    }

    static op_Implicit(obj) { return CS.UnityEngine.Object.op_Implicit(obj) }

    static op_Equality(a, b) { return CS.UnityEngine.Object.op_Equality(a, b) }

    static op_Inequality(a, b) { return CS.UnityEngine.Object.op_Inequality(a, b) }

    static Equals(a, b) { return CS.UnityEngine.Object.Equals(a, b) }

    static ReferenceEquals(a, b) { return CS.UnityEngine.Object.ReferenceEquals(a, b) }

    static FindObjectOfType(type, includeInactive = false) { return CS.UnityEngine.Object.FindObjectOfType(type, includeInactive) }

    static FindObjectsOfType(type, includeInactive = false) { return CS.UnityEngine.Object.FindObjectsOfType(type, includeInactive) }

    static FindObjectsByType(type, findObjectsInactive, sortMode) {
        if (arguments.length === 2) return CS.UnityEngine.Object.FindObjectsByType(type, sortMode)
        else if (arguments.length === 3) return CS.UnityEngine.Object.FindObjectsByType(type, findObjectsInactive, sortMode)
    }

    static DontDestroyOnLoad(target) { if (target) return CS.UnityEngine.Object.DontDestroyOnLoad(target) }

    static FindFirstObjectByType(type, findObjectsInactive) {
        if (arguments.length === 1) return CS.UnityEngine.Object.FindFirstObjectByType(type)
        else if (arguments.length === 2) return CS.UnityEngine.Object.FindFirstObjectByType(type, findObjectsInactive)
    }

    static FindAnyObjectByType(type, findObjectsInactive) {
        if (arguments.length === 1) return CS.UnityEngine.Object.FindAnyObjectByType(type)
        else if (arguments.length === 2) return CS.UnityEngine.Object.FindAnyObjectByType(type, findObjectsInactive)
    }

    //#endregion

    //#region methods
    GetComponent(type) { return this.CProxy.GetComponent(type) }

    GetComponents(type, result) {
        if (arguments.length === 1) return this.CProxy.GetComponents(type)
        else if (arguments.length === 2) return this.CProxy.GetComponents(type, result)
    }

    GetComponentInChildren(type, includeInactive = false) { return this.CProxy.GetComponentInChildren(type, includeInactive) }

    GetComponentsInChildren(type, includeInactive = false) { return this.CProxy.GetComponentsInChildren(type, includeInactive) }

    GetComponentInParent(type, includeInactive = false) { return this.CProxy.GetComponentInParent(type, includeInactive) }

    GetComponentsInParent(type, includeInactive = false) { return this.CProxy.GetComponentsInParent(type, includeInactive) }

    TryGetComponent(type, component) { return this.CProxy.TryGetComponent(type, component) }

    CompareTag(tag) { return this.CProxy.CompareTag(tag) }

    GetInstanceID() { return this.CProxy.GetInstanceID() }

    GetHashCode() { return this.CProxy.GetHashCode() }

    GetType() { return this.CProxy.GetType() }

    ToString() { return this.CProxy.ToString() }

    Equals(other) { return this.CProxy.Equals(other) }
    //#endregion
}, {
    get(target, prop) {
        if (prop == "__p_innerType") return
        if (prop in target) {
            return target[prop]
        }
        if (prop in PuerBehaviour) {
            return PuerBehaviour[prop]
        }
    }
})

CS.UnityEngine.MonoBehaviour = CS.EP.U3D.PUER.PuerBehaviour
//#endregion