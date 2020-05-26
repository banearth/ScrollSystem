using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;

//表示仅仅打印
public class ItemD : MonoBehaviour
{

    public ScrollSystem scrollSystem;
    private Text label;
    private Button button;
    private SimpleData data;

    private void Awake()
    {
        label = this.transform.Find("Text").GetComponent<Text>();
        button = this.GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnRefresh(SimpleData data)
    {
        this.data = data;
        label.text = data.index.ToString();
    }

    public void OnClick()
    {
		//Debug.Log("itemD:" + data.index);
		scrollSystem.JumpData(this.data,true);
	}

}
