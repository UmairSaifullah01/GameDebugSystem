# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
