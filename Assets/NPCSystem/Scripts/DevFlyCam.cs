/*
 * Used in devcamera
 */

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DevFlyCam : MonoBehaviour
{
    public float initialSpeed = 10f;
    public float increasedSpeed = 1.25f;

    [SerializeField]
    private float sensitivity = 90f;
    private float currentSpeed = 0f;
    private bool moving = false;

    [SerializeField]
    GameObject[] panels;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        // disable all other cameras and other unnecessary stuff in dev phase

        Camera[] cameras = GameObject.FindObjectsOfType<Camera>();

        foreach (Camera c in cameras)
        {
            if (c != this.GetComponent<Camera>())
                c.gameObject.SetActive(false);
        }

        foreach (GameObject g in panels)
        {
            if (g != null)
                g.SetActive(false);
        }

    }

    private void Update()
    {
        // movement
        bool lastMoving = moving;
        Vector3 deltaPosition = Vector3.zero;

        if (moving)
            currentSpeed += increasedSpeed * Time.deltaTime;

        moving = false;

        deltaPosition = AddMovementWithKey(KeyCode.W, deltaPosition, transform.forward);
        deltaPosition = AddMovementWithKey(KeyCode.S, deltaPosition, -transform.forward);
        deltaPosition = AddMovementWithKey(KeyCode.D, deltaPosition, transform.right);
        deltaPosition = AddMovementWithKey(KeyCode.A, deltaPosition, -transform.right);

        if (moving)
        {
            if (moving != lastMoving)
                currentSpeed = initialSpeed;

            transform.position += deltaPosition * currentSpeed * Time.deltaTime;
        }
        else currentSpeed = 0f;            

        // rotation
        float rotX = transform.localEulerAngles.y;
        float rotY = transform.localEulerAngles.x;

        rotX += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        rotY -= Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        if (rotY < 180)
            rotY = Mathf.Clamp(rotY, -90, 90);
        else
            rotY = Mathf.Clamp(rotY, 270, 450);

        transform.localRotation = Quaternion.AngleAxis(rotX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotY, Vector3.right);
    }

    private Vector3 AddMovementWithKey(KeyCode keyCode, Vector3 deltaPosition, Vector3 directionVector)
    {
        if (Input.GetKey(keyCode))
        {
            moving = true;
            deltaPosition += directionVector;
        }

        return deltaPosition;

    }
}
