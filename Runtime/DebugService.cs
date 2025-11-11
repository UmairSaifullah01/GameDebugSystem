using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace THEBADDEST.GameDebugSystem
{


	[CreateAssetMenu(fileName = "CheatService", menuName = "Cheats/Cheat Service", order = 0)]
	public sealed class DebugService : ScriptableObject
	{

		[Header("Wrappers")][SerializeField] private List<DebugWrapperBase> _wrappers = new List<DebugWrapperBase>();

		[Header("UI Presets")][SerializeField] private DebugUIBuilder.UIPreset _uiPreset = DebugUIBuilder.UIPreset.CreateDefault();

		public DebugUIBuilder.UIPreset UIPreset => _uiPreset;

		/// <summary>
		/// Build or rebuild the cheat UI onto the provided canvas.
		/// </summary>
		public void Init()
		{
			// Ensure or create overlay canvas (prefer preset)
			var canvas = CreateOverlayCanvas();
			if (canvas == null)
			{
				Debug.LogError("[DebugService] Failed to create or locate a Canvas for debug UI.");
				return;
			}

			DebugUIBuilder.ApplyUIPreset(_uiPreset);

			// Categories are created via the builder (which ensures a shared scroll root)
			foreach (var wrapper in _wrappers)
			{
				if (wrapper == null) continue;
				try
				{
					var builder = DebugUIBuilder.CreateCategory(wrapper.CategoryName, canvas);
					wrapper.RegisterCheats(builder);
				}
				catch (Exception ex)
				{
					Debug.LogError($"[DebugService] Error while registering cheats for wrapper: {(wrapper != null ? wrapper.name : "<null>")}");
					Debug.LogException(ex);
				}
			}
		}

		private Canvas CreateOverlayCanvas()
		{
			// Prefer a preset canvas prefab
			if (_uiPreset.canvasPrefab != null)
			{
				var instance = Instantiate(_uiPreset.canvasPrefab);
				instance.name = "CheatCanvas";
				var presetCanvas = instance.GetComponent<Canvas>() ?? instance.AddComponent<Canvas>();
				EnsureCanvasDefaults(presetCanvas);
				return presetCanvas;
			}

			var canvasGO = new GameObject("CheatCanvas",
				typeof(Canvas),
				typeof(CanvasScaler),
				typeof(GraphicRaycaster));

			var canvas = canvasGO.GetComponent<Canvas>();
			EnsureCanvasDefaults(canvas);
			return canvas;
		}

		private static void EnsureCanvasDefaults(Canvas canvas)
		{
			if (canvas == null) return;
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, 5000);

			var scaler = canvas.GetComponent<CanvasScaler>();
			if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			scaler.matchWidthOrHeight = 0.5f;

			if (canvas.GetComponent<GraphicRaycaster>() == null)
			{
				canvas.gameObject.AddComponent<GraphicRaycaster>();
			}
		}

	}


}