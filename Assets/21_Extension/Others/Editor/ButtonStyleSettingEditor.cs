using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Graphs;

namespace BanSupport
{
	[CustomEditor(typeof(ButtonStyleSettting))]
	public class ButtonStyleSettingEditor : Editor
	{

		[MenuItem("CONTEXT/Button/Add ButtonStyleSetting")]
		static void AddButtonStyle(MenuCommand command)
		{
			Button aButton = (Button)command.context;
			Tools.AddComponent<ButtonStyleSettting>(aButton.gameObject);
		}

		[MenuItem("CONTEXT/Graphic/Add ButtonStyleSetting")]
		static void AddGraphicStyle(MenuCommand command)
		{
			Button aButton = (Button)command.context;
			Tools.AddComponent<ButtonStyleSettting>(aButton.gameObject);
		}

		private ButtonStyleSettting origin
		{
			get
			{
				return target as ButtonStyleSettting;
			}
		}

		private string newStyleName = "Default";

		public override void OnInspectorGUI()
		{

			var stylesProperty = serializedObject.FindProperty("styles");
			for (int i = 0; i < origin.styles.Count; i++)
			{
				var curProperty = stylesProperty.GetArrayElementAtIndex(i);
				var curStyle = origin.styles[i];
				HandleStyleProperty(curProperty, curStyle, i);
			}

			//GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			newStyleName = EditorGUILayout.TextField("New Style Name:", newStyleName);
			if (GUILayout.Button("Add New Style"))
			{

				if (string.IsNullOrEmpty(newStyleName))
				{
					EditorUtility.DisplayDialog("Error", "StyleName can not be empty", "Ok");
					return;
				}

				if (origin.styles.Contains(temp => temp.styleName == newStyleName))
				{
					EditorUtility.DisplayDialog("Error", string.Format("this is a ButtonStyle with the same name:'{0}'", newStyleName), "Ok");
					return;
				}

				origin.styles.Add(GenerateOriginStyle());
				newStyleName = "";
				EditorUtility.SetDirty(target);
			}
			GUILayout.EndHorizontal();

			serializedObject.ApplyModifiedProperties();
		}

		private ButtonStyle GenerateOriginStyle()
		{
			var style = new ButtonStyle { styleName = newStyleName };
			var image = origin.GetComponent<Image>();
			var label = origin.GetComponentInChildren<Text>();
			if (label != null)
			{
				style.textColor = label.color;
			}
			if (image != null)
			{
				style.imageColor = image.color;
				style.imageMaterial = image.material;
				style.imageSprite = image.sprite;
			}
			return style;
		}

		private void HandleStyleProperty(SerializedProperty serializedProperty, ButtonStyle style,int index)
		{

			serializedProperty.isExpanded = EditorGUILayout.Foldout(serializedProperty.isExpanded, "Element " + index + ": (" + style.styleName+")");

			if (!serializedProperty.isExpanded)
			{
				return;
			}

			EditorGUI.indentLevel++;
			
			PropertyField(serializedProperty, "styleName");

			//text相关
			PropertyField(serializedProperty, "textColorEnable");
			if (style.textColorEnable)
			{
				PropertyField(serializedProperty, "textColor");
			}
			//image相关
			PropertyField(serializedProperty, "imageChangeType");
			switch (style.imageChangeType)
			{
				case ButtonStyle.ImageChangeType.Color:
					PropertyField(serializedProperty, "imageColor");
					break;
				case ButtonStyle.ImageChangeType.Material:
					PropertyField(serializedProperty, "imageMaterial");
					break;
				case ButtonStyle.ImageChangeType.Sprite:
					PropertyField(serializedProperty, "imageSprite");
					break;
			}
			EditorGUI.indentLevel--;

			GUILayout.BeginHorizontal("Box");
			GUILayout.Label("Operation:");
			GUILayout.Label("");
			GUILayout.Label("");
			//Load
			if (GUILayout.Button("Load", GUILayout.MinWidth(50)))
			{
				var newStyle = GenerateOriginStyle();
				newStyle.styleName = style.styleName;
				newStyle.imageChangeType = style.imageChangeType;
				newStyle.textColorEnable = style.textColorEnable;
				origin.styles[index] = newStyle;
				EditorUtility.SetDirty(target);
			}
			//Apply
			if (GUILayout.Button("Appy", GUILayout.MinWidth(50)))
			{
				var image = origin.GetComponent<Image>();
				var label = origin.GetComponentInChildren<Text>();
				if (label != null)
				{
					if (style.textColorEnable)
					{
						label.color = style.textColor;
					}
				}
				if (image != null)
				{
					switch (style.imageChangeType)
					{
						case ButtonStyle.ImageChangeType.Color:
							image.color = style.imageColor;
							break;
						case ButtonStyle.ImageChangeType.Material:
							image.material = style.imageMaterial;
							break;
						case ButtonStyle.ImageChangeType.Sprite:
							image.sprite = style.imageSprite;
							if (image.type == Image.Type.Simple)
							{
								image.SetNativeSize();
							}
							break;
					}
				}
				EditorUtility.SetDirty(target);
			}
			//Delete
			if (GUILayout.Button("Delete", GUILayout.MinWidth(50)))
			{
				origin.styles.RemoveAt(index);
				EditorUtility.SetDirty(target);
			}
			GUILayout.EndHorizontal();

		}

		private void PropertyField(SerializedProperty serializedProperty, string name)
		{
			var findProperty = serializedProperty.FindPropertyRelative(name);
			EditorGUILayout.PropertyField(findProperty);
		}

	}
}