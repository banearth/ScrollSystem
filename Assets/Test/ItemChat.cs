using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;

//表示聊天
public class ItemChat : MonoBehaviour
{

    private Text label;
    private Button button;
    private ChatData data;

    private void Awake()
    {
        label = this.transform.Find("Text").GetComponent<Text>();
        button = this.GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnRefresh(ChatData data)
    {
        this.data = data;
        label.text = data.msg;
    }

    public void OnClick()
    {
        Debug.Log("itemD:" + data.msg);
    }

}
