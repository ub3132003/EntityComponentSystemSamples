using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public class launchIndicatorLine : MonoBehaviour
{
    [SerializeField]
    LineRenderer line;

    public void SetPosition(int index , Vector3 point) => line.SetPosition(index, point);

    private void OnEnable()
    {
    }

    private void Update()
    {
        // 鼠标地面检测
        var physicsWorldSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        Vector2 mousePosition = Input.mousePosition;
        UnityEngine.Ray unityRay = Camera.main.ScreenPointToRay(mousePosition);
        var rayInput = new RaycastInput
        {
            Start = unityRay.origin,
            End = unityRay.origin + unityRay.direction * 100f,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << 11,//地面层
                GroupIndex = 0
            }
        };

        bool haveHit = collisionWorld.CastRay(rayInput, out var hit);

        if (haveHit)
        {
            SetPosition(1, hit.Position);
        }
    }
}
