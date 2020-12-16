using UnityEngine;
using UnityEditor;

namespace BanSupport
{
	[CustomPropertyDrawer(typeof(BoolConditionAttribute))]
	public class BoolConditionPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			BoolConditionAttribute condHAtt = (BoolConditionAttribute)attribute;
			bool enabled = IsShow(condHAtt, property);
			if (enabled)
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			BoolConditionAttribute condHAtt = (BoolConditionAttribute)attribute;
			if (IsShow(condHAtt, property))
			{
				return EditorGUI.GetPropertyHeight(property, label);
			}
			else
			{
				return -EditorGUIUtility.standardVerticalSpacing;
			}
		}

		private bool IsShow(BoolConditionAttribute conditionAttribute, SerializedProperty property)
		{
			bool enabled = true;
			string propertyPath = property.propertyPath;
			string conditionPath = propertyPath.Replace(property.name, conditionAttribute.boolField);
			SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
			if (sourcePropertyValue != null)
			{
				enabled = sourcePropertyValue.boolValue;
			}
			else
			{
				Debug.LogWarning("Attempting to use a ConditionAttribute but no matching SourcePropertyValue found in object: " + conditionAttribute.boolField);
			}

			return enabled;
		}
	}
}