using UnityEngine;

public class GridObject : MonoBehaviour
{
    private Vector3 lastPosition;
    
    private void Start()
    {
        lastPosition = transform.position;
        GridManager.UpdateObjectPosition(transform, lastPosition, transform.position);
    }
    
    private void Update()
    {
        if (transform.position != lastPosition)
        {
            GridManager.UpdateObjectPosition(transform, lastPosition, transform.position);
            lastPosition = transform.position;
        }
    }

    private void OnDestroy()
    {
        GridManager.RemoveObject(transform, lastPosition);
    }
}