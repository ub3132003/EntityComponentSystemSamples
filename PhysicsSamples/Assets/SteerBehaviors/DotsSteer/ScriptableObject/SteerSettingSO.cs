using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace Steer
{
    public abstract class SteerSettingSO : ScriptableObject
    {
        //supress "field declared but not used" warning. foldout is used by PropertyDrawer
#pragma warning disable 0414
        [SerializeField, HideInInspector]
        private bool foldout = true;
#pragma warning restore 0414

        public bool IsActive;

        [HideInInspector, Tooltip("Relative strength of this behavior's steering force.")]
        public float Weight = 1;

        public abstract void AddComponentData(Entity entity, EntityManager dstManager);
    }
    public interface IConvertToComponentData<T> where T : struct, IComponentData
    {
        public T ToComponent();
    }
}
