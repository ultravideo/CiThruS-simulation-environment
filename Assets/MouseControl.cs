using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour
{
    #region Editable properties

    [SerializeField] GameObject transformGizmoRef;
    [SerializeField] float dragSpeed = 0.3f;
    [SerializeField] Material camMaterialRef;
    [SerializeField] Material camHighlightMaterialRef;

    #endregion

    #region Private properties

    private new Camera camera;
    private static List<GameObject> selectedCams = new List<GameObject>();
    private static GameObject selectedRig;
    private static GameObject gizmo;
    private static GameObject transformGizmo;
    private static Material camMaterial;
    private static Material camHighlightMaterial;
    private bool highlighted = false;
    private static GameObject selectedCar = null;

    #endregion

    #region Monobehaviour methods

    void Start()
    {
        camera = GetComponent<Camera>();
        transformGizmo = transformGizmoRef;
        camMaterial = camMaterialRef;
        camHighlightMaterial = camHighlightMaterialRef;
    }

    // Update is called once per frame
    void Update()
    {
        if (camera.enabled && !CursorLocked())
        {
            // Click LMB to select an object
            if (Input.GetMouseButtonDown(0))
            {
                SelectObject(Input.GetKey(KeyCode.LeftControl), Input.GetKey(KeyCode.LeftShift));
            }
            // Click RMB to cancel any selections
            else if (Input.GetMouseButtonDown(1))
            {
                DeSelect();
                selectedCar = null;
            }
        }
    }

    #endregion

    #region static methods

    /** \brief Check if the cursor has been locked to the window.
     * 
     * \return `true` if the cursor has been locked.
     */
    public static bool CursorLocked() {
        return Cursor.lockState == CursorLockMode.Locked;
    }

    /** \brief Lock / unlock the cursor to / from the window.
     */
    public static void ToggleCursorLock() {
        Cursor.lockState = (CursorLocked() ? CursorLockMode.None : CursorLockMode.Locked);
    }

    public static GameObject GetSelectedRig() {
        return selectedRig;
    }

    public static GameObject GetSelectedCar() {
        return selectedCar;
    }

    #endregion

    #region private methods

    // Selects a camera or one of the transform gizmo arrows on left mouse click.
    // bool multiSelect : determines whether we add to the selection or override it.
    // bool rigSelect : determines whether we try to select a rig or not.
    private void SelectObject(bool multiSelect, bool rigSelect)
    {
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hitInfo);
        if (hit)
        {
            if (hitInfo.collider.gameObject.GetComponent<Rigfit>() != null)
            {
                selectedCar = hitInfo.collider.gameObject;
                return;
            }
            else if (hitInfo.collider.transform.parent.GetComponent<Camera>() != null)
            {
                // If a rig is selected, nothing else (rigs or cameras) can be selected as well.
                // If a camera is selected, only other individual cameras (that may belong to a rig) can be selected as well.
                if (rigSelect)
                {
                    SelectRig(hitInfo);
                    return;
                }
                if (!multiSelect || selectedRig != null)
                {
                    DeSelect();
                }
                SelectCamToMove(hitInfo);
            }
            if (hitInfo.collider.transform.parent.tag == "TransformGizmo")
            {
                IEnumerator coroutine = MoveCam(hitInfo.collider.transform.name);
                StartCoroutine(coroutine);
            }
        }
    }

    // Cancels any object selections and removes the gizmo.
    private void DeSelect()
    {
        if (selectedCams.Count > 0)
        {
            foreach (GameObject cam in selectedCams)
            {
                cam.GetComponent<MouseControl>().Highlight();
            }
            selectedCams.Clear();
        }
        if (selectedRig != null) 
        {
            foreach (var cam in selectedRig.GetComponentsInChildren<Flycam>()) 
            {
                cam.GetComponent<MouseControl>().Highlight();
            }
            selectedRig = null;
        }
        if (gizmo != null)
        {
            Destroy(gizmo);
            gizmo = null;
        }
    }

    // Makes the cam selection and spawns the transform gizmo.
    // RaycastHit hitInfo : Contains the information on the raycast performed in SelectObject().
    private void SelectCamToMove(RaycastHit hitInfo)
    {
        bool isSelected = false;
        foreach (GameObject cam in selectedCams)
        {
            if (hitInfo.collider.transform.parent.gameObject == cam)
            {
                isSelected = true;
            }
        }
        if (!isSelected)
        {
            var selectedCam = hitInfo.collider.transform.parent.gameObject;
            selectedCams.Add(selectedCam);
            selectedCam.GetComponent<MouseControl>().Highlight();
            Vector3 selectedForward = selectedCam.transform.forward;
            selectedForward.y = 0;
            if (gizmo == null)
            {
                gizmo = Instantiate(transformGizmo);
                gizmo.transform.SetPositionAndRotation(hitInfo.collider.transform.position,
                                                       Quaternion.LookRotation(selectedForward, Vector3.up));
            }
            else
            {
                gizmo.transform.SetPositionAndRotation(GizmoPosition(), gizmo.transform.rotation);
            }
        }
        else
        {
            GameObject toRemove = hitInfo.collider.transform.parent.gameObject;
            // Remove clicked cam from selected cams.
            int index = -1;
            foreach (GameObject cam in selectedCams)
            {
                if (cam == toRemove)
                {
                    index = selectedCams.IndexOf(cam);
                }
            }
            selectedCams[index].GetComponent<MouseControl>().Highlight();
            selectedCams.RemoveAt(index);
            if (selectedCams.Count == 0)
            {
                Destroy(gizmo);
                gizmo = null;
            }
            else
            {
                gizmo.transform.position = GizmoPosition();
            }
        }
    }

    // Selects the whole rig of cameras to move if the camera is part of a rig.
    private void SelectRig(RaycastHit hitInfo)
    {
        if (hitInfo.collider.transform.parent.parent == null)
        {
            return;
        }
        if (hitInfo.collider.transform.parent.parent.gameObject.name.Substring(0, 3) == "Rig")
        {
            DeSelect();
            selectedRig = hitInfo.collider.transform.parent.parent.gameObject;
            for (int i = 0; i < selectedRig.transform.childCount; ++i)
            {
                selectedRig.transform.GetChild(i).gameObject.GetComponent<MouseControl>().Highlight();
            }
            gizmo = Instantiate(transformGizmo);
            gizmo.transform.SetPositionAndRotation(selectedRig.transform.position, selectedRig.transform.rotation);
        }
    }

    // Finds the middle point of the selected objects by finding the min and max values for X, Y and Z
    // coordinates and returning the middle of each.
    private Vector3 GizmoPosition()
    {
        float minX, maxX, minY, maxY, minZ, maxZ;
        minX = maxX = selectedCams[0].transform.position.x;
        minY = maxY = selectedCams[0].transform.position.y;
        minZ = maxZ = selectedCams[0].transform.position.z;
        for (int i = 0; i < selectedCams.Count; ++i)
        {
            if (minX > selectedCams[i].transform.position.x)
            {
                minX = selectedCams[i].transform.position.x;
            }
            if (maxX < selectedCams[i].transform.position.x)
            {
                maxX = selectedCams[i].transform.position.x;
            }
            if (minY > selectedCams[i].transform.position.y)
            {
                minY = selectedCams[i].transform.position.y;
            }
            if (maxY < selectedCams[i].transform.position.y)
            {
                maxY = selectedCams[i].transform.position.y;
            }
            if (minZ > selectedCams[i].transform.position.z)
            {
                minZ = selectedCams[i].transform.position.z;
            }
            if (maxZ < selectedCams[i].transform.position.z)
            {
                maxZ = selectedCams[i].transform.position.z;
            }
        }
        return new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
    }

    // Changes the camera's highlight state to the other.
    public void Highlight()
    {
        Renderer cameraShape = gameObject.transform.Find("Model").Find("Camera_Shape").gameObject.GetComponent<Renderer>();
        Renderer lensShape = gameObject.transform.Find("Model").Find("Lens_Shape").gameObject.GetComponent<Renderer>();
        if (!highlighted)
        {
            cameraShape.material = camHighlightMaterial;
            lensShape.material = camHighlightMaterial;
            highlighted = true;
        }
        else
        {
            cameraShape.material = camMaterial;
            lensShape.material = camMaterial;
            highlighted = false;
        }
    }

    // Responsible for moving the selected camera(s) and gizmo on mousedrag.
    private IEnumerator MoveCam(string axis)
    {
        float xSpeed, ySpeed;

        // X and Y axis directions on the plane of the screen in world coordinates
        Vector3 viewX = gameObject.transform.right;
        Vector3 viewY = gameObject.transform.forward;

        // The direction the arrow is pointing to in "screen coordinates"
        Vector2 arrowDirection;

        while (Input.GetMouseButton(0))
        {
            xSpeed = dragSpeed * Input.GetAxis("Mouse X");
            ySpeed = dragSpeed * Input.GetAxis("Mouse Y");
            Vector3 translateVector = Vector3.zero;
            if (axis == "Y")
            {
                translateVector = new Vector3(0, ySpeed, 0);
            }
            else if (axis == "X")
            {
                Vector3 translateDirection = gizmo.transform.right;
                translateDirection.y = 0;
                arrowDirection = new Vector2(Vector3.Dot(translateDirection, viewX),
                                                     Vector3.Dot(translateDirection, viewY));
                float translateSpeed = xSpeed * arrowDirection.x + ySpeed * arrowDirection.y;
                translateVector = translateDirection * translateSpeed;
            }
            else if (axis == "Z")
            {
                Vector3 translateDirection = gizmo.transform.forward;
                translateDirection.y = 0;
                arrowDirection = new Vector2(Vector3.Dot(translateDirection, viewX),
                                                     Vector3.Dot(translateDirection, viewY));
                float translateSpeed = xSpeed * arrowDirection.x + ySpeed * arrowDirection.y;
                translateVector = translateDirection * translateSpeed;
            }
            gizmo.transform.Translate(translateVector, Space.World);
            if (selectedRig != null) 
            {
                selectedRig.transform.Translate(translateVector, Space.World);
            }
            else
            {
                foreach (GameObject cam in selectedCams) {
                    cam.transform.Translate(translateVector, Space.World);
                }
            }
            yield return null;
        }
        yield break;
    }

    #endregion
}
