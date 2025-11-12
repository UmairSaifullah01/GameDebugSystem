using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace THEBADDEST.GameDebugSystem
{


	public enum TriggerType
	{

		None,
		UIButton,
		CornerTap

	}

	[CreateAssetMenu(fileName = "DebugService", menuName = "THEBADDEST/Debug Service", order = 0)]
	public sealed class DebugService : ScriptableObject
	{

		private const string CanvasName = "CheatCanvas";
		[Header("Wrappers")] [SerializeField] private List<DebugWrapperBase> _wrappers = new List<DebugWrapperBase>();

		[Header("Trigger Options")] [SerializeField] private bool _hideAtStart = true;
		[SerializeField] private TriggerType _triggerType = TriggerType.UIButton;

		[SerializeField] private int _cornerTapCount = 5;
		[SerializeField] private float _cornerTapTimeout = 2f;
		[SerializeField] private float _cornerTapAreaSize = 0.1f; // 10% of screen size


		[Header("UI Presets")] [SerializeField] private DebugUIBuilder.UIPreset _uiPreset = DebugUIBuilder.UIPreset.CreateDefault();

		[NonSerialized] private Canvas _cachedCanvas;
		[NonSerialized] private TriggerHandler _triggerHandler;


		public DebugUIBuilder.UIPreset UIPreset => _uiPreset;
		public bool IsVisible
		{
			get
			{
				if (_cachedCanvas == null) return false;
				var root = _cachedCanvas.transform.Find("Root");
				return root != null && root.gameObject.activeSelf;
			}
		}
		/// <summary>
		/// Build or rebuild the cheat UI onto the provided canvas.
		/// </summary>
		public void Init()
		{
			// Ensure or create overlay canvas (prefer preset)
			var canvas = CreateOrGetOverlayCanvas();
			if (canvas == null)
			{
				Debug.LogError("[CheatService] Failed to create or locate a Canvas for debug UI.");
				return;
			}

			DontDestroyOnLoad(canvas.gameObject);
			_cachedCanvas = canvas;
			DebugUIBuilder.ApplyUIPreset(_uiPreset);

			// Add toggle button if UIButton trigger type is selected
			CreateToggleButton(canvas);

			// Categories are created via the builder (which ensures a shared scroll root)
			foreach (var wrapper in _wrappers)
			{
				if (wrapper == null) continue;
				var builder = DebugUIBuilder.CreateCategory(wrapper.CategoryName, canvas);
				wrapper.RegisterCheats(builder);
			}

			// Setup trigger handler for corner tap detection
			SetupTriggerHandler(canvas);

			// Ensure canvas is active (needed for toggle button and corner tap detection)
			EnsureCanvasActive(canvas);

			// Hide at start if configured (hide Root content, keep canvas active for toggle button)
			if (_hideAtStart)
			{
				var root = canvas.transform.Find("Root");
				if (root != null)
				{
					root.gameObject.SetActive(false);
				}
			}
		}

		public void Show()
		{
			SetCanvasVisibility(true);
		}

		public void Hide()
		{
			SetCanvasVisibility(false);
		}

		public void ToggleVisibility()
		{
			SetCanvasVisibility(!IsVisible);
		}
		
		private void SetCanvasVisibility(bool visible)
		{
			if (visible)
			{
				if (!HasBuiltUI())
				{
					Init();
					return;
				}

				var canvas = _cachedCanvas ?? CreateOrGetOverlayCanvas();
				if (canvas == null) return;

				// Ensure canvas is active
				if (!canvas.gameObject.activeSelf)
				{
					canvas.gameObject.SetActive(true);
				}

				// Show Root content
				var root = canvas.transform.Find("Root");
				if (root != null && !root.gameObject.activeSelf)
				{
					root.gameObject.SetActive(true);
				}

				return;
			}

			// Hide Root content but keep canvas active
			var canvasToHide = _cachedCanvas;
			if (canvasToHide != null)
			{
				var root = canvasToHide.transform.Find("Root");
				if (root != null && root.gameObject.activeSelf)
				{
					root.gameObject.SetActive(false);
				}
			}
		}

		private Canvas CreateOrGetOverlayCanvas()
		{
			// Prefer a preset canvas prefab
			if (_uiPreset.canvasPrefab != null)
			{
				var instance = Instantiate(_uiPreset.canvasPrefab);
				instance.name = CanvasName;
				var presetCanvas = instance.GetComponent<Canvas>() ?? instance.AddComponent<Canvas>();
				EnsureCanvasDefaults(presetCanvas);
				_cachedCanvas = presetCanvas;
				return presetCanvas;
			}

			// Fallback: create a default overlay canvas
			var canvasGO = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
			var canvas = canvasGO.GetComponent<Canvas>();
			EnsureCanvasDefaults(canvas);
			_cachedCanvas = canvas;
			return canvas;
		}

		private bool HasBuiltUI()
		{
			var canvas = _cachedCanvas;
			if (canvas == null) return false;
			_cachedCanvas = canvas;
			return canvas.transform.Find("Root") != null;
		}

		private static void EnsureCanvasActive(Canvas canvas)
		{
			if (canvas == null) return;
			var go = canvas.gameObject;
			if (go != null && !go.activeSelf)
			{
				go.SetActive(true);
			}
		}


		private void CreateToggleButton(Canvas canvas)
		{
			// Check if button already exists
			var existingButton = canvas.transform.Find("DebugToggleButton");
			if (existingButton != null)
			{
				if (_triggerType == TriggerType.UIButton)
				{
					var button = existingButton.GetComponent<Button>();
					button.onClick.AddListener(ToggleVisibility);
				}
				else
				{
					existingButton.gameObject.SetActive(false);
				}
			}
		}

		private void SetupTriggerHandler(Canvas canvas)
		{
			if (_triggerType != TriggerType.CornerTap || canvas == null) return;

			// Get handler from canvas or its children (preset may have it)
			_triggerHandler = canvas.GetComponent<TriggerHandler>();
			if (_triggerHandler == null)
			{
				_triggerHandler = canvas.GetComponentInChildren<TriggerHandler>();
			}

			// Add to canvas if not found
			if (_triggerHandler == null)
			{
				_triggerHandler = canvas.gameObject.AddComponent<TriggerHandler>();
			}

			// Initialize or reinitialize
			_triggerHandler.Initialize(this, _cornerTapCount, _cornerTapTimeout, _cornerTapAreaSize);
		}

		private static void EnsureCanvasDefaults(Canvas canvas)
		{
			if (canvas == null) return;
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			if (canvas.sortingOrder < 5000) canvas.sortingOrder = 5000;
			var scaler = canvas.GetComponent<CanvasScaler>();
			if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			scaler.matchWidthOrHeight = 1f;
			if (canvas.GetComponent<GraphicRaycaster>() == null)
			{
				canvas.gameObject.AddComponent<GraphicRaycaster>();
			}
		}

	}


}