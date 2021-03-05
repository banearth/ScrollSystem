using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanSupport;
using System;

public class Example : MonoBehaviour
{

	public static int global_index = 0;

	public bool useOpenCloseRefreshEvent = false;
	public bool useBeginDragEvent = false;
	public bool useDragEvent = false;
	public bool useEndDragEvent = false;

	public Button buttonDeleteAll;
	public Button buttonAddChat;
	public Button buttonAddChatJump;
	public Button buttonCreateDeleteAndAdd;
	public Button buttonJumpWithoutAnimation;
	public Button buttonReverse;
	public Button buttonRemoveFirst;
	public Button buttonRemoveLast;
	public Button ButtonRefresh;
	public Button ButtonChangeData;

	public InputField inputField_Number;
	public Button buttonAdd;
	public Button buttonCheck;
	public Button buttonCheckExcept;
	public Button buttonA;
	public Button buttonB;
	public Button buttonC;
	public Button buttonD;
	public Button buttonE;

	public InputField inputField_JumpDataIndex;
	public Button buttonJumpData;
	public Button buttonLastOne;
	public Button buttonNextOne;
	public Button buttonJumpBegin;
	public Button buttonJumpEnd;
	public Button ButtonIsFirstVisible;
	public Button ButtonIsLastVisible;
	public Button ButtonEnableMovement;
	public Button ButtonDisableMovement;

	public Slider sliderJumpProgress;
	public Text textJumpProgress;

	public string[] prefabNames;
	public bool[] prefabSelected;

	public ScrollSystem scrollSystem;
	public InputField inputField_ChatContent;
	public bool setWorldPosEnable = true;

	public static Example Instance;

	public List<string> GetSelectedPrefabNames()
	{
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

	private List<SimpleData> deleteAndAddDatas = new List<SimpleData>();
	private List<SimpleData> createdDatas = new List<SimpleData>();

	void Start()
	{
		for (int i = 0; i < 100; i++)
		{
			deleteAndAddDatas.Add(new SimpleData { index = Example.global_index++ });
		}

		BindEvent();

		scrollSystem.SetItemRefresh((prefabName, root, data) =>
		{
			if (useOpenCloseRefreshEvent)
			{
				Debug.Log(string.Format(" {0} Refresh id:{1}", prefabName, (data as SimpleData).index.ToString()));
			}
			switch (prefabName)
			{
				case "A":
					{
						root.GetComponent<ItemA>().OnRefresh(data as SimpleData);
					}
					break;
				case "B":
					{
						root.GetComponent<ItemB>().OnRefresh(data as SimpleData);
					}
					break;
				case "C":
					{
						root.GetComponent<ItemC>().OnRefresh(data as SimpleData);
					}
					break;
				case "D":
					{
						root.GetComponent<ItemD>().OnRefresh(data as SimpleData);
					}
					break;
				case "Chat":
					{
						root.GetComponent<ItemChat>().OnRefresh(data as ChatData);
					}
					break;
			}
		});

		scrollSystem.SetItemClose((prefabName, root, data) =>
		{
			if (useOpenCloseRefreshEvent)
			{
				Debug.Log(string.Format(" {0} Close", prefabName));
			}
		});

		scrollSystem.SetItemOpen((prefabName, root, data) =>
		{
			if (useOpenCloseRefreshEvent)
			{
				Debug.Log(string.Format(" {0} Open", prefabName));
			}
		});

		if (useBeginDragEvent)
		{
			scrollSystem.SetBeginDrag(data =>
			{
				Debug.Log("OnBeginDrag");
			});
		}

		if (useEndDragEvent)
		{
			scrollSystem.SetEndDrag(data =>
			{
				Debug.Log("OnEndDrag");
			});
		}

		if (useDragEvent)
		{
			scrollSystem.SetDrag(data =>
			{
				Debug.Log("OnDrag");
			});
		}

	}

	private void BindEvent()
	{
		Button[] buttons = new Button[] { buttonA, buttonB, buttonC, buttonD, buttonE };
		for (int i = 0; i < buttons.Length; i++)
		{
			var curButton = buttons[i];
			var index = i;
			Action readAction = () =>
			{
				curButton.transform.Find("Mark").gameObject.SetActive(prefabSelected[index]);
			};
			curButton.onClick.AddListener(() =>
			{
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
					scrollSystem.Add(aName, GenerateSimpleData());
				}
			}
		});
		buttonCheck.onClick.AddListener(() => { Debug.Log(scrollSystem.GetDataCount(GetSelectedPrefabNames().ToArray())); });
		buttonCheckExcept.onClick.AddListener(() => { Debug.Log(scrollSystem.GetDataCountExcept(GetSelectedPrefabNames().ToArray())); });
		buttonAddChat.onClick.AddListener(() =>
		{
			AddChat(inputField_ChatContent.text);
		});

		buttonAddChatJump.onClick.AddListener(() =>
		{
			AddChat(inputField_ChatContent.text);
			scrollSystem.Jump(1);
		});
		buttonDeleteAll.onClick.AddListener(() =>
		{
			createdDatas.Clear();
			scrollSystem.Clear();
		});
		buttonCreateDeleteAndAdd.onClick.AddListener(() =>
		{
			createdDatas.Clear();
			scrollSystem.Clear();
			foreach (var simpleData in deleteAndAddDatas)
			{
				createdDatas.Add(simpleData);
				scrollSystem.Add("D", simpleData);
			}
		});
		buttonJumpWithoutAnimation.onClick.AddListener(() => { scrollSystem.Jump(1, false); });
		buttonReverse.onClick.AddListener(() => { createdDatas.Reverse(); scrollSystem.Reverse(); });
		buttonRemoveFirst.onClick.AddListener(() =>
			{
				if (createdDatas.Count > 0)
				{
					var removedData = createdDatas[0];
					createdDatas.Remove(removedData);
					scrollSystem.Remove(removedData);
				}
			}
		);
		buttonRemoveLast.onClick.AddListener(() =>
			{
				if (createdDatas.Count > 0)
				{
					var removedData = createdDatas[createdDatas.Count - 1];
					createdDatas.Remove(removedData);
					scrollSystem.Remove(removedData);
				}
			}
		);
		ButtonRefresh.onClick.AddListener(() => { scrollSystem.Refresh(); });
		ButtonChangeData.onClick.AddListener(() => { createdDatas.ForEach(temp => { temp.index++; Debug.Log("added to index:" + temp.index); }); });
		buttonJumpData.onClick.AddListener(() =>
		{
			if (int.TryParse(inputField_JumpDataIndex.text, out int result))
			{
				scrollSystem.Jump(result, true);
			}
		});
		buttonLastOne.onClick.AddListener(() =>
		{
			if (int.TryParse(inputField_JumpDataIndex.text, out int result))
			{
				if (result > 0)
				{
					result--;
					inputField_JumpDataIndex.text = result.ToString();
					scrollSystem.Jump(result, true);
				}
			}
		});
		buttonNextOne.onClick.AddListener(() =>
		{
			if (int.TryParse(inputField_JumpDataIndex.text, out int result))
			{
				if (result + 1 < scrollSystem.GetDataCount())
				{
					result++;
					inputField_JumpDataIndex.text = result.ToString();
					scrollSystem.Jump(result, true);
				}
			}
		});
		sliderJumpProgress.onValueChanged.AddListener(progress =>
		{
			textJumpProgress.text = progress.ToString("0.00");
			scrollSystem.Jump(progress);
		});
		buttonJumpBegin.onClick.AddListener(() =>
		{
			scrollSystem.Jump(0, !Input.GetKey(KeyCode.S));
		});
		buttonJumpEnd.onClick.AddListener(() =>
		{
			scrollSystem.Jump(1, !Input.GetKey(KeyCode.S));
		});
		ButtonIsFirstVisible.onClick.AddListener(() =>
		{
			Debug.Log("IsFirstVIsible:"+ scrollSystem.IsFirstVisible().ToString());
		});
		ButtonIsLastVisible.onClick.AddListener(() =>
		{
			Debug.Log("IsLastVIsible:" + scrollSystem.IsLastVisible().ToString());
		});
		ButtonEnableMovement.onClick.AddListener(()=> {
			scrollSystem.EnableMovement();
		});
		ButtonDisableMovement.onClick.AddListener(() => {
			scrollSystem.DisableMovement();
		});
	}

	public SimpleData GenerateSimpleData()
	{
		var returnData = new SimpleData { index = global_index++ };
		createdDatas.Add(returnData);
		return returnData;
	}

	private void AddChat(string msg)
	{
		scrollSystem.Add("Chat", new ChatData { msg = msg}, GetChatHeight);
	}

	private Vector2 GetChatHeight(object data)
	{
		ScrollLayout referScrollLayout = scrollSystem.GetOriginPrefab("Chat").GetComponent<ScrollLayout>();
		var height = referScrollLayout.GetHeightByStr((data as ChatData).msg);
		return new Vector2(-1, height);
	}

	//private void OnGUI()
	//{
	//	GUILayout.Label(ObjectPool<ScrollSystem.SearchGroup>.GetState());
	//}

}

public class SimpleData
{
	public int index;
}

public class ChatData : SimpleData
{
	public string msg;
}