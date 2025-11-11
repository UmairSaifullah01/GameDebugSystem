using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace THEBADDEST.GameDebugSystem
{
	public sealed class CheatUIController : MonoBehaviour
	{
		[FormerlySerializedAs("_cheatService")] [SerializeField] private DebugService debugService;
		[SerializeField] private Canvas _targetCanvas;
		[SerializeField] private KeyCode _toggleKey = KeyCode.F1;

		private GameObject _canvasGO;

		private void Start()
		{
			if (debugService == null)
			{
				Debug.LogWarning("[CheatUIController] No CheatService assigned.");
				return;
			}

			if (_targetCanvas == null)
			{
				_targetCanvas = CreateOverlayCanvas(out _canvasGO);
			}

			debugService.Init(_targetCanvas);
			SetVisible(false);
		}

		private void Update()
		{
			if (Input.GetKeyDown(_toggleKey))
			{
				Toggle();
			}
		}

		private void Toggle()
		{
			var current = IsVisible();
			SetVisible(!current);
		}

		private bool IsVisible()
		{
			if (_targetCanvas == null) return false;
			return _targetCanvas.gameObject.activeSelf;
		}

		private void SetVisible(bool visible)
		{
			if (_targetCanvas == null) return;
			_targetCanvas.gameObject.SetActive(visible);
		}

		private static Canvas CreateOverlayCanvas(out GameObject canvasGO)
		{
			canvasGO = new GameObject("CheatCanvas",
				typeof(Canvas),
				typeof(CanvasScaler),
				typeof(GraphicRaycaster));

			var canvas = canvasGO.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 5000;

			var scaler = canvasGO.GetComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			scaler.matchWidthOrHeight = 1f;

			return canvas;
		}
	}
}


