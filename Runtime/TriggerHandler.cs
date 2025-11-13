using UnityEngine;


namespace THEBADDEST.GameDebugSystem
{


	[System.Flags]
	public enum CornerFlags
	{

		TopLeft = 1,
		TopRight = 2,
		BottomLeft = 4,
		BottomRight = 8

	}

	/// <summary>
	/// Handles input triggers for showing/hiding the debug UI, such as corner tap detection.
	/// </summary>
	public sealed class TriggerHandler : MonoBehaviour
	{

		private DebugService _debugService;
		private int _requiredTapCount;
		private float _tapTimeout;
		private float _cornerAreaSize;
		private CornerFlags _openCorners;
		private CornerFlags _closeCorners;
		private bool _enableKeyboardToggle;
		private KeyCode _toggleKey1;
		private KeyCode _toggleKey2;


		private int _currentTapCount;
		private float _lastTapTime;
		private bool _isInitialized;

		public void Initialize(DebugService debugService, int tapCount, float timeout, float cornerAreaSize, CornerFlags openCorners, CornerFlags closeCorners, bool enableKeyboardToggle, KeyCode key1, KeyCode key2)
		{
			_debugService = debugService;
			_requiredTapCount = tapCount;
			_tapTimeout = timeout;
			_cornerAreaSize = Mathf.Clamp01(cornerAreaSize);
			_openCorners = openCorners;
			_closeCorners = closeCorners;
			_enableKeyboardToggle = enableKeyboardToggle;
			_toggleKey1 = key1;
			_toggleKey2 = key2;
			_currentTapCount = 0;
			_lastTapTime = 0f;
			_isInitialized = true;
		}

		private void Update()
		{
			if (!_isInitialized || _debugService == null) return;
			#if UNITY_EDITOR
			if (_enableKeyboardToggle && (Input.GetKeyDown(_toggleKey1) || Input.GetKeyDown(_toggleKey2)))
			{
				_debugService.ToggleVisibility();
				return;
			}
			#endif
			
			// Check for timeout
			if (Time.time - _lastTapTime > _tapTimeout)
			{
				_currentTapCount = 0;
			}

			// Detect taps/touches
			if (Input.GetMouseButtonDown(0))
			{
				HandleTap(Input.mousePosition);
			}

			// Handle mobile touches
			if (Input.touchCount > 0)
			{
				var touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began)
				{
					HandleTap(touch.position);
				}
			}
		}

		private void HandleTap(Vector2 screenPosition)
		{
			var corner = GetCornerAtPosition(screenPosition);
			if (corner == 0)
			{
				_currentTapCount = 0;
				return;
			}

			if (_debugService.IsVisible)
			{
				if ((_closeCorners & corner) != 0)
				{
					_debugService.ToggleVisibility();
					_currentTapCount = 0;
					return;
				}

				_currentTapCount = 0;
				return;
			}

			// Reset count if timeout exceeded
			if (Time.time - _lastTapTime > _tapTimeout)
			{
				_currentTapCount = 0;
			}

			if ((_openCorners & corner) == 0) return;
			_currentTapCount++;
			_lastTapTime = Time.time;

			// Check if required tap count reached
			if (_currentTapCount >= _requiredTapCount)
			{
				_debugService.ToggleVisibility();
				_currentTapCount = 0;
			}
		}

		private CornerFlags GetCornerAtPosition(Vector2 screenPosition)
		{
			var screenWidth = Screen.width;
			var screenHeight = Screen.height;
			var cornerSize = new Vector2(screenWidth * _cornerAreaSize, screenHeight * _cornerAreaSize);
			CornerFlags c = 0;
			if (screenPosition.x <= cornerSize.x && screenPosition.y >= screenHeight - cornerSize.y) c |= CornerFlags.TopLeft;
			if (screenPosition.x >= screenWidth - cornerSize.x && screenPosition.y >= screenHeight - cornerSize.y) c |= CornerFlags.TopRight;
			if (screenPosition.x <= cornerSize.x && screenPosition.y <= cornerSize.y) c |= CornerFlags.BottomLeft;
			if (screenPosition.x >= screenWidth - cornerSize.x && screenPosition.y <= cornerSize.y) c |= CornerFlags.BottomRight;
			return c;
		}

	}


}