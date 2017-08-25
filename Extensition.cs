using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensition{
    public static void SetActive(this Component component,bool show)
    {
        component.gameObject.SetActive(show);
    }

    public static bool IsNull(this GameObject go)
    {
#if UNITY_EDITOR
        return (object) go == null;
#endif
    }

    public static bool IsNull(this Component go)
    {
#if UNITY_EDITOR
        return (object) go == null;
#endif
    }
}
