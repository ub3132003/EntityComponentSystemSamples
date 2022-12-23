using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
public static class EntityHelp
{
    public static void SetParent(
        EntityManager em,
        Entity parent,
        Entity child,
        float3 localTranslation = default,
        quaternion localRotation = default)
    {
        em.SetComponentData(child, new Translation { Value = localTranslation });
        em.SetComponentData(child, new Rotation { Value = localRotation });

        // Add Parent
        if (!em.HasComponent<Parent>(child))
        {
            em.AddComponentData(child, new Parent { Value = parent });
        }
        else
        {
            em.SetComponentData(child, new Parent { Value = parent });
        }

        // Add LocalToParent
        if (!em.HasComponent<LocalToParent>(child))
        {
            em.AddComponentData(child, new LocalToParent());
        }
    }

    public static void SetParent(
        EntityCommandBuffer commandBuffer,
        ComponentDataFromEntity<Parent> parentFromEntity,
        ComponentDataFromEntity<LocalToParent> localToParentFromEntity,
        Entity parent,
        Entity child,
        float3 localTranslation,
        quaternion localRotation)
    {
        commandBuffer.SetComponent(child, new Translation { Value = localTranslation });
        commandBuffer.SetComponent(child, new Rotation { Value = localRotation });

        // Add Parent
        if (!parentFromEntity.HasComponent(child))
        {
            commandBuffer.AddComponent(child, new Parent { Value = parent });
        }
        else
        {
            commandBuffer.SetComponent(child, new Parent { Value = parent });
        }

        // Add LocalToParent
        if (!localToParentFromEntity.HasComponent(child))
        {
            commandBuffer.AddComponent(child, new LocalToParent());
        }
    }

    /// <summary>
    /// 用于新创建的对象，需要确保没有parent
    /// </summary>
    /// <param name="commandBuffer"></param>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    /// <param name="localTranslation"></param>
    /// <param name="localRotation"></param>
    public static void SetParent(
        EntityCommandBuffer commandBuffer,
        Entity parent,
        Entity child,
        float3 localTranslation,
        quaternion localRotation)
    {
        commandBuffer.SetComponent(child, new Translation { Value = localTranslation });
        commandBuffer.SetComponent(child, new Rotation { Value = localRotation });
        // Add Parent

        commandBuffer.AddComponent(child, new Parent { Value = parent });

        // Add LocalToParent

        commandBuffer.AddComponent(child, new LocalToParent());
    }
}
