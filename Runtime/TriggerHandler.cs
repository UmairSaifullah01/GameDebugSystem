using UnityEngine;

namespace THEBADDEST.GameDebugSystem
{
	/// <summary>
	/// Handles input triggers for showing/hiding the debug UI, such as corner tap detection.
	/// </summary>
	public sealed class TriggerHandler : MonoBehaviour
	{
		private DebugService _debugService;
		private int _requiredTapCount;
		private float _tapTimeout;
		private float _cornerAreaSize;
		
		private int _currentTapCount;
		private float _lastTapTime;
		private bool _isInitialized;

		public void Initialize(DebugService debugService, int tapCount, float timeout, float cornerAreaSize)
		{
			_debugService = debugService;
			_requiredTapCount = tapCount;
			_tapTimeout = timeout;
			_cornerAreaSize = Mathf.Clamp01(cornerAreaSize);
			_currentTapCount = 0;
			_lastTapTime = 0f;
			_isInitialized = true;
		}

		private void Update()
		{
			if (!_isInitialized || _debugService == null) return;

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
			if (!IsInCornerArea(screenPosition))
			{
				_currentTapCount = 0;
				return;
			}

			// Reset count if timeout exceeded
			if (Time.time - _lastTapTime > _tapTimeout)
			{
				_currentTapCount = 0;
			}

			_currentTapCount++;
			_lastTapTime = Time.time;

			// Check if required tap count reached
			if (_currentTapCount >= _requiredTapCount)
			{
				_debugService.ToggleVisibility();
				_currentTapCount = 0;
			}
		}

		private bool IsInCornerArea(Vector2 screenPosition)
		{
			var screenWidth = Screen.width;
			var screenHeight = Screen.height;
			var cornerSize = new Vector2(screenWidth * _cornerAreaSize, screenHeight * _cornerAreaSize);

			// Check top-left corner
			if (screenPosition.x <= cornerSize.x && screenPosition.y >= screenHeight - cornerSize.y)
			{
				return true;
			}

			// Check top-right corner
			if (screenPosition.x >= screenWidth - cornerSize.x && screenPosition.y >= screenHeight - cornerSize.y)
			{
				return true;
			}

			// Check bottom-left corner
			if (screenPosition.x <= cornerSize.x && screenPosition.y <= cornerSize.y)
			{
				return true;
			}

			// Check bottom-right corner
			if (screenPosition.x >= screenWidth - cornerSize.x && screenPosition.y <= cornerSize.y)
			{
				return true;
			}

			return false;
		}
	}

}

