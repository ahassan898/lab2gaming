
using UnityEngine;

public class camerafollow : MonoBehaviour
{
    public Transform target;
    public float cameraSpeed;
    public float minY;
    public float maxY;
    public float minX;
    public float maxX;

    void FixedUpdate()
    {
        Vector2 newCamPosition = Vector2.Lerp(transform.position, target.position, cameraSpeed * Time.deltaTime);
        float clampedX = Mathf.Clamp(newCamPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(newCamPosition.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, -10f);
    }
}
