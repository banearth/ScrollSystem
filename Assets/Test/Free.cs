using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;
using System;

public class Free : MonoBehaviour
{

	public Button button;
	public ScrollSystem scrollSystem;
	
	public Slider sliderJumpProgress;
	public Text textJumpProgress;

	public string[] prefabNames;
	public bool[] prefabSelected;

	
	public static Free Instance;

	public List<string> GetSelectedPrefabNames()
	{
		List<string> result = new List<string>();
		for (int i = 0; i < prefabSelected.Length; i++)
		{
			if (prefabSelected[i])
			{
				result.Add(prefabNames[i]);
			}
		}
		return result;
	}

	private void Awake()
	{
		Instance = this;
	}

	private List<SimpleData> deleteAndAddDatas = new List<SimpleData>();
	private List<SimpleData> createdDatas = new List<SimpleData>();

	void Start()
	{
		for (int i = 0; i < 100; i++)
		{
			deleteAndAddDatas.Add(new SimpleData { index = Example.global_index++ });
		}

		BindEvent();

		scrollSystem.SetOnItemRefresh((prefabName, root, data) =>
		{
			//if (useOpenCloseRefreshEvent)
			//{
			//	Debug.Log(string.Format(" {0} Refresh id:{1}", prefabName, (data as SimpleData).index.ToString()));
			//}
			switch (prefabName)
			{
				case "A":
					{
						root.GetComponent<ItemA>().OnRefresh(data as SimpleData);
					}
					break;
				case "B":
					{
						root.GetComponent<ItemB>().OnRefresh(data as SimpleData);
					}
					break;
				case "C":
					{
						root.GetComponent<ItemC>().OnRefresh(data as SimpleData);
					}
					break;
				case "D":
					{
						root.GetComponent<ItemD>().OnRefresh(data as SimpleData);
					}
					break;
				case "Chat":
					{
						root.GetComponent<ItemChat>().OnRefresh(data as ChatData);
					}
					break;
			}
		});

		scrollSystem.SetOnItemClose((prefabName, root, data) =>
		{
			//if (useOpenCloseRefreshEvent)
			//{
			//	Debug.Log(string.Format(" {0} Close", prefabName));
			//}
		});

		scrollSystem.SetOnItemOpen((prefabName, root, data) =>
		{
			//if (useOpenCloseRefreshEvent)
			//{
			//	Debug.Log(string.Format(" {0} Open", prefabName));
			//}
		});

	}

	private void BindEvent()
	{
	}

	private int global_index = 0;

	public SimpleData GenerateSimpleData()
	{
		var returnData = new SimpleData { index = global_index++ };
		createdDatas.Add(returnData);
		return returnData;
	}

	private Vector2 GetChatHeight(object data)
	{
		ScrollLayout referScrollLayout = scrollSystem.GetOriginPrefab("Chat").GetComponent<ScrollLayout>();
		var height = referScrollLayout.GetHeightByStr((data as ChatData).msg);
		return new Vector2(-1, height);
	}

}

public class FreeData
{
	public int index;
}