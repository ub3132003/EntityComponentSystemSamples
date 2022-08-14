 

using System.Collections.Generic;
using UnityEngine;
 

[CreateAssetMenu(menuName = "Test/CreaetAnimationData")]
public class AnimationDataSO : ScriptableObject
{
    public float frameDelta;
    public int frameCount;
    public List<Vector3> positions;
    public List<Vector3> eulers;
    public List<Vector3> scales;
}

 