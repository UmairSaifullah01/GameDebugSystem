using System;
using System.Collections.Generic;
using UnityEngine;

namespace THEBADDEST.GameDebugSystem
{
	[CreateAssetMenu(fileName = "CheatService", menuName = "Cheats/Cheat Service", order = 0)]
	public sealed class DebugService : ScriptableObject
	{
		[Header("Wrappers")]
		[SerializeField]
		private List<DebugWrapperBase> _wrappers = new List<DebugWrapperBase>();

		[Header("UI Presets")]
		[SerializeField]
		private DebugUIBuilder.UIPreset _uiPreset = DebugUIBuilder.UIPreset.CreateDefault();

		/// <summary>
		/// Build or rebuild the cheat UI onto the provided canvas.
		/// </summary>
		public void Init(Canvas canvas)
		{
			if (canvas == null)
			{
				Debug.LogError("[CheatService] Init called with null Canvas.");
				return;
			}

			// Clear previous build if any
			var existingRoot = canvas.transform.Find("CheatRoot");
			if (existingRoot != null)
			{
				Destroy(existingRoot.gameObject);
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
					Debug.LogException(ex);
				}
			}
		}
	}
}


