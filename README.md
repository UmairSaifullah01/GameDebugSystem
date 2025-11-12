# Game Debug System

**Version 1.0.1**

A flexible Unity debug and cheat system that provides a customizable UI builder for creating debug panels with buttons, sliders, input fields, number fields, and toggles. Organize debug controls into categories using ScriptableObject-based wrappers.

## Features

- **Customizable UI Builder**: Create debug UI panels with buttons, sliders, input fields, number fields, and toggles
- **Category Organization**: Organize debug controls into logical categories
- **ScriptableObject-based**: Use ScriptableObjects for easy configuration and asset management
- **Prefab Support**: Assign custom UI prefabs for complete visual customization
- **Auto Canvas Creation**: Automatically creates and configures overlay canvases for debug UI
- **Show/Hide Functionality**: Toggle debug UI visibility programmatically or via triggers
- **Multiple Trigger Options**: Choose between UI Button, Corner Tap, or None for showing/hiding the debug UI
- **Value Synchronization**: Getter functions to sync UI controls with current game state values
- **Error Handling**: Robust error handling with detailed logging

## Requirements

- Unity 2020.3 or higher
- TextMeshPro (com.unity.textmeshpro)

## Installation

1. Add this package to your Unity project via Package Manager or by adding it as a git submodule
2. Ensure TextMeshPro is installed in your project
3. Create UI prefabs for buttons, sliders, inputs, and other controls (see Presets folder for examples)

## Quick Start

### 1. Create a Debug Service

1. Right-click in your Project window
2. Select `Create > Cheats > Cheat Service`
3. This creates a `DebugService` ScriptableObject asset

### 2. Create Debug Wrappers

Create ScriptableObject wrappers that define your debug controls:

1. Right-click in your Project window
2. Select `Create > Cheats > Gameplay Wrapper` (or create a custom wrapper)
3. Implement the `RegisterCheats` method to add your debug controls

### 3. Configure UI Presets and Trigger Options

In your `DebugService` asset:

- Assign UI prefabs (Canvas, Category, Button, Slider, Input, NumberInput, ToggleInput)
- Add your debug wrappers to the Wrappers list
- Configure trigger options:
  - **Hide At Start**: Hide the debug UI when initialized
  - **Trigger Type**: Choose how to show/hide the UI (UIButton, CornerTap, or None)
  - **Corner Tap Settings**: Configure corner tap detection (tap count, timeout, area size) when CornerTap is selected

### 4. Initialize the Debug Service

Call `Init()` on your `DebugService` instance:

```csharp
using THEBADDEST.GameDebugSystem;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private DebugService debugService;

    void Start()
    {
        if (debugService != null)
        {
            debugService.Init();
        }
    }
}
```

## Creating Custom Debug Wrappers

Create a custom debug wrapper by extending `DebugWrapperBase`:

```csharp
using THEBADDEST.GameDebugSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "MyDebugWrapper", menuName = "Cheats/My Debug Wrapper", order = 10)]
public class MyDebugWrapper : DebugWrapperBase
{
    public override void RegisterCheats(DebugUIBuilder builder)
    {
        // Add your debug controls here
        builder.AddButton("My Button", () => {
            Debug.Log("Button clicked!");
        });
    }
}
```

## Example: GameplayDebugWrapper

Here's a complete example from the included `GameplayDebugWrapper`:

```csharp
using UnityEngine;

namespace THEBADDEST.GameDebugSystem
{
    [CreateAssetMenu(fileName = "GameplayCheatWrapper", menuName = "Cheats/Gameplay Wrapper", order = 10)]
    public class GameplayDebugWrapper : DebugWrapperBase
    {
        public override void RegisterCheats(DebugUIBuilder builder)
        {
            // Example button
            builder.AddButton("Give 100 Gold", () =>
            {
                Debug.Log("[Cheats] Granted 100 gold.");
                // GameSystems.Inventory.AddGold(100);
            });

            // Example slider with getter to sync with current value
            builder.AddSlider("Player Speed", 1f, 10f, 3f, (v) =>
            {
                Debug.Log($"[Cheats] Player speed -> {v:0.##}");
                // PlayerController.Instance.SetSpeed(v);
            }, () => PlayerController.Instance.GetSpeed()); // Getter to sync with current value

            // Example input field
            builder.AddInputField("Teleport To", "x,y,z", (pos) =>
            {
                Debug.Log($"[Cheats] Teleport to -> {pos}");
                // Parse and teleport: var p = Vector3Parser.Parse(pos); Player.Teleport(p);
            });

            // Example number field
            builder.AddNumberField("Speed", 0, 100, 1, 1, x =>
            {
                Debug.Log($"[Cheats] Speed -> {x}");
            });

            // Example toggle with getter to sync with current value
            builder.AddToggle("God Mode", false, (b) =>
            {
                Debug.Log($"God Mode : {b}");
                // PlayerController.Instance.SetGodMode(b);
            }, () => PlayerController.Instance.IsGodModeActive()); // Getter to sync with current value
        }
    }
}
```

## API Reference

### DebugService

The main service that manages debug UI creation and initialization.

#### Methods

- `Init()`: Builds or rebuilds the debug UI on a canvas
- `Show()`: Shows the debug UI
- `Hide()`: Hides the debug UI
- `ToggleVisibility()`: Toggles the visibility of the debug UI

#### Properties

- `UIPreset`: Gets the current UI preset configuration
- `IsVisible`: Gets whether the debug UI is currently visible

### DebugUIBuilder

Builder class for creating debug UI controls.

#### Static Methods

- `CreateCategory(string name, Canvas parentCanvas)`: Creates a category panel and returns a builder instance
- `ApplyUIPreset(UIPreset preset)`: Applies a UI preset configuration

#### Instance Methods

- `AddButton(string label, Action onClick)`: Adds a button with a label and click callback
- `AddSlider(string label, float min, float max, float defaultValue, Action<float> onChanged, Func<float> getter = null)`: Adds a slider with label and value changed callback. Optional getter to sync with current value.
- `AddInputField(string label, string placeholder, Action<string> onSubmit, Func<string> getter = null)`: Adds an input field with label, placeholder, and submit callback. Optional getter to set initial value.
- `AddNumberField(string label, float min, float max, float step, float defaultValue, Action<float> onChanged, Func<float> getter = null)`: Adds a numeric input with increment/decrement buttons. Optional getter to sync with current value.
- `AddToggle(string label, bool defaultValue, Action<bool> onChanged, Func<bool> getter = null)`: Adds a toggle with label and value changed callback. Optional getter to sync with current value.

### DebugWrapperBase

Base class for creating debug wrapper ScriptableObjects.

#### Properties

- `CategoryName`: Gets the category name for this wrapper (defaults to the asset name if not set)

#### Abstract Methods

- `RegisterCheats(DebugUIBuilder builder)`: Implement this method to register your debug controls

## UI Prefab Requirements

When creating custom UI prefabs, ensure they have the following components:

### Button Prefab

- `Button` component (in children is supported)
- `TextMeshProUGUI` component with name containing "Label" for the button text

### Slider Prefab

- `Slider` component (in children is supported)
- `TextMeshProUGUI` component with name containing "Label" for the label
- Optional: `TextMeshProUGUI` component with name containing "Value" for displaying the current value

### Input Field Prefab

- `TMP_InputField` component (in children is supported)
- `TextMeshProUGUI` component with name containing "Label" for the label
- Placeholder text support via TMP_InputField's placeholder

### Number Input Prefab

- `TMP_InputField` component (in children is supported)
- `TextMeshProUGUI` component with name containing "Label" for the label
- `Button` components with names containing "left", "dec", "minus" (for decrement) or "right", "inc", "plus" (for increment)

### Toggle Prefab

- `Toggle` component (in children is supported)
- `TextMeshProUGUI` component with name containing "Label" for the label

### Canvas Prefab

- Should contain a hierarchy: `Root/ScrollView/Viewport/Content` (or just `Root`)
- The Content container will hold all category panels

## License

See LICENSE file for details.

## Author

**Umair Saifullah**

- Website: https://www.umairsaifullah.com
- Email: contact@umairsaifullah.com
