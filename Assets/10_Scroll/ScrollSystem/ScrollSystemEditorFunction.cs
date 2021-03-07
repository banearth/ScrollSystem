using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BanSupport
{
	//仅用于编辑器模式下
	public partial class ScrollSystem
	{

#if UNITY_EDITOR

		private int _startCorner = int.MinValue;
		private int _clipType = 0;

		private Vector2 _size = Vector2.zero;
		private ScrollDirection _scrollDirection = ScrollDirection.Vertical;
		private Vector2 _border = Vector2.zero;
		private Vector2 _spacing = Vector2.one * 10;
		private int _childCount = 0;
		private bool _centered = false;
		private Action _beSameAction;

		public class CheckMode
		{
			public static int Component = 1 << 0;
			public static int ContentChildren = 1 << 1;
		}

		#region 异动入口

		//参数改变时，进行全部检查
		private void OnValidate()
		{
			//只在编辑器环境下调用
			if (Application.isPlaying) { return; }
			StartCoroutine(DelayCheckChange(CheckMode.Component));
			CheckChange(CheckMode.ContentChildren);
		}

		//RectTransfrom宽高发生改变时，进行重新排列
		private void OnRectTransformDimensionsChange()
		{
			//只在编辑器环境下调用
			if (Application.isPlaying) { return; }
			CheckChange(CheckMode.ContentChildren);
		}

		//预制体数量发生改变时，进行重新排列
		private void OnPrefebCountChanged()
		{
			CheckChange(CheckMode.ContentChildren);
		}

		#endregion

		//因为存在Component的Add和Remove，所以不能直接在OnValidate里面调用
		private IEnumerator DelayCheckChange(int checkMode)
		{
			yield return new WaitForEndOfFrame();
			CheckChange(checkMode);
		}

		private void CheckChange(int checkMode)
		{
			if ((checkMode & CheckMode.Component) > 0)
			{
				if (CheckComponent())
				{
					SetComponent();
				}
			}
			if ((checkMode & CheckMode.ContentChildren) > 0)
			{
				if (CheckContent())
				{
					SetContent();
				}
			}
			if (_beSameAction != null)
			{
				_beSameAction();
				_beSameAction = null;
			}
		}

		private bool CheckComponent()
		{
			bool result = false;
			if (_scrollDirection != this.scrollDirection)
			{
				result = true;
				_beSameAction += () =>
				{
					_scrollDirection = this.scrollDirection;
				};
			}
			if (_clipType != this.clipType)
			{
				result = true;
				_beSameAction += () =>
				{
					_clipType = this.clipType;
				};
			}
			return result;
		}

		private bool CheckContent()
		{
			bool result = false;
			if (_startCorner != this.startCorner)
			{
				result = true;
				_beSameAction += () =>
				{
					_startCorner = this.startCorner;
				};
			}
			if (_size != (this.transform as RectTransform).sizeDelta)
			{
				result = true;
				_beSameAction += () =>
				{
					_size = (this.transform as RectTransform).sizeDelta;
				};
			}

			if (_spacing != this.spacing)
			{
				result = true;
				_beSameAction += () =>
				{
					_spacing = spacing;
				};
			}
			if (_scrollDirection != scrollDirection)
			{
				result = true;
				_beSameAction += () =>
				{
					_scrollDirection = scrollDirection;
				};
			}
			if (_border != border)
			{
				result = true;
				_beSameAction += () =>
				{
					_border = border;
				};
			}
			if (_centered != isCenter)
			{
				result = true;
				_beSameAction += () =>
				{
					_centered = isCenter;
				};
			}
			if (ContentTrans != null)
			{
				if (_childCount != this.ContentTrans.Value.childCount)
				{
					result = true;
					_beSameAction += () =>
					{
						_childCount = this.ContentTrans.Value.childCount;
					};
				}
			}
			return result;
		}

		private void SetComponent()
		{
			switch (clipType)
			{
				case 0:
					//不进行任何裁切相关的处理
					break;
				case 1:
					//-------------------使用Mask裁切-------------------

					//-------------------删除不必要的-------------------
					Tools.RemoveComponent<NonDrawingGraphic>(this.gameObject);
					Tools.RemoveComponent<RectMask2D>(this.gameObject);
					//-------------------增加需要的-------------------
					if (Tools.AddComponent<Image>(this.gameObject, out Image newImage))
					{
						newImage.sprite = null;
						newImage.color = new Color(1, 1, 1, 0.2f);
					}
					if (Tools.AddComponent<Mask>(this.gameObject, out Mask newMask))
					{
						newMask.showMaskGraphic = true;
					}
					break;
				case 2:
					//-------------------使用RectMask裁切-------------------
					//-------------------删除不必要的-------------------
					Tools.RemoveComponent<Image>(this.gameObject);
					Tools.RemoveComponent<Mask>(this.gameObject);
					//-------------------增加需要的-------------------
					Tools.AddComponent<NonDrawingGraphic>(this.gameObject, out NonDrawingGraphic _);
					Tools.AddComponent<RectMask2D>(this.gameObject, out RectMask2D _);
					break;
				case 3:
					//-------------------清理所有-------------------
					Tools.RemoveComponent<NonDrawingGraphic>(this.gameObject);
					Tools.RemoveComponent<RectMask2D>(this.gameObject);
					Tools.RemoveComponent<Image>(this.gameObject);
					Tools.RemoveComponent<Mask>(this.gameObject);
					break;
			}

			//ScrollRect
			if (Tools.AddComponent<ScrollRect>(this.gameObject, out ScrollRect newScrollRect))
			{
				newScrollRect.decelerationRate = 0.04f;
			}
			EnableScrollDirection();
			ScrollRect.viewport = this.transform as RectTransform;
			ScrollRect.content = ContentTrans.Value;

			//交换horizontalBar和verticalBar
			if (scrollDirection == ScrollDirection.Vertical)
			{
				if (ScrollRect.verticalScrollbar == null && ScrollRect.horizontalScrollbar != null)
				{
					ScrollRect.verticalScrollbar = ScrollRect.horizontalScrollbar;
					ScrollRect.horizontalScrollbar = null;
				}
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				if (ScrollRect.horizontalScrollbar == null && ScrollRect.verticalScrollbar != null)
				{
					ScrollRect.horizontalScrollbar = ScrollRect.verticalScrollbar;
					ScrollRect.verticalScrollbar = null;
				}
			}

			//-----------------ContentTrans-----------------
			if (_contentTrans == null)
			{
				var tempTrans = new GameObject(ContentTransformName, typeof(RectTransform)).transform as RectTransform;
				_contentTrans = tempTrans.gameObject.AddComponent<ContentTransform>();
				_contentTrans.Value.SetParent(this.transform);
				_contentTrans.Value.gameObject.layer = this.gameObject.layer;
				_contentTrans.Value.localPosition = Vector3.zero;
				_contentTrans.Value.localScale = Vector3.one;
			}
		}

		public void SetContent()
		{
			if (this.ContentTrans == null)
			{
				return;
			}
			//初始化
			InitDelegate();
			InitFormatPrefabRectTransform();
			InitCursor();
			InitContentTrans();

			Dictionary<RectTransform, Vector2> dic_RectTransform_AnchoredPosition = new Dictionary<RectTransform, Vector2>();
			var childCount = this.ContentTrans.Value.childCount;
			if (scrollDirection == ScrollDirection.Vertical)
			{
				for (int i = 0; i < childCount; i++)
				{
					var rectTransform = this.ContentTrans.Value.GetChild(i) as RectTransform;
					if (IsPrefabNameIgnored(rectTransform.name)) { continue; }
					onFormatPrefab(rectTransform);
					ScrollLayout.NewLine newLine = ScrollLayout.NewLine.None;
					var layout = this.ContentTrans.Value.GetChild(i).GetComponent<ScrollLayout>();
					if (layout != null)
					{
						newLine = layout.newLine;
					}
					if (newLine == ScrollLayout.NewLine.None)
					{
						//发生过偏移，并且这次物体的右边界超过宽度
						if (cursorPos.x > border.x && cursorPos.x + rectTransform.sizeDelta.x > Width - border.x)
						{
							//那么执行换行操作
							cursorPos.x = border.x;
							cursorPos.y += maxHeight;
							maxHeight = 0;
						}
						//设置位置
						dic_RectTransform_AnchoredPosition.Add(rectTransform, cursorPos + rectTransform.sizeDelta / 2);
						//更新光标
						cursorPos.x += rectTransform.sizeDelta.x;
						//更新最大宽度
						if (maxWidth < cursorPos.x) { maxWidth = cursorPos.x; }
						//增加间隔
						if (rectTransform.sizeDelta.x > 0)
						{
							cursorPos.x += spacing.x;
						}
						float curMaxHeight = rectTransform.sizeDelta.y > 0 ? (rectTransform.sizeDelta.y + spacing.y) : 0;
						//更新最大高度
						if (maxHeight < curMaxHeight)
						{
							maxHeight = curMaxHeight;
						}
					}
					else
					{
						//发生过偏移，换行
						if (cursorPos.x > border.x)
						{
							cursorPos.y += maxHeight;
							maxHeight = 0;
						}
						switch (newLine)
						{
							case ScrollLayout.NewLine.Center:
								{
									cursorPos.x = Width / 2;
								}
								break;
							case ScrollLayout.NewLine.LeftOrUp:
								{
									cursorPos.x = rectTransform.sizeDelta.x / 2 + border.x;
								}
								break;
							case ScrollLayout.NewLine.RightOrDown:
								{
									cursorPos.x = Width - rectTransform.sizeDelta.x / 2 - border.x;
								}
								break;
						}
						//设置位置（需要注意这里直接赋给位置，不需要进行居中处理）
						rectTransform.anchoredPosition = TransAnchoredPosition(cursorPos + rectTransform.sizeDelta.y / 2 * Vector2.up);
						//换新行
						cursorPos.x = border.x;
						cursorPos.y += rectTransform.sizeDelta.y + spacing.y;
					}
				}
				//设置content高度
				ContentTrans.Value.sizeDelta = new Vector2(ContentTrans.Value.sizeDelta.x, cursorPos.y + maxHeight - (childCount > 0 ? spacing.y : 0) + border.y);
				float centerOffset = 0;
				if (isCenter)
				{
					centerOffset = (Width - border.x - maxWidth) / 2;
				}
				foreach (var rectTransform in dic_RectTransform_AnchoredPosition.Keys)
				{
					rectTransform.anchoredPosition = TransAnchoredPosition(dic_RectTransform_AnchoredPosition[rectTransform] + Vector2.right * centerOffset);
				}
			}
			else if (scrollDirection == ScrollDirection.Horizontal)
			{
				for (int i = 0; i < childCount; i++)
				{
					var rectTransform = this.ContentTrans.Value.GetChild(i) as RectTransform;
					if (IsPrefabNameIgnored(rectTransform.name)) { continue; }
					onFormatPrefab(rectTransform);
					ScrollLayout.NewLine newLine = ScrollLayout.NewLine.None;
					var layout = this.ContentTrans.Value.GetChild(i).GetComponent<ScrollLayout>();
					if (layout != null)
					{
						newLine = layout.newLine;
					}
					if (newLine == ScrollLayout.NewLine.None)
					{
						//发生过偏移，并且这次物体的右边界超过宽度
						if (cursorPos.y > border.y && cursorPos.y + rectTransform.sizeDelta.y > Height - border.y)
						{
							//那么执行换行操作
							cursorPos.y = border.y;
							cursorPos.x += maxHeight;
							maxHeight = 0;
						}
						//设置位置
						dic_RectTransform_AnchoredPosition.Add(rectTransform, cursorPos + rectTransform.sizeDelta / 2);
						//更新光标
						cursorPos.y += rectTransform.sizeDelta.y;
						//更新最大宽度
						if (maxWidth < cursorPos.y) { maxWidth = cursorPos.y; }
						//增加间隔
						if (rectTransform.sizeDelta.y > 0)
						{
							cursorPos.y += spacing.y;
						}
						//更新最大高度
						float curMaxHeight = rectTransform.sizeDelta.x > 0 ? (rectTransform.sizeDelta.x + spacing.x) : 0;
						if (maxHeight < curMaxHeight)
						{
							maxHeight = curMaxHeight;
						}
					}
					else
					{
						//发生过偏移，换行
						if (cursorPos.y > border.y)
						{
							cursorPos.x += maxHeight;
							maxHeight = 0;
						}
						switch (newLine)
						{
							case ScrollLayout.NewLine.Center:
								{
									cursorPos.y = Height / 2;
								}
								break;
							case ScrollLayout.NewLine.LeftOrUp:
								{
									cursorPos.y = rectTransform.sizeDelta.y / 2 + border.y;
								}
								break;
							case ScrollLayout.NewLine.RightOrDown:
								{
									cursorPos.y = Height - rectTransform.sizeDelta.y / 2 - border.y;
								}
								break;
						}
						//设置位置（需要注意这里直接赋给位置，不需要进行居中处理）
						rectTransform.anchoredPosition = TransAnchoredPosition(cursorPos + rectTransform.sizeDelta.x / 2 * Vector2.right);
						//换新行
						cursorPos.y = border.y;
						cursorPos.x += rectTransform.sizeDelta.x + spacing.x;
					}
				}
				//设置content高度
				ContentTrans.Value.sizeDelta = new Vector2(cursorPos.x + maxHeight - (childCount > 0 ? spacing.x : 0) + border.x, ContentTrans.Value.sizeDelta.y);
				float centerOffset = 0;
				if (isCenter)
				{
					centerOffset = (Height - border.y - maxWidth) / 2;
				}
				foreach (var rectTransform in dic_RectTransform_AnchoredPosition.Keys)
				{
					rectTransform.anchoredPosition = TransAnchoredPosition(dic_RectTransform_AnchoredPosition[rectTransform] + Vector2.up * centerOffset);
				}
			}
			dic_RectTransform_AnchoredPosition.Clear();
		}

		private void OnDrawGizmos()
		{
			if (drawGizmos)
			{
				Gizmos.color = Color.green;
				//基本点触区域
				Tools.DrawRect(this.transform.position, Width * this.transform.lossyScale.x, Height * this.transform.lossyScale.y, Color.green);
				//滚动区域
				if (this.ContentTrans != null)
				{
					var contentRect = Tools.GetRectBounds(ContentTrans.Value, out float worldPosZ);
					Tools.DrawRect(contentRect, worldPosZ, Color.green);
					if ((border.x > 0 || border.y > 0) && (ContentTrans.Value.rect.width > 2 * border.x) && (ContentTrans.Value.rect.height > 2 * border.y))
					{
						var localScale = ContentTrans.Value.lossyScale;
						var borderOffsetX = localScale.x * border.x;
						var borderOffsetY = localScale.y * border.y;
						contentRect.left += borderOffsetX;
						contentRect.right -= borderOffsetX;
						contentRect.up -= borderOffsetY;
						contentRect.down += borderOffsetY;
						Tools.DrawRect(contentRect, worldPosZ, Color.cyan);
					}
				}
			}
		}

#endif

	}
}