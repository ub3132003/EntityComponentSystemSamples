using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ConvertAnimationDataEditor : EditorWindow
{
    private static EditorWindow window;

    [MenuItem("Test/ConverAnimationData")]
    static void Execute()
    {
        if (window == null)
            window = (ConvertAnimationDataEditor)GetWindow(typeof(ConvertAnimationDataEditor));
        window.minSize = new Vector2(300, 200);
        window.name = "动画转换工具";
        window.Show();
    }

    private AnimationClip clip;
    private AnimationDataSO animAsset;

    private void OnGUI()
    {
        using (new GUILayout.HorizontalScope("box"))
        {
            GUILayout.Label("AnimClip:", GUILayout.Width(60f));
            clip = EditorGUILayout.ObjectField(clip, typeof(AnimationClip), false) as AnimationClip;
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("SaveAsset:", GUILayout.Width(60f));
            animAsset = EditorGUILayout.ObjectField(animAsset, typeof(AnimationDataSO), false) as AnimationDataSO;
        }

        if (GUILayout.Button("Save"))
        {
            Save();
        }

    }

    private void Save()
    {
        var path = AssetDatabase.GetAssetPath(animAsset);
        var asset = AssetDatabase.LoadAssetAtPath<AnimationDataSO>(path);

        asset.frameDelta = 1f / 30f;
        asset.frameCount = Mathf.CeilToInt(clip.length / asset.frameDelta);
        asset.positions = new List<Vector3>(asset.frameCount);
        asset.scales = new List<Vector3>(asset.frameCount);
        asset.eulers = new List<Vector3>(asset.frameCount);
        for (int i = 0; i < asset.frameCount; ++i)
        {
            asset.positions.Add(Vector3.zero);
            asset.scales.Add(Vector3.one);
            asset.eulers.Add(Vector3.zero);
        }

        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);

            string propName = binding.propertyName;
            string transfomPath = binding.path; //Todo 没有记录 所有的结构动画。
            float timer = 0f;
            float maxTime = clip.length;
            int index = 0;
            while (timer < maxTime && index < asset.frameCount)
            {
                switch (propName)
                {
                    case "m_LocalPosition.x":
                        {
                            var pos = asset.positions[index];
                            pos.x = GetValue(curve.keys, timer);
                            asset.positions[index] = pos;

                        }
                        break;
                    case "m_LocalPosition.y":
                        {
                            var pos = asset.positions[index];
                            pos.y = GetValue(curve.keys, timer);
                            asset.positions[index] = pos;
                        }
                        break;
                    case "m_LocalPosition.z":
                        {
                            var pos = asset.positions[index];
                            pos.z = GetValue(curve.keys, timer);
                            asset.positions[index] = pos;
                        }
                        break;
                    case "m_LocalRotation.x":
                        {
                            var euler = animAsset.eulers[index];
                            euler.x = GetValue(curve.keys, timer);
                            animAsset.eulers[index] = euler;
                        }
                        break;
                    case "m_LocalRotation.y":
                        {
                            var euler = animAsset.eulers[index];
                            euler.y = GetValue(curve.keys, timer);
                            animAsset.eulers[index] = euler;
                        }
                        break;
                    case "m_LocalRotation.z":
                        {
                            var euler = animAsset.eulers[index];
                            euler.z = GetValue(curve.keys, timer);
                            animAsset.eulers[index] = euler;
                        }
                        break;

                    case "m_LocalScale.x":
                        {
                            var scale = animAsset.scales[index];
                            scale.x = GetValue(curve.keys, timer);
                            animAsset.scales[index] = scale;
                        }
                        break;
                    case "m_LocalScale.y":
                        {
                            var scale = asset.scales[index];
                            scale.y = GetValue(curve.keys, timer);
                            asset.scales[index] = scale;
                        }
                        break;
                    case "m_LocalScale.z":
                        {
                            var scale = asset.scales[index];
                            scale.z = GetValue(curve.keys, timer);
                            asset.scales[index] = scale;
                        }
                        break;
                }

                timer += asset.frameDelta;
                index++;
            }
        }

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
    }

    private float GetValue(Keyframe[] frames, float time)
    {
        int pre = 0;
        int next = 0;
        for (int i = 0; i < frames.Length; ++i)
        {
            var frame = frames[i];
            if (time <= frame.time)
            {
                next = i;
                break;
            }
        }
        pre = Mathf.Max(0, next - 1);

        var preFrame = frames[pre];
        var nextFrame = frames[next];

        if (pre == next)
            return nextFrame.time;

        float ret = preFrame.value + (nextFrame.value - preFrame.value) * (time - preFrame.time) / (nextFrame.time - preFrame.time);
        return ret;
    }

}