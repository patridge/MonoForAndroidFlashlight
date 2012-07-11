namespace MonoAndroidFlashlight {
    using System.Collections.Generic;
    using Android.App;
    using Android.Hardware;
    using Android.OS;
    using Android.Views;
    using Android.Widget;

    // NOTE: If you are grabbing this code, make sure you also copy the camera permission from /Properties/AndroidManifest.xml.
    //       <uses-permission android:name="android.permission.CAMERA" />
    [Activity(Label = "Flashlight", MainLauncher = true, Icon = "@drawable/icon")]
    public class FlashlightActivity : Activity, ISurfaceHolderCallback {
        protected Camera _Camera = null;
        protected IList<string> _CameraSupportedFlashModes = null;
        protected SurfaceView _SurfaceView = null;
        protected ISurfaceHolder _SurfaceHolder = null;
        protected const string FlashlightOnMode = Camera.Parameters.FlashModeTorch;
        protected const string FlashlightOffMode = Camera.Parameters.FlashModeOff;
        private bool IsPreviewing = false;

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            StartCamera();
            _SurfaceView = FindViewById<SurfaceView>(Resource.Id.SurfaceView);
            _SurfaceHolder = _SurfaceView.Holder;
            _SurfaceHolder.AddCallback(this);
            _SurfaceHolder.SetType(SurfaceType.PushBuffers);

            ImageView flashlightToggleButton = FindViewById<ImageView>(Resource.Id.FlashTorch);
            flashlightToggleButton.Click += (sender, args) => {
                if (FlashlightOnMode != GetCameraFlashMode()) {
                    TurnFlashOn();
                }
                else {
                    TurnFlashOff();
                }
            };
        }
        protected override void OnStop() {
            StopCamera();
            base.OnStop();
        }
        protected override void OnStart() {
            base.OnStart();
            StartCamera();
        }
        protected override void OnResume() {
            base.OnResume();
            TurnFlashOn();
        }
        protected override void OnPause() {
            base.OnPause();
            TurnFlashOff();
        }
        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height) { }
        public void SurfaceCreated(ISurfaceHolder holder) {
            _Camera.SetPreviewDisplay(holder);
        }
        public void SurfaceDestroyed(ISurfaceHolder holder) { }

        protected void StartCamera() {
            if (_Camera == null) {
                _Camera = Camera.Open();
                _CameraSupportedFlashModes = _CameraSupportedFlashModes ?? _Camera.GetParameters().SupportedFlashModes;
                if (_CameraSupportedFlashModes == null || !_CameraSupportedFlashModes.Contains(FlashlightOnMode) || !_CameraSupportedFlashModes.Contains(FlashlightOffMode)) {
                    StopCamera();
                }
            }
        }
        protected void StartPreviewing() {
            if (_Camera == null) {
                StartCamera();
            }
            if (_Camera != null && !IsPreviewing) {
                _Camera.StartPreview();
                IsPreviewing = true;
            }
        }
        protected void StopPreviewing() {
            if (_Camera != null && IsPreviewing) {
                _Camera.StopPreview();
            }
            IsPreviewing = false;
        }
        protected void StopCamera() {
            StopPreviewing();
            if (_Camera != null) {
                _Camera.Release();
                _Camera = null;
            }
        }
        protected string GetCameraFlashMode(Camera.Parameters cameraParameters = null) {
            string mode = null;
            if (_Camera != null) {
                if (cameraParameters == null) {
                    cameraParameters = _Camera.GetParameters();
                }
                mode = cameraParameters.FlashMode;
            }
            return mode;
        }
        protected void SetCameraFlashMode(string newMode) {
            if (_Camera != null) {
                Camera.Parameters cameraParameters = _Camera.GetParameters();
                if (newMode != GetCameraFlashMode(cameraParameters)) {
                    cameraParameters.FlashMode = newMode;
                    _Camera.SetParameters(cameraParameters);
                }
            }
        }
        protected void TurnFlashOn() {
            StartPreviewing();
            SetCameraFlashMode(FlashlightOnMode);
        }
        protected void TurnFlashOff() {
            StopPreviewing();
            SetCameraFlashMode(FlashlightOffMode);
        }
    }
}