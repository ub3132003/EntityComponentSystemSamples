using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif
namespace  Steer
{
    public class VehicleBehaviorSettingSO : ScriptableObject
    {
        public static Action<VehicleBehaviorSettingSO> OnSteeringValuesModified;
        public static Action<VehicleBehaviorSettingSO, SteerSettingSO> OnBehaviorAdded;
        public static Action<VehicleBehaviorSettingSO, SteerSettingSO> OnBehaviorValuesModified;
        public static Action<VehicleBehaviorSettingSO, SteerSettingSO> OnBehaviorRemoved;

        [SerializeField]
        [Sirenix.OdinInspector.InlineEditor]
        private List<SteerSettingSO> behaviors = new List<SteerSettingSO>();

        public List<SteerSettingSO> Behaviors { get { return behaviors; } }

        public SteerSettingSO GetBehavior(int index)
        {
            if (index < 0 || index >= behaviors.Count) return null;
            return behaviors[index];
        }

#if UNITY_EDITOR
        [Button]
        public void AddButton()
        {
            GenericMenu menu = new GenericMenu();
            List<SteerSettingSO> behaviors = this.behaviors.ToList();
            var GetAllDerivedTypes = GetSubClassNames(typeof(SteerSettingSO));
            foreach (Type type in GetAllDerivedTypes)
            {
                if (type.IsAbstract) continue;
                if (behaviors.Any(x => x.GetType() == type))
                {
                    menu.AddDisabledItem(new GUIContent(type.Name));
                }
                else
                {
                    menu.AddItem(new GUIContent(type.Name), false, AddBehavior, type);
                }
            }
            menu.ShowAsContext();
        }

        SteerSettingSO CreateBehavior(object behaviorType)
        {
            SteerSettingSO newBehavior = (SteerSettingSO)ScriptableObject.CreateInstance((Type)behaviorType);
            newBehavior.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(newBehavior, AssetDatabase.GetAssetPath(this));
            return newBehavior;
        }

        void AddBehavior(object behaviorType)
        {
            var _behaviors = behaviors;

            SteerSettingSO newBehavior = CreateBehavior(behaviorType);
            if (VehicleBehaviorSettingSO.OnBehaviorAdded != null) VehicleBehaviorSettingSO.OnBehaviorAdded.Invoke(this, newBehavior);

            _behaviors.Add(newBehavior);
        }

        /// <summary>
        /// C#获取一个类在其所在的程序集中的所有子类
        /// </summary>
        /// <param name="parentType">给定的类型</param>
        /// <returns>所有子类的名称</returns>
        public static List<Type> GetSubClassNames(Type parentType)
        {
            var subTypeList = new List<Type>();
            var assembly = parentType.Assembly;//获取当前父类所在的程序集``
            var assemblyAllTypes = assembly.GetTypes();//获取该程序集中的所有类型
            foreach (var itemType in assemblyAllTypes)//遍历所有类型进行查找
            {
                var baseType = itemType.BaseType;//获取元素类型的基类
                if (baseType != null)//如果有基类
                {
                    if (baseType.Name == parentType.Name)//如果基类就是给定的父类
                    {
                        subTypeList.Add(itemType);//加入子类表中
                    }
                }
            }
            return subTypeList;
        }

#endif
    }
}
