using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using FFmpegOut;

public class ControlPanel : MonoBehaviour {
    #region Editable properties

    [SerializeField] PostProcessProfile _profile;

    #endregion  // Editable properties
    #region Private properties

    private int rigInd = -1;
    private int rigCamInd = -1;
    private float recTime = 0;
    private List<GameObject> rigs = new List<GameObject>();

    private int settingsInd = 0;
    private List<Canvas> settingsCanvases = new List<Canvas>();
    private Dropdown allRigsSelect;
    private Dropdown rigCamerasSelect;
    private InputField rigCamsInput, rigSizeInput;
    private InputField rigPosX, rigPosY, rigPosZ;
    private InputField rigRotX, rigRotY, rigRotZ;
    private InputField camPosX, camPosY, camPosZ;
    private InputField camRotX, camRotY, camRotZ;
    private InputField fovInput, focInput;
    private InputField recTimeInput;
    private InputField resWidthInput, resHeightInput;
    private Slider grainIntensity, grainSize, grainLuminance;
    private Slider motionBlurShutter, motionBlurSample;
    private Slider rainAlpha, rainSpeed, rainClipping;
    private Slider rainBlur, rainDistortion, rainDistortionStr;
    private Slider grimeBlend;

    #endregion  // Private properties
    #region MonoBehavior functions

    /** \brief Find references to each required UI component and initialize the fields.
     */
    void Start() {
        // Force a locale where a period is used as decimal separator instead of a comma.
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;

        settingsCanvases.Add(transform.Find("CameraSettings").GetComponent<Canvas>());
        settingsCanvases.Add(transform.Find("GrainSettings").GetComponent<Canvas>());
        settingsCanvases.Add(transform.Find("MotionBlurSettings").GetComponent<Canvas>());
        settingsCanvases.Add(transform.Find("RainSettings").GetComponent<Canvas>());
        settingsCanvases.Add(transform.Find("GrimeSettings").GetComponent<Canvas>());
        for (int i = 1; i < settingsCanvases.Count; ++i) {
            settingsCanvases[i].enabled = false;
        }

        // Obtain references to UI components.
        allRigsSelect = transform.Find("CameraSettings/RigDropdown").GetComponent<Dropdown>();
        rigCamerasSelect = transform.Find("CameraSettings/CameraDropdown").GetComponent<Dropdown>();
        rigCamsInput = transform.Find("CameraSettings/RigSizeInput").GetComponent<InputField>();
        rigSizeInput = transform.Find("CameraSettings/RigRadiusInput").GetComponent<InputField>();
        rigPosX = transform.Find("CameraSettings/RigPositionX").GetComponent<InputField>();
        rigPosY = transform.Find("CameraSettings/RigPositionY").GetComponent<InputField>();
        rigPosZ = transform.Find("CameraSettings/RigPositionZ").GetComponent<InputField>();
        rigRotX = transform.Find("CameraSettings/RigRotationX").GetComponent<InputField>();
        rigRotY = transform.Find("CameraSettings/RigRotationY").GetComponent<InputField>();
        rigRotZ = transform.Find("CameraSettings/RigRotationZ").GetComponent<InputField>();
        camPosX = transform.Find("CameraSettings/CameraPositionX").GetComponent<InputField>();
        camPosY = transform.Find("CameraSettings/CameraPositionY").GetComponent<InputField>();
        camPosZ = transform.Find("CameraSettings/CameraPositionZ").GetComponent<InputField>();
        camRotX = transform.Find("CameraSettings/CameraRotationX").GetComponent<InputField>();
        camRotY = transform.Find("CameraSettings/CameraRotationY").GetComponent<InputField>();
        camRotZ = transform.Find("CameraSettings/CameraRotationZ").GetComponent<InputField>();
        fovInput = transform.Find("CameraSettings/CameraFovInput").GetComponent<InputField>();
        focInput = transform.Find("CameraSettings/CameraFoclenInput").GetComponent<InputField>();
        recTimeInput = transform.Find("CameraSettings/RecordTimeInput").GetComponent<InputField>();
        resWidthInput = transform.Find("CameraSettings/ResolutionWidthInput").GetComponent<InputField>();
        resHeightInput = transform.Find("CameraSettings/ResolutionHeightInput").GetComponent<InputField>();
        grainIntensity = transform.Find("GrainSettings/GrainIntensitySlider").GetComponent<Slider>();
        grainSize = transform.Find("GrainSettings/GrainSizeSlider").GetComponent<Slider>();
        grainLuminance = transform.Find("GrainSettings/GrainLuminanceSlider").GetComponent<Slider>();
        motionBlurShutter = transform.Find("MotionBlurSettings/MotionBlurShutterSlider").GetComponent<Slider>();
        motionBlurSample = transform.Find("MotionBlurSettings/MotionBlurSampleSlider").GetComponent<Slider>();
        rainAlpha = transform.Find("RainSettings/RainAlphaSlider").GetComponent<Slider>();
        rainSpeed = transform.Find("RainSettings/RainSpeedSlider").GetComponent<Slider>();
        rainClipping = transform.Find("RainSettings/RainClippingSlider").GetComponent<Slider>();
        rainDistortion = transform.Find("RainSettings/RainDistortionSlider").GetComponent<Slider>();
        rainDistortionStr = transform.Find("RainSettings/RainDistortionStrSlider").GetComponent<Slider>();
        rainBlur = transform.Find("RainSettings/RainBlurSlider").GetComponent<Slider>();
        grimeBlend = transform.Find("GrimeSettings/GrimeBlendSlider").GetComponent<Slider>();

        // Clear camera and rig names added in the editor.
        allRigsSelect.ClearOptions();
        rigCamerasSelect.ClearOptions();

        // Initialize InputFields with some values.
        rigCamsInput.text = "6";
        rigSizeInput.text = "0.15";
        WriteToFields(rigPosX, rigPosY, rigPosZ, Vector3.zero);
        WriteToFields(rigRotX, rigRotY, rigRotZ, Vector3.zero);
        WriteToFields(camPosX, camPosY, camPosZ, Vector3.zero);
        WriteToFields(camRotX, camRotY, camRotZ, Vector3.zero);
        WriteToFields(fovInput, focInput, recTimeInput, Vector3.zero);
        resWidthInput.text = resHeightInput.text = "1000";

        // Make a rig under which free cameras will be put.
        var individuals = new GameObject("Cameras not in rigs");
        rigInd = rigs.Count;
        rigs.Add(individuals);

        allRigsSelect.AddOptions(new List<string> { individuals.name });
        allRigsSelect.value = rigInd;

        // Get the main camera and add it to the list of cameras.
        var cam = SaveCamera(GameObject.Find("Main Camera").GetComponentInParent<Flycam>());

        // The sliders get their values set only once. After this they're only read.
        grainIntensity.value = _profile.GetSetting<Grain>().intensity;
        grainSize.value = _profile.GetSetting<Grain>().size;
        grainLuminance.value = _profile.GetSetting<Grain>().lumContrib;
        motionBlurShutter.value = _profile.GetSetting<MotionBlur>().shutterAngle;
        motionBlurSample.value = _profile.GetSetting<MotionBlur>().sampleCount;

        rainAlpha.value = 30;
        rainSpeed.value = 5;
        rainClipping.value = 0.2f;
        rainDistortion.value = 8;
        rainDistortionStr.value = 1;
        rainBlur.value = 1.5f;
        grimeBlend.value = 0.1f;
    }

    /** \brief Manage cameras' values and recordings.
     */
    void Update() {
        HandleInput();
        RaycastDropdownScrolls();
        UpdateValues();
        CheckRecordingLength();
    }

    #endregion  // MonoBehavior functions
    #region Public methods

    /** \brief Switch the currently shown settings.
     */
    public void SettingsSelected(Dropdown changed) {
        settingsCanvases[settingsInd].enabled = false;
        settingsInd = changed.value;
        settingsCanvases[settingsInd].enabled = true;
    }

    /** \brief Activate a rig and display its cameras.
     */
    public void RigSelected() {
        // Disable the current camera.
        EnableCurrentCamera(false);

        // Activate the new rig.
        rigInd = allRigsSelect.value;

        // Update the camera dropdown to show the current rig's cameras.
        var names = new List<Dropdown.OptionData>();
        foreach (var child in GetCurrentRig().transform.GetComponentsInChildren<Flycam>()) {
            names.Add(new Dropdown.OptionData(child.name));
        }

        rigCamerasSelect.options = names;
        rigCamerasSelect.value = rigCamInd = 0;
        CameraSelected();
    }

    /** \brief Activate a camera and display its positional data.
     */
    public void CameraSelected() {
        // Disable the current camera.
        EnableCurrentCamera(false);

        // Update all dropdowns to show the current camera.
        rigCamInd = rigCamerasSelect.value;

        // Enable the new current camera.
        EnableCurrentCamera(true);

        // Update the fields to show the new current camera's attributes.
        DisplayValues();
    }

    /** \brief Clone the current camera and place it in the current rig.
     */
    public void CreateCamera() {
        // Only one camera shall be enabled at a time (unless they're recording).
        var oldCam = GetCurrentCamera();
        EnableCamera(oldCam, false);
        
        // Save the camera into the current rig.
        var newCam = Instantiate(oldCam);
        SaveCamera(newCam);
    }

    /** \brief Delete the current camera, unless it's the main camera.
     */
    public void DeleteCamera() {
        // The Main Camera can't be deleted.
        if (rigInd == 0 && rigCamInd == 0) {
            return;
        }

        // "Fix" all cameras' names to reflect their new indices.
        var camNames = rigCamerasSelect.options;
        for (int i = camNames.Count - 1; i > rigCamInd; --i) {
            camNames[i].text = GetCamera(rigInd, i).name = GetCamera(rigInd, i - 1).name;
        }
        camNames.RemoveAt(rigCamInd);

        // Disable and delete the current camera.
        var cam = GetCurrentCamera();
        cam.transform.parent = null;
        EnableCamera(cam, false);
        Destroy(cam.gameObject);

        // Check that the index is still in range.
        var childCount = GetCurrentRig().transform.childCount;
        if (rigCamInd >= childCount) {
            rigCamInd = childCount - 1;
        }

        // Delete the rig if the last camera was deleted.
        if (rigCamInd < 0) {
            DeleteRig();
            // DeleteRig updates the rigs and cameras, so let's return here
            // to avoid redoing it down below.
            return;
        }

        // Update the list of renamed cameras and set the new index.
        rigCamerasSelect.options = camNames;
        rigCamerasSelect.value = rigCamInd;
        CameraSelected();
    }

    /** \brief Create a rig of cameras around current camera's position.
     * 
     * Cameras are distributed exactly evenly around the rig's local y-axis.
     */
    public void CreateRigIdeal() {
        // If a comma is used as a decimal separator the value is not the same as if a period was used.
        var radius = rigSizeInput.text.Replace(',', '.');
        CreateRig(int.Parse(rigCamsInput.text), float.Parse(radius), true);
    }

    /** \brief Create a rig of cameras around current camera's position.
     * 
     * Cameras aren't distributed exactly evenly around the rig's local y-axis, but there
     * are slight deviations from the expected angles.
     */
    public void CreateRigRough() {
        // If a comma is used as a decimal separator the value is not the same as if a period was used.
        var radius = rigSizeInput.text.Replace(',', '.');
        CreateRig(int.Parse(rigCamsInput.text), float.Parse(radius), false);
    }

    /** \brief Delete the current rig, unless it contains the free cameras.
     */
    public void DeleteRig() {
        // The rig of "individual cameras" can't be deleted.
        if (rigInd == 0) {
            return;
        }

        // "Fix" all rigs' names to reflect their new indices.
        var rigNames = allRigsSelect.options;
        for (int newInd = rigNames.Count - 1; newInd > rigInd; --newInd) {
            var oldInd = newInd - 1;
            var newerRig = GetRig(newInd);
            var olderRig = GetRig(oldInd);

            // Every rig after the deleted one shall be named like the rig before.
            rigNames[newInd].text = newerRig.name = olderRig.name;

            // The child cameras of each rig need to be renamed with the new index as well.
            foreach (var c in newerRig.transform.GetComponentsInChildren<Flycam>()) {
                c.name = "Camera" + oldInd + c.name.Substring(c.name.IndexOf('-'));
            }
        }
        rigNames.RemoveAt(rigInd);

        // Delete the rig.
        Destroy(GetCurrentRig());
        rigs.RemoveAt(rigInd);

        // Check that the index is still in range.
        if (rigInd >= rigs.Count) {
            rigInd = rigs.Count - 1;
        }

        // Update the list of renamed rigs and set the new index.
        allRigsSelect.options = rigNames;
        allRigsSelect.value = rigInd;
        RigSelected();
    }

    /** \brief Enable the current rig and start recording on all its cameras.
     */
    public void StartRecording() {
        if (recTime <= 0) {
            recTime = float.Parse(recTimeInput.text);
            if (recTime > 0) {
                foreach (var rig in rigs) {
                    foreach (var cam in rig.transform.GetComponentsInChildren<Flycam>()) {
                        EnableCamera(cam, true);
                        cam.GetComponentInParent<CameraCapture>().OpenPipe();
                    }
                }
            }
        }
    }

    /** \brief Stop recording with the current rig and disable all but the current camera.
     */
    public void StopRecording() {
        recTime = 0;
        foreach (var rig in rigs) {
            foreach (var cam in rig.transform.GetComponentsInChildren<Flycam>()) {
                cam.GetComponentInParent<CameraCapture>().ClosePipe();
                EnableCamera(cam, false);
            }
        }

        EnableCurrentCamera(true);
    }

    #endregion  // Public methods
    #region Private methods

    /** \brief Check button presses etc. and do things.
     * 
     * ESC: Exit the application.
     *   O: Toggle cursorlock.
     */
    void HandleInput() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();

        } else if (Input.GetKeyDown(KeyCode.O)) {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.None) ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    /** \brief Use the scroll wheel to switch the selected value of a dropdown when hovering over one.
     */
    void RaycastDropdownScrolls() {
        // List all UI components currently under the cursor.
        var mousePos = new PointerEventData(null);
        mousePos.position = Input.mousePosition;
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(mousePos, results);

        // Dropdowns aren't overlapping, so only one can be selected at once.
        foreach (var rcRes in results) {
            var drop = rcRes.gameObject.GetComponent<Dropdown>();
            if (drop != null) {
                var scrollDelta = Input.GetAxis("Mouse ScrollWheel");
                if (scrollDelta < 0f && drop.value + 1 < drop.options.Count) {
                    ++drop.value;
                    // The rig and/or cameras get activated in RigSelected and/or
                    // CameraSelected, which get called automatically by Unity
                    // when allRigsSelect's or rigCamerasSelect's value changes.

                } else if (scrollDelta > 0f && drop.value > 0) {
                    --drop.value;
                }
                break;
            }
        }
    }

    /** \brief Handle the updating of values from cameras to UI or vice versa.
     */
    void UpdateValues() {
        if (GetCurrentRig().transform.parent != null) {
            // When the rig is linked to a car, update the ui with the new values.
            DisplayValues();

        } else  if (Input.GetMouseButton(0)) {
            // When dragging the camera, update the ui with the new values.
            // Dragging is handled by MouseControl.
            DisplayValues();

        } else if (MouseControl.CursorLocked()) {
            // When moving the camera by mouse, update the ui with the new values.
            GetCurrentCamera().UpdateRotation();
            GetCurrentCamera().UpdatePosition();
            DisplayValues();

        } else {
            // When writing the values by hand, apply those values to the camera.
            ApplyValues();
        }
    }

    /** \brief Update the current rig and camera's values to the UI.
     */
    void DisplayValues() {
        var activeRig = GetCurrentRig();
        var rigPos = activeRig.transform.position;
        var rigRot = activeRig.transform.eulerAngles;
        for (int i = 0; i < 3; ++i) {
            if (rigRot[i] > 180) {
                rigRot[i] -= 360;
            }
        }
        WriteToFields(rigPosX, rigPosY, rigPosZ, rigPos);
        WriteToFields(rigRotX, rigRotY, rigRotZ, rigRot);

        var activeCam = GetCurrentCamera();
        var camPos = activeCam.transform.localPosition;
        var camRot = activeCam.transform.localRotation.eulerAngles;
        for (int i = 0; i < 3; ++i) {
            if (camRot[i] > 180) {
                camRot[i] -= 360;
            }
        }
        WriteToFields(camPosX, camPosY, camPosZ, camPos);
        WriteToFields(camRotX, camRotY, camRotZ, camRot);

        fovInput.text = activeCam.GetComponent<Camera>().fieldOfView.ToString("0.000");
        focInput.text = activeCam.GetComponent<Camera>().focalLength.ToString("0.000");

        resWidthInput.text = activeCam.GetComponent<CameraCapture>()._width.ToString("0");
        resHeightInput.text = activeCam.GetComponent<CameraCapture>()._height.ToString("0");
    }

    /** \brief Apply the values shown in the UI to the current rig and camera.
     */
    void ApplyValues() {
        var activeRig = GetCurrentRig();
        activeRig.transform.position = ReadFromFields(rigPosX, rigPosY, rigPosZ);
        activeRig.transform.eulerAngles = ReadFromFields(rigRotX, rigRotY, rigRotZ);

        var activeCam = GetCurrentCamera();
        activeCam.transform.localPosition = ReadFromFields(camPosX, camPosY, camPosZ);
        activeCam.transform.localRotation = Quaternion.Euler(ReadFromFields(camRotX, camRotY, camRotZ));

        var cam = activeCam.GetComponent<Camera>();
        cam.fieldOfView = Mathf.Clamp(float.Parse(fovInput.text), 1e-3f, 180 - 1e-3f);
        fovInput.text = cam.fieldOfView.ToString("0.000");
        focInput.text = cam.focalLength.ToString("0.000");

        activeCam.GetComponent<CameraCapture>()._width = int.Parse(resWidthInput.text);
        activeCam.GetComponent<CameraCapture>()._height = int.Parse(resHeightInput.text);

        _profile.GetSetting<Grain>().intensity.value = grainIntensity.value;
        _profile.GetSetting<Grain>().size.value = grainSize.value;
        _profile.GetSetting<Grain>().lumContrib.value = grainLuminance.value;
        _profile.GetSetting<MotionBlur>().shutterAngle.value = motionBlurShutter.value;
        _profile.GetSetting<MotionBlur>().sampleCount.value = (int) motionBlurSample.value;

        var rainSettings = cam.GetComponentInChildren<CameraLensRainDropImageEffectScript>();
        rainSettings.alpha = rainAlpha.value;
        rainSettings.rainSpeed = rainSpeed.value;
        rainSettings.clipping = rainClipping.value;
        rainSettings.distortion = rainDistortion.value;
        rainSettings.distortionStrength = rainDistortionStr.value;
        rainSettings.blurAmount = rainBlur.value;

        cam.GetComponentInChildren<LensGrimeDirtImageEffectScript>().blendingFactor = grimeBlend.value;
    }

    /** \brief Insert a component from a vector into a InputField.
     * 
     * \param x The first InputField to be inserted into.
     * \param y The second InputField to be inserted into.
     * \param z The third InputField to be inserted into.
     * \param vec The vector whose components shall be written into the InputFields.
     */
    void WriteToFields(InputField x, InputField y, InputField z, Vector3 vec) {
        x.text = vec.x.ToString("0.000");
        y.text = vec.y.ToString("0.000");
        z.text = vec.z.ToString("0.000");
    }

    /** \brief Read components from InputFields into a vector.
     * 
     * \param x The first InputField to be read from.
     * \param y The second InputField to be read from.
     * \param z The third InputField to be read from.
     * \return The vector whose components were read from the InputFields.
     */
    Vector3 ReadFromFields(InputField x, InputField y, InputField z) {
        return new Vector3 {
            x = float.Parse(x.text),
            y = float.Parse(y.text),
            z = float.Parse(z.text)
        };
    }

    /** \brief Check if the recording is long enough and stop.
     */
    void CheckRecordingLength() {
        if (recTime > 0) {
            recTime -= Time.deltaTime;
            if (recTime <= 0) {
                StopRecording();
            }
            recTimeInput.text = recTime.ToString("0.000");
        }
    }

    /** \brief Setup a new camera and add it to a rig.
     * 
     * The camera will be named, added to a rig and enabled.
     * The dropdown and UI input fields will also be updated.
     * 
     * \param cam The camera to be handled.
     * \return The camera that was handled.
     */
    Flycam SaveCamera(Flycam cam) {
        // Update the current camera's index to the newest camera.
        var rig = GetCurrentRig();
        rigCamInd = rig.transform.childCount;

        // Name the new camera, put it in a rig and enable it.
        cam.name = "Camera" + rigInd + "-" + rigCamInd;
        cam.transform.parent = rig.transform;

        // Add the new camera to the dropdown of the rig's cameras and select it.
        rigCamerasSelect.AddOptions(new List<string> { cam.name });
        rigCamerasSelect.value = rigCamInd;

        EnableCurrentCamera(true);
        DisplayValues();

        return cam;
    }

    /** \brief Get a camera rig.
     * 
     * \param rigi Index of the camera rig.
     * \return The camera rig or null.
     */
    GameObject GetRig(int rigi) {
        if (rigi < 0 || rigi >= rigs.Count) {
            print("Requested a rig(" + rigi + ") with " + rigs.Count + " rigs in existence!");
            return null;
        }
        return rigs[rigi];
    }

    /** \brief Get the current camera rig.
     * 
     * \return The current camera rig or null.
     */
    GameObject GetCurrentRig() {
        return GetRig(rigInd);
    }

    /** \brief Get a camera.
     * 
     * \param rigi Index of the camera rig.
     * \param cami Index of the camera in the rig.
     * \return The camera or null.
     */
    Flycam GetCamera(int rigi, int cami) {
        var rigTrans = GetRig(rigi)?.transform;
        if (rigTrans == null) {
            return null;
        }

        if (cami < 0 || cami >= rigTrans.childCount) {
            print("Requested a camera(" + cami + ") with " + rigTrans.childCount + " cameras in the given rig(" + rigi + ")!");
            return null;
        }

        return rigTrans.GetChild(cami).GetComponent<Flycam>();
    }

    /** \brief Get the current camera.
     * 
     * \return The current camera or null.
     */
    Flycam GetCurrentCamera() {
        return GetCamera(rigInd, rigCamInd);
    }

    /** \brief Enable a camera to capture video and make its model invisible.
     * 
     * One camera must be disabled and another must be enabled to 
     * switch the source of the image being shown on the screen.
     * 
     * A camera must be made invisible when it captures 
     * video to avoid the model getting in the view.
     * 
     * \param cam The camera to be modified.
     * \param enabled `true` to enable the camera and hide the model.
     */
    void EnableCamera(Flycam cam, bool enabled) {
        if (cam == null) {
            return;
        }

        cam.GetComponent<Camera>().enabled = enabled;

        // A camera is made invisible by disabling the Renderers of its children.
        foreach (var r in cam.GetComponentsInChildren<Renderer>()) {
            r.enabled = !enabled;
        }
    }

    /** \brief Enable the current camera to record video and make its model invisible.
     * 
     * \param enabled `true` to enable the camera and hide the model.
     */
    void EnableCurrentCamera(bool enabled) {
        EnableCamera(GetCurrentCamera(), enabled);
    }

    /** \brief Create a rig of cameras around the current camera's position.
     * 
     * \param cams The number of cameras in the rig.
     * \param radius Rig's cameras' distance from the current camera.
     * \param ideal `true` to distribute cameras evenly.
     */
    void CreateRig(int cams, float radius, bool ideal) {
        EnableCurrentCamera(false);
        var oldCam = GetCurrentCamera();
        var currentTrans = oldCam.transform;

        // Create the rig under which the cameras will be added.
        // (Don't use the real rigInd, so SelectCamera does what it's supposed to...)
        var rigInd = rigs.Count;
        GameObject rig = new GameObject("Rig" + rigInd);
        rig.transform.SetPositionAndRotation(currentTrans.position, currentTrans.rotation);
        rigs.Add(rig);

        var rotDiffDeg = 360f / cams;
        for (int i = 0; i < cams; ++i) {
            // Create a new camera, name it and add it to the rig.
            var newCam = Instantiate(oldCam);
            newCam.name = "Camera" + rigInd + "-" + i;
            newCam.transform.parent = rig.transform;

            // Add a deviation of [-0.1, 0.1] * expected angle between cameras if rig's not ideal.
            float rotDev = (ideal ? 0 : 0.1f * (2 * UnityEngine.Random.value - 1));
            var rotDeg = (i + rotDev) * rotDiffDeg;
            var rotRad = rotDeg * Mathf.Deg2Rad;

            newCam.transform.localRotation = Quaternion.Euler(0, rotDeg, 0);
            newCam.transform.localPosition = new Vector3(Mathf.Sin(rotRad) * radius, 0, Mathf.Cos(rotRad) * radius);
        }

        allRigsSelect.AddOptions(new List<string> { rig.name });
        allRigsSelect.value = rigInd;

        EnableCurrentCamera(true);
        DisplayValues();
    }

    #endregion  // Private methods
}
