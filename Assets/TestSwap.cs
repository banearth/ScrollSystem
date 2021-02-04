using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;

public class TestSwap : MonoBehaviour
{

	public ScrollSystem scrollSystem;
	public SlotData[] slotItems;
	public Button buttonClear;
	public Button buttonAdd;
	public Button buttonConsume;
	public Button buttonEquip;
	public Button buttonCollect;
	public int maxCount = 12;

	void Start()
	{
		scrollSystem.SetOnItemRefresh(UpdateInfo);

		buttonClear.onClick.AddListener(() =>
		{
			scrollSystem.Clear();
		});
		buttonAdd.onClick.AddListener(() =>
		{
			scrollSystem.Add("SlotItem", new SlotData { type = SlotData.Type.empty });
		});

		buttonConsume.onClick.AddListener(() =>
		{
			OnSelectType(SlotData.Type.consume);
		});

		buttonCollect.onClick.AddListener(() =>
		{
			OnSelectType(SlotData.Type.Collect);
		});

		buttonEquip.onClick.AddListener(() =>
		{
			OnSelectType(SlotData.Type.equip);
		});

		OnSelectType(SlotData.Type.consume);

	}

	private void OnSelectType(SlotData.Type type)
	{
		scrollSystem.Clear();
		foreach (var aSlotItem in slotItems)
		{
			if (aSlotItem.type == type)
			{
				scrollSystem.Add("SlotItem", aSlotItem);
			}
		}
		int emptyCount = maxCount - scrollSystem.GetCount();
		for (int i = 0; i < emptyCount; i++)
		{
			scrollSystem.Add("SlotItem", new SlotData { type = SlotData.Type.empty });
		}
	}

	private void UpdateInfo(string prefab, GameObject trans, object data)
	{
		switch (prefab)
		{
			case "SlotItem":
				trans.GetComponent<SlotItem>().UpdateInfo(data as SlotData);
				break;
		}
	}

}
