using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;
using System;

public class Example : MonoBehaviour
{

	public Button buttonDeleteAll;
	public Button buttonAddChat;
	public Button buttonAddChatJump;
	public Button buttonCreateDeleteAndAdd;
	public Button buttonJumpWithoutAnimation;

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

	public class ChatData
	{
		public string msg;
	}

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

	void Start()
	{
		//prefabSelected = new bool[prefabNames.Length];
		//for (int i = 0; i < prefabNames.Length; i++)
		//{
		//	prefabSelected[i] = false;
		//}

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

		scrollSystem.SetItemContentDelegate((prefabName, root, data) =>
		{
			switch (prefabName)
			{
				case "A":
					{
						//Debug.Log("Read A");
						//a表示累加
						var a = data as SimpleData;
						root.Find("Text").GetComponent<Text>().text = a.index.ToString();

						var button = root.GetComponent<Button>();
						button.onClick.RemoveAllListeners();
						button.onClick.AddListener(() =>
						{
							a.index++;
							scrollSystem.Set(a);
						});
					}
					break;
				case "B":
					{
						//Debug.Log("Read B");
						//表示删除
						var a = data as SimpleData;
						root.Find("Text").GetComponent<Text>().text = a.index.ToString();

						var button = root.GetComponent<Button>();
						button.onClick.RemoveAllListeners();
						button.onClick.AddListener(() =>
						{
							scrollSystem.Remove(data);
						});
					}
					break;
				case "C":
					{
						//Debug.Log("Read C");
						//表示插入
						var a = data as SimpleData;
						root.Find("Text").GetComponent<Text>().text = a.index.ToString();

						var button = root.GetComponent<Button>();
						button.onClick.RemoveAllListeners();
						button.onClick.AddListener(() =>
						{
							scrollSystem.Insert("C",data, new SimpleData { index = global_index++ });
						});
					}
					break;
				case "Chat":
					{
						var chatData = data as ChatData;
						root.Find("Text").GetComponent<Text>().text = chatData.msg;
						var button = root.GetComponent<Button>();
						button.onClick.RemoveAllListeners();
						button.onClick.AddListener(() =>
						{
							scrollSystem.Remove(data);
						});
					}
					break;
			}
		});

	}

	private static int global_index = 0;

	public class SimpleData
	{
		public int index;
	}

	private void AddChatJump()
	{
		scrollSystem.AddChatData("Chat", new ChatData { msg = inputField_ChatContent.text }, data => (data as ChatData).msg);
		scrollSystem.Jump(1);
	}

	private void AddChat()
	{
		for (int i = 0;i<5;i++) {
			scrollSystem.AddChatData("Chat", new ChatData { msg = inputField_ChatContent.text }, data => (data as ChatData).msg);
		}
	}

	private void DeleteAll()
	{
		scrollSystem.Clear();
	}

	private void DeleteAndAdd() {
		scrollSystem.Clear();
		for (int i = 0; i < 30; i++)
		{
			scrollSystem.AddChatData("Chat", new ChatData { msg = inputField_ChatContent.text }, data => (data as ChatData).msg);
		}
	}

	private void RemoveFirst()
	{
		scrollSystem.RemoveFirst();
	}

	public void RemoveLast()
	{
		scrollSystem.RemoveLast();
	}

	public void Clear()
	{
		scrollSystem.Clear();
	}

}
