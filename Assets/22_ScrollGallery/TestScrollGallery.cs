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

	private GalleryData item;

	// Start is called before the first frame update
	void Start()
	{
		scrollGallery.SetOnItemRefresh((aGo, item) =>
		{
			this.item = item;
			aGo.GetComponentInChildren<Text>().text = "" + item.normalizedPos.ToString("0.00");
		});
		for (int i = 0; i < 10; i++)
		{
			scrollGallery.Add(new SimpleData { number = i });
		}
	}

	// Update is called once per frame
	void Update()
	{
		var str = (item != null) ? item.normalizedPos.ToString("0.00") : "";
		label.text = str;
	}



}
