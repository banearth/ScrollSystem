using UnityEngine;
using UnityEditor;
namespace BanSupport
{
	[CustomPropertyDrawer(typeof(IntEnumAttribute))]
	public class IntEnumPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			IntEnumAttribute customAttribute = (IntEnumAttribute)attribute;
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			property.intValue = EditorGUI.IntPopup(position, property.intValue, customAttribute.enumStrs, customAttribute.intValues);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label);
		}

		
	}
}