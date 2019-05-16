using System.IO;
using UnityEngine;

public class Flycam : MonoBehaviour {
    #region Editable properties

    [SerializeField] float cameraSensitivity = 90;
    [SerializeField] float normalMoveSpeed = 10;
    [SerializeField] float slowMoveFactor = 0.333f;
    [SerializeField] float fastMoveFactor = 3;

    #endregion  // Editable properties
    #region Private properties

    private static string fileName;

    #endregion  // Private properties
    #region Static methods

    /** \brief Set the name of the file where positional data should be written.
     * 
     * Applies to all cameras until another file name gets set.
     * 
     * \param fileName Posdata file name.
     */
    public static void SetFileName(string fileName) {
        Flycam.fileName = fileName;
    }

    #endregion  // Static methods
    #region Public methods

    /** \brief Rotate the Flycam depending on the mouse's movement.
     */
    public void UpdateRotation() {
        var rotationX = transform.localEulerAngles.y;
        var rotationY = transform.localEulerAngles.x;
        rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
        rotationY -= Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
        
        if (rotationY < 180) {
            rotationY = Mathf.Clamp(rotationY, -90, 90);
        } else {
            rotationY = Mathf.Clamp(rotationY, 270, 450);
        }
        
        transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.right);
    }

    /** \brief Move the Flycam depending on the user's keypresses.
     * 
     * W: Move camera forward.
     * S: Move camera backward.
     * A: Move camera left.
     * D: Move camera right.
     * Q: Move camera up.
     * E: Move camera down.
     */
    public void UpdatePosition() {
        var moveSpeed = normalMoveSpeed;
        if (GoingFast()) {
            moveSpeed *= fastMoveFactor;
        } else if (GoingSlow()) {
            moveSpeed *= slowMoveFactor;
        }

        var ver_direction = 0f;
        if (GoingUp()) {
            ver_direction = 1;
        } else if (GoingDown()) {
            ver_direction = -1;
        }

        transform.position += Time.deltaTime * moveSpeed * transform.forward * Input.GetAxis("Vertical");
        transform.position += Time.deltaTime * moveSpeed * transform.right * Input.GetAxis("Horizontal");
        transform.position += Time.deltaTime * moveSpeed * transform.up * ver_direction;
    }

    /** \brief Write this camera's positions and rotations from this frame into the csv file.
     */
    public void WriteToFile() {
        if (fileName == null) {
            return;
        }

        // Add a header explaining the fields in the file.
        var file_exists = File.Exists(fileName);
        var file = new StreamWriter(fileName, file_exists);
        if (!file_exists) {
            file.WriteLine("Camera," +
                "WorldPosX,WorldPosY,WorldPosZ," +
                "WorldRotX,WorldRotY,WorldRotZ," +
                "LocalPosX,LocalPosY,LocalPosZ," +
                "LocalRotX,LocalRotY,LocalRotZ," +
                "FoV,FocLen");
        }

        // Write the camera's data from the current frame.
        var cam = GetComponent<Camera>();
        var world_pos = cam.transform.position;
        var world_rot = cam.transform.eulerAngles;
        var local_pos = cam.transform.localPosition;
        var local_rot = cam.transform.localEulerAngles;
        file.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
            name,
            world_pos.x, world_pos.y, world_pos.z,
            world_rot.x, world_rot.y, world_rot.z,
            local_pos.x, local_pos.y, local_pos.z,
            local_rot.x, local_rot.y, local_rot.z,
            cam.fieldOfView, cam.focalLength);
        file.Close();
    }

    #endregion  // Public methods
    #region Private methods

    /** \brief Check if the Flycam should start descending.
     * 
     * \return `true` if E is being pressed.
     */
    bool GoingDown() {
        return Input.GetKey(KeyCode.E);
    }

    /** \brief Check if the Flycam should move fast.
     * 
     * \return `true` if either shift key is being pressed.
     */
    bool GoingFast() {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    /** \brief Check if the Flycam should move slow.
     * 
     * \return `true` if either control key is being pressed.
     */
    bool GoingSlow() {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    /** \brief Check if the Flycam should start ascending.
     * 
     * \return `true` if Q is being pressed.
     */
    bool GoingUp() {
        return Input.GetKey(KeyCode.Q);
    }

    #endregion  // Private methods
}
