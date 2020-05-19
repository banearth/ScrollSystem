using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;
using System;

public class Example : MonoBehaviour
{

	public static int global_index = 0;

	public Button buttonDeleteAll;
	public Button buttonAddChat;
	public Button buttonAddChatJump;
	public Button buttonCreateDeleteAndAdd;
	public Button buttonJumpWithoutAnimation;
	public Button buttonReverse;
	public Button buttonRemoveFirst;
	public Button buttonRemoveLast;
	public Button ButtonRefresh;
	public Button ButtonChangeData;

	public InputField inputField_Number;
	public Button buttonAdd;
	public Button buttonCheck;
	public Button buttonCheckExcept;
	public Button buttonA;
	public Button buttonB;
	public Button buttonC;
	public Button buttonD;

	public string[] prefabNames;
	public bool[] prefabSelected;

	public ScrollSystem scrollSystem;
	public InputField inputField_ChatContent;
	public bool setWorldPosEnable = true;

	public static Example Instance;

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

	public int GetInputFieldCount()
	{
		return int.Parse(inputField_Number.text);
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

		BindButton();

		scrollSystem.SetOnItemRefresh((prefabName, root, data) =>
		{
			Debug.Log(string.Format(" {0} Refresh id:{1}", prefabName, (data as SimpleData).index.ToString()));
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
			Debug.Log(string.Format(" {0} Close", prefabName));
		});

		scrollSystem.SetOnitemOpen((prefabName, root, data) =>
		{
			Debug.Log(string.Format(" {0} Open", prefabName));
		});

		scrollSystem.SetOnBeginDrag(data =>
		{
			Debug.Log("OnBeginDrag");
		});

		scrollSystem.SetOnEndDrag(data =>
		{
			Debug.Log("OnEndDrag");
		});

		//scrollSystem.SetOnDrag(data =>
		//{
		//	Debug.Log("OnDrag");
		//});


	}

	private void BindButton()
	{
		Button[] buttons = new Button[] { buttonA, buttonB, buttonC, buttonD };
		for (int i = 0; i < buttons.Length; i++)
		{
			var curButton = buttons[i];
			var index = i;
			Action readAction = () =>
			{
				curButton.transform.Find("Mark").gameObject.SetActive(prefabSelected[index]);
			};
			curButton.onClick.AddListener(() =>
			{
				prefabSelected[index] = !prefabSelected[index];
				readAction();
			});
			readAction();
		}
		buttonAdd.onClick.AddListener(() =>
		{
			foreach (var aName in GetSelectedPrefabNames())
			{
				for (int i = 0; i < GetInputFieldCount(); i++)
				{
					scrollSystem.Add(aName, GenerateSimpleData());
				}
			}
		});
		buttonCheck.onClick.AddListener(() => { Debug.Log(scrollSystem.GetCount(GetSelectedPrefabNames().ToArray())); });
		buttonCheckExcept.onClick.AddListener(() => { Debug.Log(scrollSystem.GetCountExcept(GetSelectedPrefabNames().ToArray())); });
		buttonAddChat.onClick.AddListener(() =>
		{
			scrollSystem.Add("Chat", new ChatData { msg = inputField_ChatContent.text }, data =>
			{
				return new Vector2(0, scrollSystem.GetPreferHeightByString("Chat", (data as ChatData).msg));
			});
		});
		buttonAddChatJump.onClick.AddListener(() =>
		{
			scrollSystem.Add("Chat", new ChatData { msg = inputField_ChatContent.text }, data =>
			{
				return new Vector2(0, scrollSystem.GetPreferHeightByString("Chat", (data as ChatData).msg));
			});
			scrollSystem.Jump(1);
		});
		buttonDeleteAll.onClick.AddListener(() =>
		{
			createdDatas.Clear();
			scrollSystem.Clear();
		});
		buttonCreateDeleteAndAdd.onClick.AddListener(() =>
		{
			createdDatas.Clear();
			scrollSystem.Clear();
			foreach (var simpleData in deleteAndAddDatas)
			{
				createdDatas.Add(simpleData);
				scrollSystem.Add("D", simpleData);
			}
		});
		buttonJumpWithoutAnimation.onClick.AddListener(() => { scrollSystem.Jump(1, false); });
		buttonReverse.onClick.AddListener(() => { createdDatas.Reverse(); scrollSystem.Reverse(); });
		buttonRemoveFirst.onClick.AddListener(() =>
			{
				if (createdDatas.Count > 0)
				{
					var removedData = createdDatas[0];
					createdDatas.Remove(removedData);
					scrollSystem.Remove(removedData);
				}
			}
		);
		buttonRemoveLast.onClick.AddListener(() =>
			{
				if (createdDatas.Count > 0)
				{
					var removedData = createdDatas[createdDatas.Count - 1];
					createdDatas.Remove(removedData);
					scrollSystem.Remove(removedData);
				}
			}
		);
		ButtonRefresh.onClick.AddListener(() => { scrollSystem.Refresh(); });
		ButtonChangeData.onClick.AddListener(() => { createdDatas.ForEach(temp => { temp.index++; Debug.Log("added to index:" + temp.index); }); });
	}

	public SimpleData GenerateSimpleData()
	{
		var returnData = new SimpleData { index = global_index++ };
		createdDatas.Add(returnData);
		return returnData;
	}

}

public class SimpleData
{
	public int index;
}

public class ChatData : SimpleData
{
	public string msg;
}