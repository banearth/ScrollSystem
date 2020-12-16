using UnityEngine;
using System;
using System.Collections;

namespace BanSupport
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
		AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
	public class BoolConditionAttribute : PropertyAttribute
	{

		public string boolField = "";

		public BoolConditionAttribute(string boolField)
		{
			this.boolField = boolField;
		}

	}
}