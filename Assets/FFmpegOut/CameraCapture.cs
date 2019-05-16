using System;
using System.IO;
using UnityEngine;

namespace FFmpegOut {
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("FFmpegOut/Camera Capture")]
    public class CameraCapture : MonoBehaviour {
        #region Editable properties

        [SerializeField] bool _setResolution = true;
        [SerializeField] public int _width = 1920;
        [SerializeField] public int _height = 1080;
        [SerializeField] int _frameRate = 30;
        [SerializeField] bool _allowSlowDown = true;
        [SerializeField] FFmpegPipe.Preset _preset;

        #endregion

        #region Private members

        [SerializeField, HideInInspector] Shader _shader;
        Material _material;

        FFmpegPipe _pipe;

        RenderTexture _tempTarget;
        GameObject _tempBlitter;

        static int _activePipeCount;

        #endregion

        #region MonoBehavior functions

        void OnValidate() {

        }

        void OnEnable() {
            if (!FFmpegConfig.CheckAvailable) {
                Debug.LogError(
                    "ffmpeg.exe is missing. " +
                    "Please refer to the installation instruction. " +
                    "https://github.com/keijiro/FFmpegOut"
                );
                enabled = false;
            }
        }

        void OnDisable() {
            if (_pipe != null) ClosePipe();
        }

        void OnDestroy() {
            if (_pipe != null) ClosePipe();
        }

        void Start() {
            _material = new Material(_shader);
        }

        void Update() {

        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_pipe != null) {
                var tempRT = RenderTexture.GetTemporary(source.width, source.height);
                Graphics.Blit(source, tempRT, _material, 0);

                var tempTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                tempTex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
                tempTex.Apply();

                // Write camera position and rotation to a csv.
                GetComponentInParent<Flycam>().WriteToFile();

                _pipe.Write(tempTex.GetRawTextureData());

                Destroy(tempTex);
                RenderTexture.ReleaseTemporary(tempRT);
            }

            Graphics.Blit(source, destination);
        }

        #endregion

        #region Public methods

        public void OpenPipe() {
            if (_pipe != null) return;

            var camera = GetComponent<Camera>();
            var width = _width;
            var height = _height;

            // Apply the screen resolution settings.
            if (_setResolution) {
                _tempTarget = RenderTexture.GetTemporary(width, height, 24);
                camera.targetTexture = _tempTarget;
                _tempBlitter = Blitter.CreateGameObject(camera);
            } else {
                width = camera.pixelWidth;
                height = camera.pixelHeight;
            }

            // Create the directory where the videos should be put.
            if (!Directory.Exists("videos")) {
                Directory.CreateDirectory("videos");
            }

            // Set the name of the file where posdata should be written.
            if (_activePipeCount == 0) {
                var dateTime = DateTime.Now.ToString("yyyy_MMdd_HHmmss");
                var fileName = "videos/" + dateTime + "_posdata.csv";
                Flycam.SetFileName(fileName);
            }

            // Open an output stream.
            _pipe = new FFmpegPipe("videos/", name, width, height, _frameRate, _preset);
            _activePipeCount++;

            // Change the application frame rate on the first pipe.
            if (_activePipeCount == 1) {
                if (_allowSlowDown)
                    Time.captureFramerate = _frameRate;
                else
                    Application.targetFrameRate = _frameRate;
            }

            Debug.Log("Capture started (" + _pipe.Filename + ")");
        }

        public void ClosePipe() {
            var camera = GetComponent<Camera>();

            // Destroy the blitter object.
            if (_tempBlitter != null) {
                Destroy(_tempBlitter);
                _tempBlitter = null;
            }

            // Release the temporary render target.
            if (_tempTarget != null && _tempTarget == camera.targetTexture) {
                camera.targetTexture = null;
                RenderTexture.ReleaseTemporary(_tempTarget);
                _tempTarget = null;
            }

            // Close the output stream.
            if (_pipe != null) {
                Debug.Log("Capture ended (" + _pipe.Filename + ")");

                _pipe.Close();
                _activePipeCount--;

                // Clear the name of the file where posdata should be written.
                if (_activePipeCount == 0) {
                    Flycam.SetFileName(null);
                }

                if (!string.IsNullOrEmpty(_pipe.Error)) {
                    Debug.LogWarning(
                        "ffmpeg returned with a warning or an error message. " +
                        "See the following lines for details:\n" + _pipe.Error
                    );
                }

                _pipe = null;

                // Reset the application frame rate on the last pipe.
                if (_activePipeCount == 0) {
                    if (_allowSlowDown)
                        Time.captureFramerate = 0;
                    else
                        Application.targetFrameRate = -1;
                }
            }
        }

        #endregion
    }
}
