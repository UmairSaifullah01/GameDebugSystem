# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2024-01-XX

### Added

- **Show/Hide Functionality**: Added `Show()`, `Hide()`, and `ToggleVisibility()` methods to `DebugService` for programmatic control
- **Trigger Options**: Added `TriggerType` enum with options for UIButton, CornerTap, or None
- **UI Button Trigger**: Toggle button in the top-right corner to show/hide debug UI
- **Corner Tap Detection**: Tap detection system that opens debug UI when tapping screen corners multiple times
- **Corner Tap Configuration**: Configurable tap count, timeout, and corner area size for corner tap detection
- **Value Synchronization**: Added optional getter parameters (`Func<T>`) to all `Add` methods:
  - `AddSlider`: Optional `Func<float> getter` to sync slider with current value
  - `AddInputField`: Optional `Func<string> getter` to set initial input value
  - `AddNumberField`: Optional `Func<float> getter` to sync number field with current value
  - `AddToggle`: Optional `Func<bool> getter` to sync toggle with current state
- **Custom Editor**: Created `DebugServiceEditor` for conditional visibility of corner tap settings in Unity Inspector
- **Hide At Start Option**: Option to hide debug UI when initialized
- **Canvas Persistence**: Canvas persists across scene loads using `DontDestroyOnLoad`

### Changed

- **Performance Optimization**: Replaced costly `FindObjectOfType` calls with direct component access from canvas
- **Visibility Logic**: Updated visibility system to hide/show Root content while keeping canvas active for triggers
- **API Enhancement**: All `Add` methods now support optional getter parameters for value synchronization

### Technical Details

- Trigger handler component (`TriggerHandler`) can be added to canvas prefab for better performance
- Corner tap detection works with both mouse clicks and touch input
- UI button is always accessible when UIButton trigger type is selected
- Canvas remains active to support trigger detection even when UI is hidden

## [1.0.0] - 2024-01-XX

### Added

- Initial release of Game Debug System
- `DebugService` ScriptableObject for managing debug UI
- `DebugUIBuilder` class for building debug UI controls
- `DebugWrapperBase` abstract class for creating custom debug wrappers
- `IDebugWrapper` interface for debug wrapper implementations
- `GameplayDebugWrapper` example implementation
- Support for buttons, sliders, input fields, number fields, and toggles
- UI preset system for customizing debug UI appearance
- Automatic canvas creation and configuration
- Category-based organization of debug controls
- Error handling and logging throughout the system
- Null checks for UI components to prevent runtime errors
- Example prefabs for UI controls (Button, Slider, Input, NumberInput, ToggleInput, Category, Canvas)

### Features

- Customizable UI builder with prefab support
- ScriptableObject-based configuration
- Category organization for debug controls
- Auto canvas setup with proper scaling and rendering
- TextMeshPro integration for text rendering
- Increment/decrement buttons for number fields
- Value display for sliders
- Placeholder text support for input fields

### Technical Details

- Unity 2020.3+ support
- TextMeshPro dependency
- ScreenSpaceOverlay canvas rendering
- Canvas sorting order set to 5000+ for proper overlay
- Reference resolution: 1920x1080 with 0.5 match width/height
- Runtime and editor mode support
