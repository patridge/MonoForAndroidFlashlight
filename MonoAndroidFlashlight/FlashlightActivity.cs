namespace MonoAndroidFlashlight {
	using System.Collections.Generic;
	using Android.App;
	using Android.Hardware;
	using Android.OS;
	using Android.Views;
	using Android.Widget;

	// NOTE: If you are grabbing this code, make sure you also copy the camera permission from /Properties/AndroidManifest.xml.
	//       <uses-permission android:name="android.permission.CAMERA" />
	[Activity(
		Label = "Flashlight",
		MainLauncher = true,
		Icon = "@drawable/icon",
		ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class FlashlightActivity : Activity, ISurfaceHolderCallback {
		Camera camera;
		IList<string> cameraSupportedFlashModes;
		SurfaceView surfaceView;
		ISurfaceHolder surfaceHolder;
		LinearLayout mainLayout;
		const string FlashlightOnMode = Camera.Parameters.FlashModeTorch;
		const string FlashlightOffMode = Camera.Parameters.FlashModeOff;
		bool isPreviewing = false;
		ImageView flashIcon;

		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);
			mainLayout = FindViewById<LinearLayout>(Resource.Id.FlashButtonLayout);
			surfaceView = FindViewById<SurfaceView>(Resource.Id.SurfaceView);
			surfaceHolder = surfaceView.Holder;
			surfaceHolder.AddCallback(this);
			surfaceHolder.SetType(SurfaceType.PushBuffers);

			flashIcon = FindViewById<ImageView>(Resource.Id.FlashIcon);
			flashIcon.Click += (sender, args) => {
				ToggleFlash();
			};
			StartLightMode();
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
			StartLightMode();
		}
		protected override void OnPause() {
			base.OnPause();
			StopLightMode();
		}

		public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height) {
		}
		public void SurfaceCreated(ISurfaceHolder holder) {
			if (camera != null) {
				camera.SetPreviewDisplay(holder);
			}
		}
		public void SurfaceDestroyed(ISurfaceHolder holder) {
		}

		void StartCamera() {
			if (camera == null) {
				camera = Camera.Open();
				cameraSupportedFlashModes = cameraSupportedFlashModes ?? camera.GetParameters().SupportedFlashModes;
				if (cameraSupportedFlashModes == null || !cameraSupportedFlashModes.Contains(FlashlightOnMode) || !cameraSupportedFlashModes.Contains(FlashlightOffMode)) {
					StopCamera();
				}
			}
		}
		void StartPreviewing() {
			if (camera == null) {
				StartCamera();
			}
			if (camera != null && !isPreviewing) {
				camera.StartPreview();
				isPreviewing = true;
			}
		}
		void StopPreviewing() {
			if (camera != null && isPreviewing) {
				camera.StopPreview();
			}
			isPreviewing = false;
		}
		void StopCamera() {
			StopPreviewing();
			if (camera != null) {
				camera.Release();
				camera = null;
			}
		}

		protected string GetCameraFlashMode(Camera.Parameters cameraParameters = null) {
			string mode = null;
			if (camera != null) {
				if (cameraParameters == null) {
					cameraParameters = camera.GetParameters();
				}
				mode = cameraParameters.FlashMode;
			}
			return mode;
		}

		protected void SetCameraFlashMode(string newMode) {
			if (camera != null) {
				Camera.Parameters cameraParameters = camera.GetParameters();
				if (newMode != GetCameraFlashMode(cameraParameters)) {
					cameraParameters.FlashMode = newMode;
					camera.SetParameters(cameraParameters);
				}
			}
		}

		void ToggleFlash() {
			if (camera == null) {
				if (!isLighting) {
					StartLightMode();
				} else {
					StopLightMode();
				}
			} else {
				// TODO: Try out isLighting.
				if (FlashlightOnMode != GetCameraFlashMode()) {
					StartLightMode();
				} else {
					StopLightMode();
				}
			}
		}

		bool isLighting = false;
		void StartLightMode() {
			if (camera != null) {
				StartPreviewing();
				SetCameraFlashMode(FlashlightOnMode);
				flashIcon.SetImageResource(Resource.Drawable.PowerIconWhite);
			} else {
				mainLayout.SetBackgroundColor(Android.Graphics.Color.White);
				flashIcon.SetImageResource(Resource.Drawable.PowerIconGray);
			}
			isLighting = true;
		}
		void StopLightMode() {
			if (camera != null) {
				StopPreviewing();
				SetCameraFlashMode(FlashlightOffMode);
				flashIcon.SetImageResource(Resource.Drawable.PowerIconGray);
			} else {
				mainLayout.SetBackgroundColor(Android.Graphics.Color.Black);
				flashIcon.SetImageResource(Resource.Drawable.PowerIconWhite);
			}
			isLighting = false;
		}
	}
}