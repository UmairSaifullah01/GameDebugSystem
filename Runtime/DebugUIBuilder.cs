using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace THEBADDEST.GameDebugSystem
{
	/// <summary>
	/// Builder that creates uGUI controls organized under a category panel.
	/// </summary>
	public sealed class DebugUIBuilder
	{
		[Serializable]
		public struct UIPreset
		{
			public GameObject canvasPrefab;
			public GameObject categoryPrefab;
			public GameObject buttonPrefab;
			public GameObject sliderPrefab;
			public GameObject inputFieldPrefab;
			public GameObject numberInputFieldPrefab;
			public GameObject toggleInputFieldPrefab;

			public static UIPreset CreateDefault() => new UIPreset
			{
				canvasPrefab = null,
				categoryPrefab = null,
				buttonPrefab = null,
				sliderPrefab = null,
				inputFieldPrefab = null,
				numberInputFieldPrefab = null,
				toggleInputFieldPrefab = null,
			};
		}

		private static UIPreset _activePreset = UIPreset.CreateDefault();
		public static UIPreset ActivePreset => _activePreset;

		public static void ApplyUIPreset(UIPreset preset)
		{
			_activePreset = preset;
		}

		private readonly RectTransform _categoryRoot;
		private readonly string _categoryName;

		private DebugUIBuilder(string categoryName, RectTransform categoryRoot)
		{
			_categoryName = categoryName;
			_categoryRoot = categoryRoot;
		}

		/// <summary>
		/// Creates a category panel under the given canvas and returns a scoped builder.
		/// </summary>
		public static DebugUIBuilder CreateCategory(string name, Canvas parentCanvas)
		{
			if (parentCanvas == null) throw new ArgumentNullException(nameof(parentCanvas));

			var container = EnsureRootContainer(parentCanvas);
			var categoryPanel = InstantiatePrefab(_activePreset.categoryPrefab, container, $"{name} Category");
			SetLabelText(categoryPanel, name);

			return new DebugUIBuilder(name, categoryPanel);
		}

		/// <summary>
		/// Adds a button with a label and click callback.
		/// </summary>
		public void AddButton(string label, Action onClick)
		{
			var buttonRT = InstantiatePrefab(_activePreset.buttonPrefab, _categoryRoot, $"{_categoryName}_Button_{label}");
			var button = buttonRT.GetComponentInChildren<Button>(true);
			if (button == null)
			{
				Debug.LogError($"[DebugUIBuilder] Button prefab is missing a Button component for '{label}'.");
				return;
			}

			SetLabelText(buttonRT, label);
			if (onClick != null)
			{
				button.onClick.AddListener(() => onClick());
			}
		}

		/// <summary>
		/// Adds a slider with label and value changed callback.
		/// </summary>
		public void AddSlider(string label, float min, float max, float defaultValue, Action<float> onChanged)
		{
			var sliderRT = InstantiatePrefab(_activePreset.sliderPrefab, _categoryRoot, $"{_categoryName}_Slider_{label}");

			var slider = sliderRT.GetComponentInChildren<Slider>(true);
			slider.minValue = min;
			slider.maxValue = max;
			slider.value = Mathf.Clamp(defaultValue, min, max);

			SetLabelText(sliderRT, label);

			var valueText = FindText(sliderRT, "Value");
			if (valueText != null)
			{
				valueText.text = slider.value.ToString("0.##");
			}

			if (onChanged != null)
			{
				slider.onValueChanged.AddListener(v =>
				{
					if (valueText != null) valueText.text = v.ToString("0.##");
					onChanged(v);
				});
			}
			else if (valueText != null)
			{
				slider.onValueChanged.AddListener(v => valueText.text = v.ToString("0.##"));
			}
		}

		/// <summary>
		/// Adds an input field with label, placeholder, and submit callback.
		/// </summary>
		public void AddInputField(string label, string placeholder, Action<string> onSubmit)
		{
			var inputRT = InstantiatePrefab(_activePreset.inputFieldPrefab, _categoryRoot, $"{_categoryName}_Input_{label}");
			SetLabelText(inputRT, label);

			var input = inputRT.GetComponentInChildren<TMP_InputField>(true);
			if (input == null)
			{
				Debug.LogError($"[DebugUIBuilder] Input prefab is missing a TMP_InputField component for '{label}'.");
				return;
			}

			input.lineType = TMP_InputField.LineType.SingleLine;

			var placeholderText = input.placeholder as TMP_Text;
			if (placeholderText != null && !string.IsNullOrEmpty(placeholder))
			{
				placeholderText.text = placeholder;
			}

			if (onSubmit != null)
			{
				input.onSubmit.AddListener(onSubmit.Invoke);
			}

			if (onSubmit == null && !string.IsNullOrEmpty(placeholder))
			{
				// keep placeholder consistent without listeners
				input.onEndEdit.AddListener(_ => { });
			}
		}

		/// <summary>
		/// Adds a numeric input with optional '<' and '>' buttons to decrement/increment.
		/// Uses NumberInput prefab exclusively.
		/// </summary>
		public void AddNumberField(string label, float min, float max, float step, float defaultValue, Action<float> onChanged)
		{
			var numberRT = InstantiatePrefab(_activePreset.numberInputFieldPrefab, _categoryRoot, $"{_categoryName}_Number_{label}");
			SetLabelText(numberRT, label);

			var input = numberRT.GetComponentInChildren<TMP_InputField>(true);
			if (input == null)
			{
				Debug.LogError($"[DebugUIBuilder] Number input prefab is missing a TMP_InputField for '{label}'.");
				return;
			}

			float current = Mathf.Clamp(defaultValue, min, max);
			Action<TMP_InputField, float, bool> UpdateDisplay = (field, v, notify) =>
			{
				var clamped = Mathf.Clamp(v, min, max);
				current = clamped;
				if (notify && onChanged != null) onChanged(clamped);
				if (field != null) field.SetTextWithoutNotify(clamped.ToString("0.##"));
			};

			UpdateDisplay(input, current, false);

			var buttons = numberRT.GetComponentsInChildren<Button>(true);
			if (buttons != null)
			{
				foreach (var button in buttons)
				{
					if (button == null) continue;
					var nameLower = button.name.ToLowerInvariant();
					if (nameLower.Contains("left") || nameLower.Contains("dec") || nameLower.Contains("minus"))
					{
						button.onClick.AddListener(() => UpdateDisplay(input, current - Mathf.Abs(step), true));
					}
					else if (nameLower.Contains("right") || nameLower.Contains("inc") || nameLower.Contains("plus"))
					{
						button.onClick.AddListener(() => UpdateDisplay(input, current + Mathf.Abs(step), true));
					}
				}
			}

			input.onEndEdit.AddListener(str =>
			{
				if (float.TryParse(str, out var parsed))
				{
					UpdateDisplay(input, parsed, true);
				}
				else
				{
					UpdateDisplay(input, current, false);
				}
			});
		}

		/// <summary>
		/// Adds a toggle with label and value changed callback.
		/// </summary>
		public void AddToggle(string label, bool defaultValue, Action<bool> onChanged)
		{
			var toggleRT = InstantiatePrefab(_activePreset.toggleInputFieldPrefab, _categoryRoot, $"{_categoryName}_Toggle_{label}");
			SetLabelText(toggleRT, label);

			var toggle = toggleRT.GetComponentInChildren<Toggle>(true);
			if (toggle == null)
			{
				Debug.LogError($"[DebugUIBuilder] Toggle prefab is missing a Toggle component for '{label}'.");
				return;
			}
			toggle.isOn = defaultValue;

			if (onChanged != null)
			{
				toggle.onValueChanged.AddListener(onChanged.Invoke);
			}
		}

		// Helpers
		private static RectTransform EnsureRootContainer(Canvas canvas)
		{
			// Expect a prefab-created hierarchy: Root/ScrollView/Viewport/Content
			var root = canvas.transform.Find("Root/ScrollView/Viewport/Content") as RectTransform;
			if (root != null) return root;

			// Fallback: attempt to use Root directly
			root = canvas.transform.Find("Root") as RectTransform;
			if (root != null) return root;

			Debug.LogError("[DebugUIBuilder] Could not locate the preset Content container. Ensure the canvas prefab contains 'Root/ScrollView/Viewport/Content'.");
			throw new InvalidOperationException("Missing preset content root.");
		}

		private static RectTransform InstantiatePrefab(GameObject prefab, RectTransform parent, string name)
		{
			if (prefab == null)
			{
				Debug.LogError($"[DebugUIBuilder] Missing prefab for '{name}'. Please assign it in the UIPreset.");
				throw new InvalidOperationException($"Missing prefab for {name}");
			}

			var instance = Object.Instantiate(prefab, parent);
			instance.name = name;
			var rt = instance.transform as RectTransform;
			if (rt == null)
			{
				rt = instance.AddComponent<RectTransform>();
			}
			rt.SetParent(parent, false);
			return rt;
		}

		private static void SetLabelText(RectTransform root, string text)
		{
			if (root == null) return;
			var label = FindText(root, "Label") ?? root.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault();
			if (label != null)
			{
				label.text = text;
			}
		}

		private static TextMeshProUGUI FindText(RectTransform root, string nameContains)
		{
			if (root == null) return null;
			var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
			return texts.FirstOrDefault(t => t.name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) >= 0);
		}
	}
}


