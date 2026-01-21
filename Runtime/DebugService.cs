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
		[SerializeField] private CornerFlags _openCorners = CornerFlags.TopLeft | CornerFlags.TopRight | CornerFlags.BottomLeft | CornerFlags.BottomRight;
		[SerializeField] private CornerFlags _closeCorners = CornerFlags.TopLeft | CornerFlags.TopRight | CornerFlags.BottomLeft | CornerFlags.BottomRight;
		[SerializeField] private bool _enableKeyboardToggle = true;
		[SerializeField] private KeyCode _toggleKey1 = KeyCode.F1;
		[SerializeField] private KeyCode _toggleKey2 = KeyCode.BackQuote;


		[Header("UI Presets")] [SerializeField] private DebugUIBuilder.UIPreset _uiPreset = DebugUIBuilder.UIPreset.CreateDefault();

		[NonSerialized] private Canvas _cachedCanvas;
		[NonSerialized] private TriggerHandler _triggerHandler;
		[NonSerialized] private RectTransform _rootTransform;
		[NonSerialized] private readonly List<IDebugWrapper> _runtimeWrappers = new List<IDebugWrapper>();


		public DebugUIBuilder.UIPreset UIPreset => _uiPreset;
		public bool IsVisible
		{
			get { return _rootTransform != null && _rootTransform.gameObject.activeSelf; }
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
			_rootTransform = canvas.transform.Find("Root") as RectTransform;
			for (int i = 0; i < _wrappers.Count; i++)
			{
				var w = _wrappers[i];
				if (w == null) continue;
				if (!_runtimeWrappers.Contains(w)) _runtimeWrappers.Add(w);
			}

			// Add toggle button if UIButton trigger type is selected
			CreateToggleButton(canvas);
			CreateCrossButton(canvas);

			// Categories are created via the builder (which ensures a shared scroll root)
			foreach (var wrapper in _runtimeWrappers)
			{
				if (wrapper == null) continue;
				var builder = DebugUIBuilder.CreateCategory(wrapper.CategoryName, canvas);
				wrapper.RegisterCheats(builder);
			}

			// Setup trigger handler for corner tap detection
			SetupTriggerHandler(canvas);

			// Ensure canvas is active (needed for toggle button and corner tap detection)
			EnsureCanvasActive(canvas);
			if (_hideAtStart)
			{
				if (_rootTransform != null)
				{
					_rootTransform.gameObject.SetActive(false);
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

		public void AddWrapper(IDebugWrapper wrapper)
		{
			if (wrapper == null) return;
			if (!_runtimeWrappers.Contains(wrapper)) _runtimeWrappers.Add(wrapper);
			Rebuild();
		}

		public void RemoveWrapper(IDebugWrapper wrapper)
		{
			if (wrapper == null) return;
			if (_runtimeWrappers.Contains(wrapper)) _runtimeWrappers.Remove(wrapper);
			Rebuild();
		}

		public void Rebuild()
		{
			ClearBuiltUI();
			Init();
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

				if (_rootTransform != null)
				{
					StartFade(true);
				}

				return;
			}

			if (_rootTransform != null)
			{
				StartFade(false);
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
			return _rootTransform != null;
		}

		private void ClearBuiltUI()
		{
			if (_cachedCanvas != null)
			{
				var go = _cachedCanvas.gameObject;
				_cachedCanvas = null;
				_rootTransform = null;
				_triggerHandler = null;
				if (go != null)
				{
					if (Application.isPlaying) Destroy(go);
					else DestroyImmediate(go);
				}
			}
			else
			{
				var existing = GameObject.Find(CanvasName);
				if (existing != null)
				{
					if (Application.isPlaying) Destroy(existing);
					else DestroyImmediate(existing);
				}
			}

			DebugUIBuilder.ResetCache();
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
					button.onClick.AddListener(() =>
					{
						existingButton.gameObject.SetActive(false);
						Show();
					});
				}
				else
				{
					existingButton.gameObject.SetActive(false);
				}
			}
		}

		private void CreateCrossButton(Canvas canvas)
		{
			// Check if button already exists
			var existingButton = canvas.transform.Find("Root/CrossButton");
			if (existingButton != null)
			{
				var button = existingButton.GetComponent<Button>();
				button.onClick.AddListener(() =>
				{
					Hide();
					if (_triggerType == TriggerType.UIButton)
					{
						var debugButton = canvas.transform.Find("DebugToggleButton");
						debugButton?.gameObject?.SetActive(true);
					}
				});
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

			_triggerHandler.Initialize(this, _cornerTapCount, _cornerTapTimeout, _cornerTapAreaSize, _openCorners, _closeCorners, _enableKeyboardToggle, _toggleKey1, _toggleKey2);
		}

		private void StartFade(bool show)
		{
			if (_rootTransform == null) return;
			var go = _rootTransform.gameObject;
			var cg = go.GetComponent<CanvasGroup>();
			if (cg == null) cg = go.AddComponent<CanvasGroup>();
			var fader = _cachedCanvas != null ? _cachedCanvas.GetComponent<DebugCanvasFader>() : null;
			if (fader == null && _cachedCanvas != null)
			{
				fader = _cachedCanvas.gameObject.AddComponent<DebugCanvasFader>();
			}

			if (fader != null)
			{
				fader.Fade(cg, show, 0.15f);
			}
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