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
				scrollGallery.Select(aData, true);
			});
		});
		scrollGallery.SetOnItemClose((aGo, aData) =>
		{
			aGo.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
		});
		scrollGallery.SetOnItemRefresh((aGo, aData, isSelected) =>
		{
			Debug.Log("OnRefresh:" + (aData as SimpleData).number + " isSelected:" + isSelected);
			aGo.GetComponentInChildren<Text>().text = "number:" + (aData as SimpleData).number + (isSelected ? "√" : "");
		});

		this.datas = new SimpleData[10];

		for (int i = 0; i < 10; i++)
		{
			datas[i] = new SimpleData { number = i };
			scrollGallery.Add(datas[i]);
		}
		scrollGallery.Select(datas[1]);

	}

	private SimpleData[] datas;

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			scrollGallery.Select(this.datas[0],true);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			scrollGallery.Select(this.datas[1], true);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			scrollGallery.Select(this.datas[2], true);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			scrollGallery.Select(this.datas[3], true);
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			scrollGallery.Select(this.datas[4], true);
		}

		if (Input.GetKeyDown(KeyCode.C))
		{
			scrollGallery.Clear();
		}

		if (Input.GetKeyDown(KeyCode.B))
		{
			datas[0].number += 10;
			scrollGallery.Set(datas[0]);
		}

		if (Input.GetKeyDown(KeyCode.S))
		{
			for (int i = 0; i < datas.Length; i++)
			{
				Debug.Log(string.Format("data index:{0} isSelected:{1}",i, scrollGallery.IsSelected(datas[i])) );
			}
		}

		//var str = (item != null) ? item.normalizedPos.ToString("0.00") : "";
		//label.text = str;
	}



}
