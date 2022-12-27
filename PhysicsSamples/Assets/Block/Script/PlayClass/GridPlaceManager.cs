using Hypertonic.GridPlacement;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class GridPlaceManager : Singleton<GridPlaceManager>
{
    private GameObject _selectedGridObject;
    //listing in
    [SerializeField] GameObjectEventChannelSO placeObjEvent;
    [SerializeField] VoidEventChannelSO confirmPlaceObjEvent;
    private void OnDisable()
    {
        placeObjEvent.OnEventRaised -= OnPlaceObject;
        confirmPlaceObjEvent.OnEventRaised -= HandleConfirmButtonPressed;
    }

    private void OnEnable()
    {
        placeObjEvent.OnEventRaised += OnPlaceObject;
        confirmPlaceObjEvent.OnEventRaised += HandleConfirmButtonPressed;
    }

    void OnPlaceObject(GameObject obj)
    {
        _selectedGridObject = obj;
    }

    void Update()
    {
        if (_selectedGridObject != null)
        {
            //返回是float型的！这个由滚轮向前（正数）还是向后（负数）滚决定
            var scale = Input.GetAxis("Mouse ScrollWheel");
            if (scale != 0)
            {
                _selectedGridObject.transform.Rotate(new Vector3(0, 90 * scale, 0));
                GridManagerAccessor.GridManager.HandleGridObjectRotated();
            }
            Debug.Log(scale);
        }
    }

    private void HandleConfirmButtonPressed()
    {
        bool placed = GridManagerAccessor.GridManager.ConfirmPlacement();

        if (placed)
        {
            _selectedGridObject = null;
        }
    }

    private void HandleCancelPlacementPressed()
    {
        GridManagerAccessor.GridManager.CancelPlacement();
        _selectedGridObject = null;
    }

    private void HandleDeleteObjectPressed()
    {
        GridManagerAccessor.GridManager.DeleteObject(_selectedGridObject);
        _selectedGridObject = null;
    }

    private void HandleRotateLeftPressed()
    {
        //_selectedGridObject.transform.Rotate(new Vector3(0, -90, 0));

        GridManagerAccessor.GridManager.HandleGridObjectRotated();
    }

    private void HandleRotateRightPressed()
    {
        //_selectedGridObject.transform.Rotate(new Vector3(0, 90, 0));

        GridManagerAccessor.GridManager.HandleGridObjectRotated();
    }
}
