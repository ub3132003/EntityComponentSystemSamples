using UnityEngine;
using UnityEngine.EventSystems;
namespace RectTransformExtensions
{
    public static class RectTransformExtension
    {
        private static readonly Vector3[] s_Corners = new Vector3[4];
        /// <summary>
        /// ui 显示在world坐标上,overlay 模式
        /// </summary>
        public static Vector2 ConverToWorldPoint(this Transform self, RectTransform parent,  Vector2 offset)
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, self.position);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, null, out var vector2))
            {
                return vector2 + offset;
            }
            return new Vector2();
        }

        /// <summary>
        /// 给定rect 和坐标, 给出ui在世界坐标的映射位置
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="worldPosition"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Vector2 ConverToWorldPoint(RectTransform canvas, Vector3 worldPosition, Vector2 offset)
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screenPoint, null, out var vector2))
            {
                return vector2 + offset;
            }
            return new Vector2();
        }

        public static bool Contains(this RectTransform self, PointerEventData eventData)
        {
            var selfBounds = GetBounds(self);
            var worldPos = Vector3.zero;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                self,
                eventData.position,
                eventData.pressEventCamera,
                out worldPos);
            worldPos.z = 0f;
            return selfBounds.Contains(worldPos);
        }

        public static bool Contains(this RectTransform self, RectTransform target)
        {
            var selfBounds = GetBounds(self);
            var targetBounds = GetBounds(target);
            return selfBounds.Contains(new Vector3(targetBounds.min.x, targetBounds.min.y, 0f)) &&
                selfBounds.Contains(new Vector3(targetBounds.max.x, targetBounds.max.y, 0f)) &&
                selfBounds.Contains(new Vector3(targetBounds.min.x, targetBounds.max.y, 0f)) &&
                selfBounds.Contains(new Vector3(targetBounds.max.x, targetBounds.min.y, 0f));
        }

        /// <summary>
        /// Bounds
        /// </summary>
        private static Bounds GetBounds(this RectTransform self)
        {
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            self.GetWorldCorners(s_Corners);
            for (var index2 = 0; index2 < 4; ++index2)
            {
                min = Vector3.Min(s_Corners[index2], min);
                max = Vector3.Max(s_Corners[index2], max);
            }

            max.z = 0f;
            min.z = 0f;

            Bounds bounds = new Bounds(min, Vector3.zero);
            bounds.Encapsulate(max);
            return bounds;
        }
    }
}
