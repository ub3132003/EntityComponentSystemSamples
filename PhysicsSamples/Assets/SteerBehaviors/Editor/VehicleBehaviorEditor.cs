//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using System;
//using System.Linq;


//namespace Steer
//{
//    [CustomEditor(typeof(VehicleBehaviorSettingSO), true)]
//    public class VehicleBehaviorEditor : Editor
//    {
//        const string activeStyle = "ProgressBarBar";
//        const string inactiveStyle = "ProgressBarBack";


//        private VehicleBehaviorSettingSO targetSettings;

//        private SerializedProperty _behaviors;

//        private int toRemove = -1;
//        private void OnEnable()
//        {
//            targetSettings = (VehicleBehaviorSettingSO)target;
//            _behaviors = serializedObject.FindProperty("behaviors");
//        }

//        public override void OnInspectorGUI()
//        {
//            base.OnInspectorGUI();


//            //for (int i = 0; i < _behaviors.arraySize; i++)
//            //{
//            //    DrawBehaviorBox(targetSettings.GetBehavior(i), _behaviors.GetArrayElementAtIndex(i), i, true);
//            //}

//            //if (toRemove >= 0)
//            //{
//            //    if (VehicleBehaviorSettingSO.OnBehaviorRemoved != null) VehicleBehaviorSettingSO.OnBehaviorRemoved.Invoke(targetSettings, targetSettings.GetBehavior(toRemove));
//            //    AssetDatabase.RemoveObjectFromAsset(_behaviors.GetArrayElementAtIndex(toRemove).objectReferenceValue);
//            //    _behaviors.DeleteArrayElementAtIndex(toRemove);
//            //    toRemove = -1;
//            //}

//            GUILayout.BeginVertical("BOX");
//            GUILayout.Space(10);
//            GUILayout.BeginHorizontal();
//            GUILayout.FlexibleSpace();
//            if (GUILayout.Button("Add Behavior", GUILayout.Width(130)))
//            {
//                GenericMenu menu = new GenericMenu();
//                List<SteerSettingSO> behaviors = targetSettings.Behaviors.ToList();
//                var GetAllDerivedTypes = GetSubClassNames(typeof(SteerSettingSO));
//                foreach (Type type in GetAllDerivedTypes)
//                {
//                    if (type.IsAbstract) continue;
//                    if (behaviors.Any(x => x.GetType() == type))
//                    {
//                        menu.AddDisabledItem(new GUIContent(type.Name));
//                    }
//                    else
//                    {
//                        menu.AddItem(new GUIContent(type.Name), false, AddBehavior, type);
//                    }
//                }
//                menu.ShowAsContext();
//            }
//            GUILayout.FlexibleSpace();
//            GUILayout.EndHorizontal();
//            GUILayout.Space(10);
//            GUILayout.EndVertical();

//            serializedObject.ApplyModifiedProperties();
//        }

//        SteerSettingSO CreateBehavior(object behaviorType)
//        {
//            SteerSettingSO newBehavior = (SteerSettingSO)ScriptableObject.CreateInstance((Type)behaviorType);
//            newBehavior.hideFlags = HideFlags.HideInHierarchy;

//            AssetDatabase.AddObjectToAsset(newBehavior, AssetDatabase.GetAssetPath(target));
//            return newBehavior;
//        }

//        void AddBehavior(object behaviorType)
//        {
//            _behaviors.arraySize = _behaviors.arraySize + 1;
//            SteerSettingSO newBehavior = CreateBehavior(behaviorType);
//            if (VehicleBehaviorSettingSO.OnBehaviorAdded != null) VehicleBehaviorSettingSO.OnBehaviorAdded.Invoke(targetSettings, newBehavior);

//            _behaviors.GetArrayElementAtIndex(_behaviors.arraySize - 1).objectReferenceValue = (UnityEngine.Object)newBehavior;
//            serializedObject.ApplyModifiedProperties();
//        }

//        void DrawBehaviorBox(SteerSettingSO behavior, SerializedProperty property, int i, bool canRemove)
//        {
//            if (!behavior) return;
//            EditorGUILayout.BeginVertical("BOX");
//            EditorGUILayout.BeginHorizontal(behavior.IsActive ? activeStyle : inactiveStyle);
//            GUILayout.Space(20);

//            SerializedObject behaviorObject = new SerializedObject(property.objectReferenceValue);
//            SerializedProperty foldoutProperty = behaviorObject.FindProperty("foldout");

//            bool foldout = foldoutProperty.boolValue;

//#if UNITY_2018
//            foldout = EditorGUILayout.Foldout(foldout, behavior.GetType().Name);
//#else
//            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, behavior.GetType().Name);
//            EditorGUILayout.EndFoldoutHeaderGroup();
//#endif
//            foldoutProperty.boolValue = foldout;

//            behaviorObject.ApplyModifiedProperties();

//            GUILayout.FlexibleSpace();
//            if (canRemove)
//            {
//                if (GUILayout.Button("Remove", GUILayout.Width(60)))
//                {
//                    toRemove = i;
//                }
//            }

//            EditorGUILayout.EndHorizontal();

//            if (foldout)
//            {
//                GUILayout.Space(-20);

//                EditorGUILayout.PropertyField(property);
//            }


//            GUILayout.Space(5);

//            EditorGUILayout.EndVertical();
//            GUILayout.Space(10);
//        }

//        /// <summary>
//        /// C#获取一个类在其所在的程序集中的所有子类
//        /// </summary>
//        /// <param name="parentType">给定的类型</param>
//        /// <returns>所有子类的名称</returns>
//        public static List<Type> GetSubClassNames(Type parentType)
//        {
//            var subTypeList = new List<Type>();
//            var assembly = parentType.Assembly;//获取当前父类所在的程序集``
//            var assemblyAllTypes = assembly.GetTypes();//获取该程序集中的所有类型
//            foreach (var itemType in assemblyAllTypes)//遍历所有类型进行查找
//            {
//                var baseType = itemType.BaseType;//获取元素类型的基类
//                if (baseType != null)//如果有基类
//                {
//                    if (baseType.Name == parentType.Name)//如果基类就是给定的父类
//                    {
//                        subTypeList.Add(itemType);//加入子类表中
//                    }
//                }
//            }
//            return subTypeList;
//        }
//    }
//}
