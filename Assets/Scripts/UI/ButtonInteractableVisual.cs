
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class ButtonInteractableVisual : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Transform buttonBaseTransform;

    private float maxOffsetAlongNormal;
    private Vector2 planarOffset;

    private HashSet<PointerEventData> collidingPointers;

    protected bool started = false;

    protected virtual void Start()
    {
        collidingPointers = new HashSet<PointerEventData>();
        maxOffsetAlongNormal = Vector3.Dot(transform.position - buttonBaseTransform.position, -1f * buttonBaseTransform.forward);
        Vector3 pointOnPlane = transform.position - maxOffsetAlongNormal * buttonBaseTransform.forward;
        planarOffset = new Vector2(
                            Vector3.Dot(pointOnPlane - buttonBaseTransform.position, buttonBaseTransform.right),
            Vector3.Dot(pointOnPlane - buttonBaseTransform.position, buttonBaseTransform.up));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        collidingPointers.Add(eventData);
        UpdateComponentPosition();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        collidingPointers.Remove(eventData);
        UpdateComponentPosition();
    }

    private void UpdateComponentPosition()
    {
        float closestDistance = maxOffsetAlongNormal;
        foreach (PointerEventData pointer in collidingPointers)
        {
            float collisionDistance = Vector3.Dot(new Vector3(pointer.position.x, 0, pointer.position.y) - buttonBaseTransform.position, -1f * buttonBaseTransform.forward);

            if (collisionDistance < 0f)
            {
                collisionDistance = 0f;
            }

            closestDistance = Mathf.Min(collisionDistance, closestDistance);
        }

        transform.position = buttonBaseTransform.position +
                             buttonBaseTransform.forward * (-1f * closestDistance) +
                             buttonBaseTransform.right * planarOffset.x +
                             buttonBaseTransform.up * planarOffset.y;
    }


}
