using System;
using System.Collections;
using Unity.Entities;
using UnityEngine;


public static class NativeExtension
{
    public static int FindBuffIndex<T>(this DynamicBuffer<T> buffEffects , T ele) where T : struct , IBufferElementData, IEquatable<T>
    {
        var length = buffEffects.Length;
        for (int i = 0; i < length; i++)
        {
            if (buffEffects[i].Equals(ele))
            {
                return i;
            }
        }
        return -1;
    }
}
