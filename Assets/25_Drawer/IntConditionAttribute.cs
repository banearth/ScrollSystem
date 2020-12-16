using UnityEngine;
using System;
using System.Collections;

namespace BanSupport
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
		AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
	public class IntConditionAttribute : PropertyAttribute
	{

		public string intField = "";
		public int expertValue;

		public IntConditionAttribute(string intField,int expertValue)
		{
			this.intField = intField;
			this.expertValue = expertValue;
		}

	}
}