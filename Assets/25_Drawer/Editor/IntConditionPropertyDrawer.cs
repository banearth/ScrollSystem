using UnityEngine;
using UnityEditor;

namespace BanSupport
{
	[CustomPropertyDrawer(typeof(IntConditionAttribute))]
	public class IntConditionPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			IntConditionAttribute condHAtt = (IntConditionAttribute)attribute;
			bool enabled = IsShow(condHAtt, property);
			if (enabled)
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			IntConditionAttribute condHAtt = (IntConditionAttribute)attribute;
			if (IsShow(condHAtt, property))
			{
				return EditorGUI.GetPropertyHeight(property, label);
			}
			else
			{
				return -EditorGUIUtility.standardVerticalSpacing;
			}
		}

		private bool IsShow(IntConditionAttribute conditionAttribute, SerializedProperty property)
		{
			bool enabled = true;
			string propertyPath = property.propertyPath;
			string conditionPath = propertyPath.Replace(property.name, conditionAttribute.intField);
			SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
			if (sourcePropertyValue != null)
			{
				enabled = sourcePropertyValue.intValue == conditionAttribute.expertValue;
			}
			else
			{
				Debug.LogWarning("Attempting to use a ConditionAttribute but no matching SourcePropertyValue found in object: " + conditionAttribute.intField);
			}
			return enabled;
		}
	}
}