using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeAdd : MonoBehaviour
{

	public Transform targetTrans;
	public Transform[] addedItems;

	void Start()
	{

	}

	void Update()
	{

	}

	[ContextMenu("AddScrollSystem")]
	void AddScrollSystem()
	{
		var scrollsystem = BanSupport.ScrollSystem.Create(targetTrans);
		foreach (var aTrans in addedItems)
		{
			scrollsystem.AddInEditor(aTrans);
		}

	}

}
