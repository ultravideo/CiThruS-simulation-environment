using UnityEngine;

public class Rigfit : MonoBehaviour {
    #region MonoBehavior functions

    /** \brief Check if L has been pressed and link the chosen camera rig to the chosen vehicle's model.
     */
    void Update() {
        if (Input.GetKeyDown(KeyCode.L)) {
            var rig = MouseControl.GetSelectedRig();
            var vehicle = MouseControl.GetSelectedCar();
            if (rig != null && vehicle == gameObject) {
                FitRig(rig);
            }
        }
    }

    #endregion  // MonoBehavior functions
    #region Private methods

    /** \brief Link a camera rig onto a vehicle.
     * 
     * \param rig The rig to be fitted onto this vehicle.
     */
    private void FitRig(GameObject rig) {
        rig.transform.parent = null;

        // Move all cameras in the rig inside the vehicle and start fitting.
        var pos = transform.position;
        pos.y = rig.transform.position.y;
        rig.transform.SetPositionAndRotation(pos, transform.rotation);
        foreach (var cam in rig.transform.GetComponentsInChildren<Flycam>()) {
            cam.transform.localPosition = Vector3.zero;
            FitCamera(cam);
        }

        rig.transform.parent = gameObject.transform;
    }

    private void FitCamera(Flycam camera) {
        // Camera should now be inside the vehicle it's getting fitted onto.
        // Move it forward until it emerges from the vehicle.
        var forward = camera.transform.forward;
        RaycastHit hit = new RaycastHit();
        int safety = 0;
        while (hit.collider?.gameObject != gameObject) {
            camera.transform.Translate(forward, Space.World);
            Physics.Raycast(camera.transform.position, -forward, out hit);
            if (++safety == 100) {
                print("Failed to link the rig!");
                return;
            }
        }

        // Move the camera backward until it touches the vehicle.
        camera.transform.Translate(-hit.distance * forward, Space.World);
    }

    #endregion  // Private methods
}
