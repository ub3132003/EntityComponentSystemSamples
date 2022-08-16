using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
#if USE_ODIN
using Sirenix.OdinInspector
#endif

public class ThingSO : SerializableScriptableObject, IUIIcon
{
#if USE_ODIN
    [FoldoutGroup("Base Info")]
    [Tooltip("The name of the item")]
    [SerializeField] protected LocalizedString _name = default;
#endif

    [Tooltip("The name of the item")]
    [SerializeField] protected LocalizedString _name = default;
    [Tooltip("A preview image for the item")]
    [SerializeField]
    protected Sprite _previewImage = default;

    [Tooltip("A description of the item")]
    [SerializeField]
    protected LocalizedString _description = default;

    [Tooltip("A prefab reference for the model of the item")]
    [SerializeField]
    protected GameObject _prefab = default;

    public GameObject Prefab => _prefab;
    public LocalizedString Name => _name;
    public Sprite PreviewImage => _previewImage;
    public LocalizedString Description => _description;
}
public interface IUIIcon
{
    public Sprite PreviewImage
    {
        get;
    }

    public LocalizedString Description { get; }

    public LocalizedString Name { get; }
}
