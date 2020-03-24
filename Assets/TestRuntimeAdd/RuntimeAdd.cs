using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeAdd : MonoBehaviour
{

	public Transform targetTrans;
	public Transform[] addedItems;

	private BanSupport.ScrollSystem mScrollSystem;

	void Awake()
	{
		mScrollSystem = GameObject.FindObjectOfType<BanSupport.ScrollSystem>();
		mScrollSystem.PreSetting(1, 0, false, new Vector2(1, 1), new Vector2(5, 5), -1, 0);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.A))
		{
			mScrollSystem.Add("Item", new object());
		}
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
