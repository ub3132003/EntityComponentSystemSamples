using Unity.Entities;
using Unity.Mathematics;

public struct DroneComponent : IComponentData
{
    public float Magnitude;
    public float3 Direction;
    public float3 Offset;

    //public Vector3 position;
    //public Vector3 velocity;
    //public Vector3 smoothPosition;
    //public Vector3 smoothDirection;
    //public int team;
    //public float size;
    //public Bee enemyTarget;
    public Entity resourceTarget;

    //public bool dead = false;
    //public float deathTimer = 1f;
    //public bool isAttacking;
    //public bool isHoldingResource;
    public int index;
}
