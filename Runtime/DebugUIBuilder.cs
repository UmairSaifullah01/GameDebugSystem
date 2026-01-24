using System;
using System.Collections.Generic;
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
			public GameObject rowPrefab;
			public GameObject buttonPrefab;
			public GameObject sliderPrefab;
			public GameObject inputFieldPrefab;
			public GameObject numberInputFieldPrefab;
			public GameObject toggleInputFieldPrefab;
			public GameObject dropdownPrefab;

			public static UIPreset CreateDefault() => new UIPreset
			{
				canvasPrefab = null,
				categoryPrefab = null,
				buttonPrefab = null,
				sliderPrefab = null,
				inputFieldPrefab = null,
				numberInputFieldPrefab = null,
				toggleInputFieldPrefab = null,
				dropdownPrefab = null,
				rowPrefab=null
			};
		}

		private static UIPreset _activePreset = UIPreset.CreateDefault();
		public static UIPreset ActivePreset => _activePreset;

		private static RectTransform _cachedContentRoot;
		
		public static void ApplyUIPreset(UIPreset preset)
		{
			_activePreset = preset;
		}

		public static void ResetCache()
		{
			_cachedContentRoot = null;
		}

		private readonly RectTransform _categoryRoot;
		private readonly string _categoryName;
		private RectTransform _currentRow; // Track current row to support EndRow()
		public static Transform toggleButton;
		private DebugUIBuilder(string categoryName, RectTransform categoryRoot)
		{
			_categoryName = categoryName;
			_categoryRoot = categoryRoot;
			_currentRow = null;
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
			AddButton(label, onClick, out _);
		}

		/// <summary>
		/// Adds a button with a label and click callback, returning the button instance for further modification.
		/// </summary>
		/// <param name="label">Button label text</param>
		/// <param name="onClick">Click callback action</param>
		/// <param name="button">Output parameter for the created button instance</param>
		public void AddButton(string label, Action onClick, out Button button)
		{
			var row = GetOrCreateRow();
			var buttonRT = InstantiatePrefab(_activePreset.buttonPrefab, row, $"{_categoryName}_Button_{label}");
			button = buttonRT.GetComponentInChildren<Button>(true);
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
		public void AddSlider(string label, float min, float max, float defaultValue, Action<float> onChanged, Func<float> getter = null, float step = 0f)
		{
			var row = GetOrCreateRow();
			var sliderRT = InstantiatePrefab(_activePreset.sliderPrefab, row, $"{_categoryName}_Slider_{label}");

			var slider = sliderRT.GetComponentInChildren<Slider>(true);
			if (slider == null)
			{
				Debug.LogError($"[DebugUIBuilder] Slider prefab is missing a Slider component for '{label}'.");
				return;
			}

			slider.minValue = min;
			slider.maxValue = max;
			
			// Use getter if provided, otherwise use default value
			float initialValue = getter != null ? getter() : defaultValue;
			slider.value = Mathf.Clamp(initialValue, min, max);

			SetLabelText(sliderRT, label);

			var valueText = FindText(sliderRT, "Value");
			if (valueText != null)
			{
				valueText.text = slider.value.ToString("0.##");
			}

			// Action to update slider value with clamping and notification
			Action<float> UpdateSliderValue = (newValue) =>
			{
				var clamped = Mathf.Clamp(newValue, min, max);
				slider.SetValueWithoutNotify(clamped);
				if (valueText != null) valueText.text = clamped.ToString("0.##");
				if (onChanged != null) onChanged(clamped);
			};

			// Add increment/decrement buttons if step is provided
			if (step > 0f)
			{
				var buttons = sliderRT.GetComponentsInChildren<Button>(true);
				if (buttons != null)
				{
					foreach (var button in buttons)
					{
						if (button == null) continue;
						var nameLower = button.name.ToLowerInvariant();
						if (nameLower.Contains("left") || nameLower.Contains("dec") || nameLower.Contains("minus"))
						{
							button.onClick.AddListener(() => UpdateSliderValue(slider.value - step));
						}
						else if (nameLower.Contains("right") || nameLower.Contains("inc") || nameLower.Contains("plus"))
						{
							button.onClick.AddListener(() => UpdateSliderValue(slider.value + step));
						}
					}
				}
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
		public void AddInputField(string label, string placeholder, Action<string> onSubmit, Func<string> getter = null)
		{
			var row = GetOrCreateRow();
			var inputRT = InstantiatePrefab(_activePreset.inputFieldPrefab, row, $"{_categoryName}_Input_{label}");
			SetLabelText(inputRT, label);

			var input = inputRT.GetComponentInChildren<TMP_InputField>(true);
			if (input == null)
			{
				Debug.LogError($"[DebugUIBuilder] Input prefab is missing a TMP_InputField component for '{label}'.");
				return;
			}

			input.lineType = TMP_InputField.LineType.SingleLine;

			// Use getter if provided to set initial value
			if (getter != null)
			{
				var currentValue = getter();
				if (!string.IsNullOrEmpty(currentValue))
				{
					input.text = currentValue;
				}
			}

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
		public void AddNumberField(string label, float min, float max, float step, float defaultValue, Action<float> onChanged, Func<float> getter = null)
		{
			var row = GetOrCreateRow();
			var numberRT = InstantiatePrefab(_activePreset.numberInputFieldPrefab, row, $"{_categoryName}_Number_{label}");
			SetLabelText(numberRT, label);

			var input = numberRT.GetComponentInChildren<TMP_InputField>(true);
			if (input == null)
			{
				Debug.LogError($"[DebugUIBuilder] Number input prefab is missing a TMP_InputField for '{label}'.");
				return;
			}

			// Use getter if provided, otherwise use default value
			float initialValue = getter != null ? getter() : defaultValue;
			float current = Mathf.Clamp(initialValue, min, max);
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
		public void AddToggle(string label, bool defaultValue, Action<bool> onChanged, Func<bool> getter = null)
		{
			var row = GetOrCreateRow();
			var toggleRT = InstantiatePrefab(_activePreset.toggleInputFieldPrefab, row, $"{_categoryName}_Toggle_{label}");
			SetLabelText(toggleRT, label);

			var toggle = toggleRT.GetComponentInChildren<Toggle>(true);
			if (toggle == null)
			{
				Debug.LogError($"[DebugUIBuilder] Toggle prefab is missing a Toggle component for '{label}'.");
				return;
			}
			
			// Use getter if provided, otherwise use default value
			toggle.isOn = getter != null ? getter() : defaultValue;

			if (onChanged != null)
			{
				toggle.onValueChanged.AddListener(onChanged.Invoke);
			}
		}

		/// <summary>
		/// Adds a dropdown with label, options, and value changed callback.
		/// </summary>
		public void AddDropdown(string label, IEnumerable<string> options, int defaultIndex, Action<int> onChanged, Func<int> getter = null)
		{
			var row = GetOrCreateRow();
			var dropdownRT = InstantiatePrefab(_activePreset.dropdownPrefab, row, $"{_categoryName}_Dropdown_{label}");

			var dropdown = dropdownRT.GetComponentInChildren<TMP_Dropdown>(true);
			if (dropdown == null)
			{
				Debug.LogError($"[DebugUIBuilder] Dropdown prefab is missing a TMP_Dropdown component for '{label}'.");
				return;
			}

			SetLabelText(dropdownRT, label);

			// Clear existing options and add new ones
			dropdown.ClearOptions();
			var optionsList = options?.ToList() ?? new List<string>();
			if (optionsList.Count == 0)
			{
				Debug.LogWarning($"[DebugUIBuilder] Dropdown '{label}' has no options provided.");
				return;
			}
			dropdown.AddOptions(optionsList);

			// Use getter if provided, otherwise use default index
			int initialIndex = getter != null ? getter() : defaultIndex;
			int clampedIndex = Mathf.Clamp(initialIndex, 0, optionsList.Count - 1);
			dropdown.SetValueWithoutNotify(clampedIndex);

			if (onChanged != null)
			{
				dropdown.onValueChanged.AddListener(onChanged.Invoke);
			}
		}

		/// <summary>
		/// Ends the current row, forcing the next element to be added to a new row.
		/// </summary>
		public void EndRow()
		{
			_currentRow = null;
		}

		// Helpers
		/// <summary>
		/// Gets or creates a row that has space for a new element (max 2 elements per row).
		/// Each category manages its own rows independently, so multiple categories work correctly.
		/// Rows are added after the Header element in the category prefab.
		/// </summary>
		private RectTransform GetOrCreateRow()
		{
			// If EndRow() was called, force a new row
			if (_currentRow == null)
			{
				_currentRow = CreateNewRow();
				return _currentRow;
			}
			
			// Check if current row has space
			if (_currentRow.childCount < 2)
			{
				return _currentRow;
			}
			
			// Current row is full, create a new one
			_currentRow = CreateNewRow();
			return _currentRow;
		}

		/// <summary>
		/// Creates a new row after the Header element.
		/// </summary>
		private RectTransform CreateNewRow()
		{
			// Find the Header element (if it exists) to know where rows start
			Transform headerTransform = null;
			int headerIndex = -1;
			int childCount = _categoryRoot.childCount;
			
			for (int i = 0; i < childCount; i++)
			{
				var child = _categoryRoot.GetChild(i);
				var nameLower = child.name.ToLowerInvariant();
				if (nameLower.Contains("header"))
				{
					headerTransform = child;
					headerIndex = i;
					break;
				}
			}
			
			// Count only rows (excluding Header) for naming
			int rowCount = headerIndex >= 0 ? childCount - 1 : childCount;
			var newRow = InstantiatePrefab(_activePreset.rowPrefab, _categoryRoot, $"{_categoryName}_Row_{rowCount}");
			
			// Ensure the row is positioned after the Header
			// InstantiatePrefab adds at the end, which is correct if Header is at the beginning
			// If Header is somehow not at the beginning, ensure row is after it
			if (headerIndex >= 0 && headerIndex > 0)
			{
				// Header is not at the beginning, ensure row is after Header
				// After instantiation, childCount has increased, so we need to account for that
				int targetIndex = headerIndex + 1;
				if (newRow.GetSiblingIndex() < targetIndex)
				{
					newRow.SetSiblingIndex(targetIndex);
				}
			}
			// Otherwise, Header is at index 0 or doesn't exist, and new row is at the end (correct)
			
			return newRow;
		}

		private static RectTransform EnsureRootContainer(Canvas canvas)
		{
			if (_cachedContentRoot != null) return _cachedContentRoot;
			var root = canvas.transform.Find("Root/ScrollView/Viewport/Content") as RectTransform;
			if (root != null)
			{
				_cachedContentRoot = root;
				return root;
			}
			root = canvas.transform.Find("Root") as RectTransform;
			if (root != null)
			{
				_cachedContentRoot = root;
				return root;
			}
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

		/// <summary>
		/// Changes the transparency (alpha) of a button's colors.
		/// Updates normal, highlighted, pressed, and selected colors.
		/// </summary>
		/// <param name="button">The button to modify</param>
		/// <param name="alpha">Alpha value between 0 (transparent) and 1 (opaque)</param>
		public static void SetButtonTransparency(Button button, float alpha)
		{
			if (button == null)
			{
				Debug.LogError("[DebugUIBuilder] Button is null, cannot change transparency.");
				return;
			}

			alpha = Mathf.Clamp01(alpha);
			var colors = button.colors;
			
			// Update all color states with new alpha
			colors.normalColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, alpha);
			colors.highlightedColor = new Color(colors.highlightedColor.r, colors.highlightedColor.g, colors.highlightedColor.b, alpha);
			colors.pressedColor = new Color(colors.pressedColor.r, colors.pressedColor.g, colors.pressedColor.b, alpha);
			colors.selectedColor = new Color(colors.selectedColor.r, colors.selectedColor.g, colors.selectedColor.b, alpha);
			colors.disabledColor = new Color(colors.disabledColor.r, colors.disabledColor.g, colors.disabledColor.b, alpha);
			
			button.colors = colors;

			// Also update the Image component's alpha if it exists
			var image = button.GetComponent<Image>();
			if (image != null)
			{
				var imageColor = image.color;
				imageColor.a = alpha;
				image.color = imageColor;
			}
		}
	}
}


