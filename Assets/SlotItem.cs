using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;

public class SlotItem : MonoBehaviour
{
	// Start is called before the first frame update

	public Text empty;
	public Text consume;
	public Text equip;
	public Text collect;
	private GameObject[] all;

	void Awake()
	{
		all = new GameObject[] { empty.gameObject, consume.gameObject, equip.gameObject , collect.gameObject};
	}

	public void UpdateInfo(SlotData slotData)
	{
		switch (slotData.type) {
			case SlotData.Type.empty:
				all.Select(empty.gameObject);
				break;
			case SlotData.Type.consume:
				all.Select(consume.gameObject);
				break;
			case SlotData.Type.equip:
				all.Select(equip.gameObject);
				break;
			case SlotData.Type.Collect:
				all.Select(collect.gameObject);
				break;
		}
	}

}

[System.Serializable]
public class SlotData
{
	public enum Type { empty, consume, equip,Collect };
	public Type type = Type.empty;
}