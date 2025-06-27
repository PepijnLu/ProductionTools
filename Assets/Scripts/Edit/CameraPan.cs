using UnityEngine;

public class CameraPanLimited : MonoBehaviour
{
    public Vector2 minPosition = new Vector2(-10f, -10f);
    public Vector2 maxPosition = new Vector2(10f, 10f);
    public float panSpeed = 0.01f; // Adjust for sensitivity

    private Vector3 lastMousePosition;
    private bool isPanning;

    void Update()
    {
        if(UIManager.instance.inMenu) return;

        if (Input.GetMouseButtonDown(2)) // Middle mouse pressed
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(2)) // Middle mouse released
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x * panSpeed, -delta.y * panSpeed, 0f);

            // Convert screen delta to world delta
            if (Camera.main.orthographic)
                move = Camera.main.transform.TransformDirection(move);
            else
                move = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.ScreenToWorldPoint(lastMousePosition);

            transform.position += move;
            ClampPosition();

            lastMousePosition = Input.mousePosition;
        }
    }

    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);
        transform.position = pos;
    }
}
