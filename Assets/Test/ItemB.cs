using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;

//表示删除
public class ItemB : MonoBehaviour
{

    private Text label;
    private Button button;
    private ScrollSystem scrollSystem;
    private SimpleData data;

    void Awake()
    {
        label = this.transform.Find("Text").GetComponent<Text>();
        button = this.GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        this.scrollSystem = GetComponentInParent<ScrollSystem>();
    }

    public void OnRefresh(SimpleData data)
	{
		this.data = data;
		label.text = data.index.ToString();
	}

	public void OnClick()
	{
        scrollSystem.Remove(data);
    }

}
