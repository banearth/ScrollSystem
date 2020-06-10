using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;

public class TestScrollGallery : MonoBehaviour
{

	public ScrollGallery scrollGallery;
	public Text label;

	public class SimpleData
	{
		public int number;
	}

	// Start is called before the first frame update
	void Start()
	{
		scrollGallery.SetOnItemOpen((aGo, aData) =>
		{
			aGo.GetComponentInChildren<Button>().onClick.AddListener(() =>
			{
				Debug.Log("OnOpen:" + (aData as SimpleData).number.ToString());
				scrollGallery.Select(aData);
			});
		});
		scrollGallery.SetOnItemClose((aGo, aData) =>
		{
			aGo.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
		});
		scrollGallery.SetOnItemRefresh((aGo, aData, isSelected) =>
		{
			aGo.GetComponentInChildren<Text>().text = "number:" + (aData as SimpleData).number + (isSelected ? "√" : "");
		});

		this.datas = new SimpleData[10];

		for (int i = 0; i < 10; i++)
		{
			datas[i] = new SimpleData { number = i };
			scrollGallery.Add(datas[i]);
		}

	}

	private SimpleData[] datas;

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			Debug.Log("111");
			scrollGallery.Select(this.datas[0]);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			scrollGallery.Select(this.datas[1]);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			scrollGallery.Select(this.datas[2]);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			scrollGallery.Select(this.datas[3]);
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			scrollGallery.Select(this.datas[4]);
		}


		//var str = (item != null) ? item.normalizedPos.ToString("0.00") : "";
		//label.text = str;
	}



}
