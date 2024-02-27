using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;
public static class AddOffsetAtName
{
    [MenuItem("Tools/AddOffsetAtName")]
    public static void AddOffsetAtNameAction()
    {
        Transform target = Selection.activeTransform;
        AddNameForChildren(target, "Offset");
    }

    private static void AddNameForChildren(Transform target, string add)
    {
        if (target == null) return;

        foreach (Transform tra in target)
        {
            tra.name += add;
            AddNameForChildren(tra, add);
        }
    }
}

#endif
