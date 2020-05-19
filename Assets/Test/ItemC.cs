using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;

//表示插入
public class ItemC : MonoBehaviour
{

    private Text label;
    private Button button;
    private ScrollSystem scrollSystem;
    private SimpleData data;

    private void Awake()
    {
        label = this.transform.Find("Text").GetComponent<Text>();
        button = this.GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void Init(ScrollSystem scrollSystem)
    {
        this.scrollSystem = scrollSystem;
    }

    public void OnRefresh(SimpleData data)
    {
        this.data = data;
        label.text = data.index.ToString();
    }

    public void OnClick()
    {
        scrollSystem.Insert("C",data,new SimpleData { index = Example.global_index++ });
    }

}
