using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;

public class Test : MonoBehaviour
{

	public ScrollSystem scrollSystem;
	public int totalCount = 5;

	public InputField inputField_Send;
	public InputField inputField_A;
	public InputField inputField_B;

	public Button btn_JumpToStart;
	public Button btn_JumpToEnd;
	public Button btn_Send;
	public Button btn_Clear;
	public Button btn_AddA;
	public Button btn_AddB;

	public class Data
	{
		public int index;
		public int number;
	}

	public class ChatData
	{
		public int index;
		public string msg;
	}

	void Start()
	{
		btn_JumpToStart.onClick.AddListener(() =>
		{
			scrollSystem.Jump(0);
		});

		btn_JumpToEnd.onClick.AddListener(() =>
		{
			scrollSystem.Jump(1);
		});
		btn_Send.onClick.AddListener(() =>
		{
			Send();
		});
		btn_Clear.onClick.AddListener(() =>
		{
			scrollSystem.Clear();
		});
		btn_AddA.onClick.AddListener(() =>
		{
			if (int.TryParse(inputField_A.text, out int result))
			{
				Add(result, "A");
			}
		});
		btn_AddB.onClick.AddListener(() =>
		{
			//Add(1, "B");
			if (int.TryParse(inputField_B.text, out int result))
			{
				Add(result, "B");
			}
		});
	}

	private void Update()
	{
	}

	private void Add(int number, string prefabName)
	{
		/*
		var data = new Data { number = number };
		scrollSystem.Add(prefabName, (go) =>
		{
			go.transform.Find("Text").GetComponent<Text>().text = data.number.ToString() + "\n" + ("index:" + data.index.ToString());
			var aButton = go.GetComponent<Button>();
			aButton.onClick.RemoveAllListeners();
			aButton.onClick.AddListener(() =>
			{
				//点击移除
				scrollSystem.Remove(data.index);
				//scrollSystem.Show();
			});
		}, (index) =>
		{
			data.index = index;
		});
		//scrollSystem.Show();
		*/
	}


	private void Send()
	{
		/*
		var chatData = new ChatData { msg = inputField_Send.text };
		scrollSystem.Add("C", (go) =>
	   {
		   go.transform.Find("Text").GetComponent<Text>().text = chatData.msg;
		   var aButton = go.GetComponent<Button>();
		   aButton.onClick.RemoveAllListeners();
		   aButton.onClick.AddListener(() =>
		   {
			   scrollSystem.Remove(chatData.index);
			   //scrollSystem.Show();
		   });
	   }, index =>
	   {
		   chatData.index = index;
	   }, () =>
	   {
		   return chatData.msg;
	   });
		//scrollSystem.Show();
		scrollSystem.JumpTo(1);
		*/
	}

}

