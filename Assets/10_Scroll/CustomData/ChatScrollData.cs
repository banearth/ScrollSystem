using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport
{

	//public static partial class ScrollSytemExtension
	//{
		//public static void AddChatData(this ScrollSystem scrollSystem, string prefabName, object dataSource, Func<object, string> getMsgFunc)
		//{
		//	var newScrollData = new ChatScrollData(scrollSystem, prefabName, dataSource, getMsgFunc);
		//	scrollSystem.Add(prefabName, newScrollData);
		//}
	//}

	public class ChatScrollData : ScrollData
	{

		private Func<object, string> getMsgFunc;

		public ChatScrollData(ScrollSystem scrollSystem, string prefabName, object dataSource, Func<object, string> getMsgFunc)
		{
			//base.Init(scrollSystem, prefabName, dataSource, ResizeByString);
			//this.getMsgFunc = getMsgFunc;
		}

		//private Vector2 ResizeByString(object dataSource)
		//{
		//	var msg = getMsgFunc(dataSource);
		//	return new Vector2(0, objectPool.CalculateHeightByFitString(msg));
		//}

	}

}