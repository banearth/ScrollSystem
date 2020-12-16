using UnityEngine;
using System;
using System.Collections;

namespace BanSupport
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
		AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
	public class IntEnumAttribute : PropertyAttribute
	{

		public string[] enumStrs;
		public int[] intValues;

		/// <summary>
		/// Example: 1,2,3,"TypeA","TypeB","TypeC"
		/// </summary>
		public IntEnumAttribute(params object[] objects)
		{
			if (objects.Length % 2 != 0)
			{
				Debug.Log("IntEnumAttribute use objects by pairs");
				return;
			}
			var half = objects.Length / 2;
			intValues = new int[half];
			enumStrs = new string[half];
			for (int i = 0; i < half; i++)
			{
				intValues[i] = (int)objects[i];
				enumStrs[i] = (string)objects[i + half];
			}
		}

	}
}


