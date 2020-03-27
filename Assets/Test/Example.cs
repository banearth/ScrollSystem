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

	public List<string> GetSelectedPrefabNames() {
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

	private List<SimpleData> currentDatas = null;

	void Start()
	{

		if (currentDatas == null)
		{
			currentDatas = new List<SimpleData>();
			for (int i = 0; i < 100; i++)
			{
				currentDatas.Add(new SimpleData { index = Example.global_index++ });
			}
		}

		Button[] buttons = new Button[] { buttonA, buttonB, buttonC, buttonD };

		for (int i = 0;i<buttons.Length;i++) {
			var curButton = buttons[i];
			var index = i;
			Action readAction = () =>
			{
				curButton.transform.Find("Mark").gameObject.SetActive(prefabSelected[index]);
			};
			curButton.onClick.AddListener(()=> {
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
					scrollSystem.Add(aName, new SimpleData { index = global_index++ });
				}
			}
		});

		buttonCheck.onClick.AddListener(() =>
		{
			Debug.Log(scrollSystem.GetCount(GetSelectedPrefabNames().ToArray()));
		});

		buttonCheckExcept.onClick.AddListener(() =>
		{
			Debug.Log(scrollSystem.GetCountExcept(GetSelectedPrefabNames().ToArray()));
		});

		buttonAddChat.onClick.AddListener(AddChat);
		buttonAddChatJump.onClick.AddListener(AddChatJump);

		buttonDeleteAll.onClick.AddListener(DeleteAll);
		buttonCreateDeleteAndAdd.onClick.AddListener(DeleteAndAdd);
		buttonJumpWithoutAnimation.onClick.AddListener(() =>
		{
			scrollSystem.Jump(1, false);
		});
		buttonReverse.onClick.AddListener(() =>
		{
			scrollSystem.Reverse();
		});

		scrollSystem.SetItemInitDelegate((prefabName, root) =>
		{
			switch (prefabName)
			{
				case "B":
					root.GetComponent<ItemB>().Init(scrollSystem);
					break;
				case "C":
					root.GetComponent<ItemC>().Init(scrollSystem);
					break;
			}
		});

		scrollSystem.SetItemContentDelegate((prefabName, root, data) =>
		{
			switch (prefabName)
			{
				case "A":
					{
						root.GetComponent<ItemA>().UpdateInfo(data as SimpleData);
					}
					break;
				case "B":
					{
						root.GetComponent<ItemB>().UpdateInfo(data as SimpleData);
					}
					break;
				case "C":
					{
						root.GetComponent<ItemC>().UpdateInfo(data as SimpleData);
					}
					break;
				case "D":
					{
						root.GetComponent<ItemD>().UpdateInfo(data as SimpleData);
					}
					break;
				case "Chat":
					{
						root.GetComponent<ItemChat>().UpdateInfo(data as ChatData);
					}
					break;
			}
		});

	}


	private void AddChatJump()
	{
		scrollSystem.Add("Chat", new ChatData { msg = inputField_ChatContent.text }, data =>
		{
			return new Vector2(0, scrollSystem.GetPreferHeightByString("Chat", (data as ChatData).msg));
		});
		scrollSystem.Jump(1);
	}

	private void AddChat()
	{
		scrollSystem.Add("Chat", new ChatData { msg = inputField_ChatContent.text }, data =>
		{
			return new Vector2(0, scrollSystem.GetPreferHeightByString("Chat", (data as ChatData).msg));
		});
	}

	private void DeleteAll()
	{
		scrollSystem.Clear();
	}

	private void DeleteAndAdd()
	{
		scrollSystem.Clear();
		foreach (var simpleData in currentDatas)
		{
			scrollSystem.Add("D", simpleData);
		}
	}

}

public class SimpleData
{
	public int index;
}

public class ChatData
{
	public string msg;
}