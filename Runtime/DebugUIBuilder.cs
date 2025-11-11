using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
			public GameObject buttonPrefab;
			public GameObject sliderPrefab;
			public GameObject inputFieldPrefab;
			public GameObject numberInputFieldPrefab;

			public static UIPreset CreateDefault() => new UIPreset
			{
				buttonPrefab = null,
				sliderPrefab = null,
				inputFieldPrefab = null,
				numberInputFieldPrefab = null,
			};
		}

		private static UIPreset _activePreset = UIPreset.CreateDefault();

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
			var categoryPanel = CreateUIObject($"{name} Category", container);

			var image = categoryPanel.gameObject.AddComponent<Image>();
			image.color = new Color(0f, 0f, 0f, 0.5f);

			var layout = categoryPanel.gameObject.AddComponent<VerticalLayoutGroup>();
			layout.padding = new RectOffset(10, 10, 10, 10);
			layout.spacing = 6f;
			layout.childAlignment = TextAnchor.UpperLeft;
			layout.childControlHeight = true;
			layout.childControlWidth = true;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = true;

			var fitter = categoryPanel.gameObject.AddComponent<ContentSizeFitter>();
			fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			// Header
			CreateHeader($"{name}", categoryPanel);

			return new DebugUIBuilder(name, categoryPanel);
		}

		/// <summary>
		/// Adds a button with a label and click callback.
		/// </summary>
		public void AddButton(string label, Action onClick)
		{
			var row = CreateRow($"{_categoryName}_Button_{label}");

			var buttonRT = InstantiateControl(_activePreset.buttonPrefab, row, "Button");
			var button = buttonRT.GetComponent<Button>() ?? buttonRT.gameObject.AddComponent<Button>();
			var image = buttonRT.GetComponent<Image>() ?? buttonRT.gameObject.AddComponent<Image>();
			if (_activePreset.buttonPrefab == null)
			{
				image.color = new Color(1f, 1f, 1f, 0.15f);
			}

			if (onClick != null)
			{
				button.onClick.AddListener(() => onClick());
			}

			var layoutElem = buttonRT.GetComponent<LayoutElement>() ?? buttonRT.gameObject.AddComponent<LayoutElement>();
			layoutElem.preferredHeight = Mathf.Max(layoutElem.preferredHeight, 32f);

			var text = buttonRT.GetComponentInChildren<TextMeshProUGUI>();
			if (text == null)
			{
				var textGO = CreateUIObject("Label", buttonRT);
				text = textGO.gameObject.AddComponent<TextMeshProUGUI>();
				text.alignment = TextAlignmentOptions.Center;
				text.color = Color.white;
				StretchToFill(text.rectTransform);
			}
			text.text = label;

			StretchToFill(buttonRT);
			StretchToFill(row);
		}

		/// <summary>
		/// Adds a slider with label and value changed callback.
		/// </summary>
		public void AddSlider(string label, float min, float max, float defaultValue, Action<float> onChanged)
		{
			var row = CreateRow($"{_categoryName}_Slider_{label}");

			// Label
			var labelGO = CreateUIObject("Label", row);
			var labelText = labelGO.gameObject.AddComponent<TextMeshProUGUI>();
			labelText.text = label;
			labelText.alignment = TextAlignmentOptions.MidlineLeft;
			labelText.color = Color.white;
			var labelLE = labelGO.gameObject.GetComponent<LayoutElement>() ?? labelGO.gameObject.AddComponent<LayoutElement>();
			labelLE.minWidth = 100f;
			labelLE.preferredHeight = 28f;

			// Slider
			var sliderRT = InstantiateControl(_activePreset.sliderPrefab, row, "Slider");
			var sliderBg = sliderRT.GetComponent<Image>() ?? sliderRT.gameObject.AddComponent<Image>();
			if (_activePreset.sliderPrefab == null)
			{
				sliderBg.color = new Color(1f, 1f, 1f, 0.08f);
			}

			var slider = sliderRT.GetComponent<Slider>() ?? sliderRT.gameObject.AddComponent<Slider>();
			slider.minValue = min;
			slider.maxValue = max;
			slider.value = Mathf.Clamp(defaultValue, min, max);

			if (slider.fillRect == null)
			{
				var fillArea = CreateUIObject("Fill Area", sliderRT);
				fillArea.anchorMin = new Vector2(0f, 0.25f);
				fillArea.anchorMax = new Vector2(1f, 0.75f);
				fillArea.offsetMin = new Vector2(10f, 0f);
				fillArea.offsetMax = new Vector2(-40f, 0f);

				var fill = CreateUIObject("Fill", fillArea);
				var fillImg = fill.gameObject.AddComponent<Image>();
				fillImg.color = new Color(0.3f, 0.7f, 1f, 0.8f);
				slider.fillRect = fill;

				var handleSlideArea = CreateUIObject("Handle Slide Area", sliderRT);
				handleSlideArea.anchorMin = new Vector2(0f, 0f);
				handleSlideArea.anchorMax = new Vector2(1f, 1f);
				handleSlideArea.offsetMin = new Vector2(10f, 0f);
				handleSlideArea.offsetMax = new Vector2(-10f, 0f);

				var handle = CreateUIObject("Handle", handleSlideArea);
				var handleImg = handle.gameObject.AddComponent<Image>();
				handleImg.color = new Color(1f, 1f, 1f, 0.9f);
				slider.targetGraphic = handleImg;
				slider.handleRect = handle;
			}

			var sliderLE = sliderRT.GetComponent<LayoutElement>() ?? sliderRT.gameObject.AddComponent<LayoutElement>();
			sliderLE.flexibleWidth = 1f;
			sliderLE.preferredHeight = Mathf.Max(sliderLE.preferredHeight, 28f);

			// Value text
			var valueGO = CreateUIObject("Value", row);
			var valueText = valueGO.gameObject.AddComponent<TextMeshProUGUI>();
			valueText.text = slider.value.ToString("0.##");
			valueText.alignment = TextAlignmentOptions.MidlineRight;
			valueText.color = Color.white;
			var valueLE = valueGO.gameObject.GetComponent<LayoutElement>() ?? valueGO.gameObject.AddComponent<LayoutElement>();
			valueLE.minWidth = 60f;
			valueLE.preferredHeight = 28f;

			if (onChanged != null)
			{
				slider.onValueChanged.AddListener(v =>
				{
					valueText.text = v.ToString("0.##");
					onChanged(v);
				});
			}
			else
			{
				slider.onValueChanged.AddListener(v => valueText.text = v.ToString("0.##"));
			}

			StretchToFill(row);
		}

		/// <summary>
		/// Adds an input field with label, placeholder, and submit callback.
		/// </summary>
		public void AddInputField(string label, string placeholder, Action<string> onSubmit)
		{
			var row = CreateRow($"{_categoryName}_Input_{label}");

			// Label
			var labelGO = CreateUIObject("Label", row);
			var labelText = labelGO.gameObject.AddComponent<TextMeshProUGUI>();
			labelText.text = label;
			labelText.alignment = TextAlignmentOptions.MidlineLeft;
			labelText.color = Color.white;
			var labelLE = labelGO.gameObject.GetComponent<LayoutElement>() ?? labelGO.gameObject.AddComponent<LayoutElement>();
			labelLE.minWidth = 100f;
			labelLE.preferredHeight = 28f;

			// Input Field container
			var inputRT = InstantiateControl(_activePreset.inputFieldPrefab, row, "InputField");
			var bg = inputRT.GetComponent<Image>() ?? inputRT.gameObject.AddComponent<Image>();
			if (_activePreset.inputFieldPrefab == null)
			{
				bg.color = new Color(1f, 1f, 1f, 0.1f);
			}

			var input = inputRT.GetComponent<TMP_InputField>() ?? inputRT.gameObject.AddComponent<TMP_InputField>();

			if (input.textComponent == null)
			{
				var textGO = CreateUIObject("Text", inputRT);
				var text = textGO.gameObject.AddComponent<TextMeshProUGUI>();
				text.color = Color.white;
				text.alignment = TextAlignmentOptions.MidlineLeft;
				input.textComponent = text;
				StretchToFill(text.rectTransform);
			}

			if (input.placeholder == null)
			{
				var placeholderGO = CreateUIObject("Placeholder", inputRT);
				var placeholderText = placeholderGO.gameObject.AddComponent<TextMeshProUGUI>();
				placeholderText.text = string.IsNullOrEmpty(placeholder) ? "Enter value..." : placeholder;
				placeholderText.color = new Color(1f, 1f, 1f, 0.5f);
				placeholderText.alignment = TextAlignmentOptions.MidlineLeft;
				input.placeholder = placeholderText;
				StretchToFill(placeholderText.rectTransform);
			}
			else if (!string.IsNullOrEmpty(placeholder))
			{
				switch (input.placeholder)
				{
					case TextMeshProUGUI tmpPlaceholder:
						tmpPlaceholder.text = placeholder;
						break;
					case TMP_Text tmpText:
						tmpText.text = placeholder;
						break;
				}
			}

			input.lineType = TMP_InputField.LineType.SingleLine;

			var inputLE = inputRT.GetComponent<LayoutElement>() ?? inputRT.gameObject.AddComponent<LayoutElement>();
			inputLE.flexibleWidth = 1f;
			inputLE.preferredHeight = Mathf.Max(inputLE.preferredHeight, 28f);

			if (onSubmit != null)
			{
				input.onSubmit.AddListener(onSubmit.Invoke);
			}

			StretchToFill(row);
		}

		/// <summary>
		/// Adds a numeric input with '<' and '>' buttons to decrement/increment.
		/// </summary>
		// public void AddNumberField(string label, float min, float max, float step, float defaultValue, Action<float> onChanged)
		// {
		// 	var row = CreateRow($"{_categoryName}_Number_{label}");
		//
		// 	// Label
		// 	var labelGO = CreateUIObject("Label", row);
		// 	var labelText = labelGO.gameObject.AddComponent<TextMeshProUGUI>();
		// 	labelText.text = label;
		// 	labelText.alignment = TextAlignmentOptions.MidlineLeft;
		// 	labelText.color = Color.white;
		// 	var labelLE = labelGO.gameObject.GetComponent<LayoutElement>() ?? labelGO.gameObject.AddComponent<LayoutElement>();
		// 	labelLE.minWidth = 100f;
		// 	labelLE.preferredHeight = 28f;
		//
		// 	// State and updater
		// 	float current = Mathf.Clamp(defaultValue, min, max);
		// 	Action<TMP_InputField, float, bool> UpdateDisplay = (field, v, notify) =>
		// 	{
		// 		var clamped = Mathf.Clamp(v, min, max);
		// 		current = clamped;
		// 		if (notify && onChanged != null) onChanged(clamped);
		// 		if (field != null) field.SetTextWithoutNotify(clamped.ToString("0.##"));
		// 	};
		//
		// 	// If a composite number field prefab is provided, use it directly
		// 	if (_activePreset.numberFieldPrefab != null)
		// 	{
		// 		var composite = InstantiateControl(_activePreset.numberFieldPrefab, row, "NumberField");
		// 		var compositeLE = composite.GetComponent<LayoutElement>() ?? composite.gameObject.AddComponent<LayoutElement>();
		// 		compositeLE.flexibleWidth = 1f;
		// 		compositeLE.preferredHeight = Mathf.Max(compositeLE.preferredHeight, 28f);
		//
		// 		var compositeInput = composite.GetComponentInChildren<TMP_InputField>(true);
		// 		Button compositeDecBtn = null;
		// 		Button compositeIncBtn = null;
		//
		// 		// Try to find buttons by common names/text markers
		// 		var buttons = composite.GetComponentsInChildren<Button>(true);
		// 		foreach (var b in buttons)
		// 		{
		// 			if (compositeDecBtn == null)
		// 			{
		// 				if (b.name.IndexOf("dec", StringComparison.OrdinalIgnoreCase) >= 0 ||
		// 				    b.name.IndexOf("minus", StringComparison.OrdinalIgnoreCase) >= 0)
		// 				{
		// 					compositeDecBtn = b;
		// 				}
		// 				else
		// 				{
		// 					var t = b.GetComponentInChildren<TextMeshProUGUI>();
		// 					if (t != null && t.text.Trim() == "<") compositeDecBtn = b;
		// 				}
		// 			}
		// 			if (compositeIncBtn == null)
		// 			{
		// 				if (b.name.IndexOf("inc", StringComparison.OrdinalIgnoreCase) >= 0 ||
		// 				    b.name.IndexOf("plus", StringComparison.OrdinalIgnoreCase) >= 0)
		// 				{
		// 					compositeIncBtn = b;
		// 				}
		// 				else
		// 				{
		// 					var t = b.GetComponentInChildren<TextMeshProUGUI>();
		// 					if (t != null && t.text.Trim() == ">") compositeIncBtn = b;
		// 				}
		// 			}
		// 		}
		//
		// 		// Fallback: if still missing, try assign by order
		// 		if (buttons != null && buttons.Length > 0)
		// 		{
		// 			if (compositeDecBtn == null) compositeDecBtn = buttons[0];
		// 			if (compositeIncBtn == null && buttons.Length > 1) compositeIncBtn = buttons[1];
		// 		}
		//
		// 		// Ensure input field exists in composite, otherwise create a simple one
		// 		if (compositeInput == null)
		// 		{
		// 			var compositeInputRT = InstantiateControl(_activePreset.inputFieldPrefab, composite, "InputField");
		// 			var compositeInputBg = compositeInputRT.GetComponent<Image>() ?? compositeInputRT.gameObject.AddComponent<Image>();
		// 			if (_activePreset.inputFieldPrefab == null)
		// 			{
		// 				compositeInputBg.color = new Color(1f, 1f, 1f, 0.1f);
		// 			}
		// 			compositeInput = compositeInputRT.GetComponent<TMP_InputField>() ?? compositeInputRT.gameObject.AddComponent<TMP_InputField>();
		// 			if (compositeInput.textComponent == null)
		// 			{
		// 				var textGO = CreateUIObject("Text", compositeInputRT);
		// 				var text = textGO.gameObject.AddComponent<TextMeshProUGUI>();
		// 				text.color = Color.white;
		// 				text.alignment = TextAlignmentOptions.Center;
		// 				compositeInput.textComponent = text;
		// 				StretchToFill(text.rectTransform);
		// 			}
		// 			compositeInput.contentType = TMP_InputField.ContentType.DecimalNumber;
		// 			compositeInput.lineType = TMP_InputField.LineType.SingleLine;
		// 		}
		//
		// 		UpdateDisplay(compositeInput, current, false);
		//
		// 		if (compositeDecBtn != null) compositeDecBtn.onClick.AddListener(() => UpdateDisplay(compositeInput, current - Mathf.Abs(step), true));
		// 		if (compositeIncBtn != null) compositeIncBtn.onClick.AddListener(() => UpdateDisplay(compositeInput, current + Mathf.Abs(step), true));
		// 		if (compositeInput != null)
		// 		{
		// 			compositeInput.onEndEdit.AddListener(str =>
		// 			{
		// 				if (float.TryParse(str, out var parsed))
		// 				{
		// 					UpdateDisplay(compositeInput, parsed, true);
		// 				}
		// 				else
		// 				{
		// 					UpdateDisplay(compositeInput, current, false);
		// 				}
		// 			});
		// 		}
		//
		// 		StretchToFill(row);
		// 		return;
		// 	}
		//
		// 	// Decrement Button
		// 	var decPrefab = _activePreset.decrementButtonPrefab != null
		// 		? _activePreset.decrementButtonPrefab
		// 		: _activePreset.buttonPrefab;
		// 	var decRT = InstantiateControl(decPrefab, row, "Dec");
		// 	var decBtn = decRT.GetComponent<Button>() ?? decRT.gameObject.AddComponent<Button>();
		// 	var decImg = decRT.GetComponent<Image>() ?? decRT.gameObject.AddComponent<Image>();
		// 	if (decPrefab == null)
		// 	{
		// 		decImg.color = new Color(1f, 1f, 1f, 0.15f);
		// 	}
		// 	var decText = decRT.GetComponentInChildren<TextMeshProUGUI>();
		// 	if (decText == null)
		// 	{
		// 		var t = CreateUIObject("Label", decRT);
		// 		decText = t.gameObject.AddComponent<TextMeshProUGUI>();
		// 		decText.alignment = TextAlignmentOptions.Center;
		// 		decText.color = Color.white;
		// 		StretchToFill(decText.rectTransform);
		// 	}
		// 	decText.text = "<";
		// 	var decLE = decRT.GetComponent<LayoutElement>() ?? decRT.gameObject.AddComponent<LayoutElement>();
		// 	decLE.minWidth = 32f;
		// 	decLE.preferredHeight = Mathf.Max(decLE.preferredHeight, 28f);
		//
		// 	// Input
		// 	var numberInputPrefab = _activePreset.numberInputFieldPrefab != null
		// 		? _activePreset.numberInputFieldPrefab
		// 		: _activePreset.inputFieldPrefab;
		// 	var inputRT = InstantiateControl(numberInputPrefab, row, "InputField");
		// 	var inputBg = inputRT.GetComponent<Image>() ?? inputRT.gameObject.AddComponent<Image>();
		// 	if (numberInputPrefab == null)
		// 	{
		// 		inputBg.color = new Color(1f, 1f, 1f, 0.1f);
		// 	}
		// 	var input = inputRT.GetComponent<TMP_InputField>() ?? inputRT.gameObject.AddComponent<TMP_InputField>();
		// 	if (input.textComponent == null)
		// 	{
		// 		var textGO = CreateUIObject("Text", inputRT);
		// 		var text = textGO.gameObject.AddComponent<TextMeshProUGUI>();
		// 		text.color = Color.white;
		// 		text.alignment = TextAlignmentOptions.Center;
		// 		input.textComponent = text;
		// 		StretchToFill(text.rectTransform);
		// 	}
		// 	input.contentType = TMP_InputField.ContentType.DecimalNumber;
		// 	input.lineType = TMP_InputField.LineType.SingleLine;
		// 	var inputLE = inputRT.GetComponent<LayoutElement>() ?? inputRT.gameObject.AddComponent<LayoutElement>();
		// 	inputLE.flexibleWidth = 1f;
		// 	inputLE.preferredHeight = Mathf.Max(inputLE.preferredHeight, 28f);
		//
		// 	// Increment Button
		// 	var incPrefab = _activePreset.incrementButtonPrefab != null
		// 		? _activePreset.incrementButtonPrefab
		// 		: _activePreset.buttonPrefab;
		// 	var incRT = InstantiateControl(incPrefab, row, "Inc");
		// 	var incBtn = incRT.GetComponent<Button>() ?? incRT.gameObject.AddComponent<Button>();
		// 	var incImg = incRT.GetComponent<Image>() ?? incRT.gameObject.AddComponent<Image>();
		// 	if (incPrefab == null)
		// 	{
		// 		incImg.color = new Color(1f, 1f, 1f, 0.15f);
		// 	}
		// 	var incText = incRT.GetComponentInChildren<TextMeshProUGUI>();
		// 	if (incText == null)
		// 	{
		// 		var t = CreateUIObject("Label", incRT);
		// 		incText = t.gameObject.AddComponent<TextMeshProUGUI>();
		// 		incText.alignment = TextAlignmentOptions.Center;
		// 		incText.color = Color.white;
		// 		StretchToFill(incText.rectTransform);
		// 	}
		// 	incText.text = ">";
		// 	var incLE = incRT.GetComponent<LayoutElement>() ?? incRT.gameObject.AddComponent<LayoutElement>();
		// 	incLE.minWidth = 32f;
		// 	incLE.preferredHeight = Mathf.Max(incLE.preferredHeight, 28f);
		//
		// 	UpdateDisplay(input, current, false);
		//
		// 	decBtn.onClick.AddListener(() => UpdateDisplay(input, current - Mathf.Abs(step), true));
		// 	incBtn.onClick.AddListener(() => UpdateDisplay(input, current + Mathf.Abs(step), true));
		// 	input.onEndEdit.AddListener(str =>
		// 	{
		// 		if (float.TryParse(str, out var parsed))
		// 		{
		// 			UpdateDisplay(input, parsed, true);
		// 		}
		// 		else
		// 		{
		// 			UpdateDisplay(input, current, false);
		// 		}
		// 	});
		//
		// 	StretchToFill(row);
		// }

		// Helpers
		private static RectTransform EnsureRootContainer(Canvas canvas)
		{
			// Root parent under canvas for all cheats (with ScrollRect)
			var existing = canvas.transform.Find("CheatRoot") as RectTransform;
			if (existing != null) return existing;

			// Ensure EventSystem exists
			EnsureEventSystem();

			// Root object
			var root = CreateUIObject("CheatRoot", canvas.transform as RectTransform);
			var rootRT = root;
			StretchToFill(rootRT);

			// ScrollView
			var scrollView = CreateUIObject("ScrollView", rootRT);
			var scrollRect = scrollView.gameObject.AddComponent<ScrollRect>();
			var scrollImage = scrollView.gameObject.AddComponent<Image>();
			scrollImage.color = new Color(0f, 0f, 0f, 0.25f);

			// Viewport
			var viewport = CreateUIObject("Viewport", scrollView);
			var maskImage = viewport.gameObject.AddComponent<Image>();
			maskImage.color = new Color(0f, 0f, 0f, 0.01f);
			viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

			// Content
			var content = CreateUIObject("Content", viewport);
			var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
			vlg.padding = new RectOffset(12, 12, 12, 12);
			vlg.spacing = 12f;
			vlg.childControlWidth = true;
			vlg.childControlHeight = true;
			vlg.childForceExpandHeight = false;
			vlg.childForceExpandWidth = true;

			var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			StretchToFill(scrollView);
			StretchToFill(viewport);
			scrollRect.viewport = viewport;
			scrollRect.content = content;
			scrollRect.horizontal = false;

			return content;
		}

		private static void EnsureEventSystem()
		{
			if (EventSystem.current != null) return;
			new GameObject("EventSystem",
				typeof(EventSystem),
				typeof(StandaloneInputModule));
		}

		private static RectTransform CreateHeader(string title, RectTransform parent)
		{
			var header = CreateUIObject("Header", parent);
			var text = header.gameObject.AddComponent<TextMeshProUGUI>();
			text.text = title;
			text.fontSize = 18;
			text.alignment = TextAlignmentOptions.MidlineLeft;
			text.color = Color.yellow;
			var le = header.gameObject.AddComponent<LayoutElement>();
			le.preferredHeight = 28f;
			StretchToFill(header);
			return header;
		}

		private RectTransform CreateRow(string name)
		{
			var row = CreateUIObject(name, _categoryRoot);
			var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
			layout.spacing = 6f;
			layout.childAlignment = TextAnchor.MiddleLeft;
			layout.childControlHeight = true;
			layout.childControlWidth = true;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = true;
			var le = row.gameObject.AddComponent<LayoutElement>();
			le.minHeight = 28f;
			return row;
		}

		private static RectTransform InstantiateControl(GameObject prefab, RectTransform parent, string name)
		{
			if (prefab != null)
			{
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

			return CreateUIObject(name, parent);
		}

		private static RectTransform CreateUIObject(string name, RectTransform parent)
		{
			var go = new GameObject(name, typeof(RectTransform));
			var rt = go.GetComponent<RectTransform>();
			rt.SetParent(parent, false);
			rt.localScale = Vector3.one;
			rt.localPosition = Vector3.zero;
			rt.localRotation = Quaternion.identity;
			rt.anchorMin = new Vector2(0f, 1f);
			rt.anchorMax = new Vector2(1f, 1f);
			rt.pivot = new Vector2(0.5f, 1f);
			rt.sizeDelta = new Vector2(0f, 0f);
			return rt;
		}

		private static void StretchToFill(RectTransform rt)
		{
			rt.anchorMin = new Vector2(0f, 0f);
			rt.anchorMax = new Vector2(1f, 1f);
			rt.pivot = new Vector2(0.5f, 0.5f);
			rt.offsetMin = Vector2.zero;
			rt.offsetMax = Vector2.zero;
		}
	}
}


