using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Draggable UI element with Unity's IDragHandler and IEndDragHandler interfaces
/// </summary>
public class Draggable : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private Vector3 initPosition;

    private void Awake()
    {
        //set initial position
        initPosition = transform.position;
    }
    public void OnDrag(PointerEventData eventData)
    {
        //move to event data pos
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Drop FPS player prefab to terrain
        ApplicationController.Instance.DropFPSPlayerToTerrain(eventData.position);
        //Reset ui position to init pos
        transform.position = initPosition;
    }
}
