using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;

[Serializable]
[MaterialProperty("_BlockID", MaterialPropertyFormat.Float)]
public struct BlockID : IComponentData
{
    public float blockID;
}
