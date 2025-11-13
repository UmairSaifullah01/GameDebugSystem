using UnityEngine;
using UnityEditor;
using THEBADDEST.GameDebugSystem;

[CustomEditor(typeof(DebugService))]
public class DebugServiceEditor : Editor
{
	private SerializedProperty _hideAtStart;
	private SerializedProperty _triggerType;
	private SerializedProperty _cornerTapCount;
	private SerializedProperty _cornerTapTimeout;
	private SerializedProperty _cornerTapAreaSize;
	private SerializedProperty _openCorners;
	private SerializedProperty _closeCorners;
	private SerializedProperty _enableKeyboardToggle;
	private SerializedProperty _toggleKey1;
	private SerializedProperty _toggleKey2;

	private void OnEnable()
	{
		_hideAtStart = serializedObject.FindProperty("_hideAtStart");
		_triggerType = serializedObject.FindProperty("_triggerType");
		_cornerTapCount = serializedObject.FindProperty("_cornerTapCount");
		_cornerTapTimeout = serializedObject.FindProperty("_cornerTapTimeout");
		_cornerTapAreaSize = serializedObject.FindProperty("_cornerTapAreaSize");
		_openCorners = serializedObject.FindProperty("_openCorners");
		_closeCorners = serializedObject.FindProperty("_closeCorners");
		_enableKeyboardToggle = serializedObject.FindProperty("_enableKeyboardToggle");
		_toggleKey1 = serializedObject.FindProperty("_toggleKey1");
		_toggleKey2 = serializedObject.FindProperty("_toggleKey2");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		// Draw default properties except the ones we're handling custom
		DrawPropertiesExcluding(serializedObject, new string[] 
		{ 
			"_hideAtStart", 
			"_triggerType", 
			"_cornerTapCount", 
			"_cornerTapTimeout", 
			"_cornerTapAreaSize",
			"_openCorners",
			"_closeCorners",
			"_enableKeyboardToggle",
			"_toggleKey1",
			"_toggleKey2",
			"m_Script"
		});

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Trigger Options", EditorStyles.boldLabel);
		
		EditorGUILayout.PropertyField(_hideAtStart);
	EditorGUILayout.PropertyField(_triggerType);

	// Show corner tap settings only when CornerTap is selected
	if (_triggerType.enumValueIndex == (int)TriggerType.CornerTap)
	{
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Corner Tap Settings", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(_cornerTapCount);
		EditorGUILayout.PropertyField(_cornerTapTimeout);
		EditorGUILayout.PropertyField(_cornerTapAreaSize);
		EditorGUILayout.PropertyField(_openCorners);
		EditorGUILayout.PropertyField(_closeCorners);
	}

	EditorGUILayout.Space();
	EditorGUILayout.LabelField("Keyboard Toggle", EditorStyles.boldLabel);
	EditorGUILayout.PropertyField(_enableKeyboardToggle);
	EditorGUILayout.PropertyField(_toggleKey1);
	EditorGUILayout.PropertyField(_toggleKey2);

		serializedObject.ApplyModifiedProperties();
	}
}

