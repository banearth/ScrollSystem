using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BanSupport.ScrollSystem
{
	public class JumpState
	{
		private enum State { None, Directly, Animated }
		private State state = State.None;
		private float targetNormalizedPos;
		private ScrollData targetScrollData;
		private Action<float> setNormalizedPos;
		private Func<float> getNormalizedPos;
		private ScrollSystem scrollSystem;

		public JumpState(ScrollSystem scrollSystem, Action<float> setNormalizedPos, Func<float> getNormalizedPos)
		{
			this.scrollSystem = scrollSystem;
			this.setNormalizedPos = setNormalizedPos;
			this.getNormalizedPos = getNormalizedPos;
		}

		public bool Update()
		{
			if (state != State.None)
			{
				if (scrollSystem.ScrollDirection_GET == ScrollSystem.ScrollDirection.Vertical)
				{
					float offset = scrollSystem.ContentSize - scrollSystem.Height;
					if (offset <= 0)
					{
						this.targetNormalizedPos = 0;
					}
					else
					{
						if (this.targetScrollData != null)
						{
							switch (scrollSystem.startCorner)
							{
								case 0: //Left Up
								case 1: //Right Up
									this.targetNormalizedPos = 1 - (this.targetScrollData.originPosition.y - this.targetScrollData.height / 2) / offset;
									break;
								case 2: //Left Down
								case 3: //Right Down
									this.targetNormalizedPos = (this.targetScrollData.originPosition.y - this.targetScrollData.height / 2) / offset;
									break;
							}
						}
					}
				}
				else
				{
					float offset = scrollSystem.ContentSize - scrollSystem.Width;
					if (offset <= 0)
					{
						this.targetNormalizedPos = 0;
					}
					else
					{
						if (this.targetScrollData != null)
						{
							switch (scrollSystem.startCorner)
							{
								case 0: //Left Up
								case 2: //Left Down
									this.targetNormalizedPos = (this.targetScrollData.originPosition.x - this.targetScrollData.width / 2) / offset;
									break;
								case 1: //Right Up
								case 3: //Right Down
									this.targetNormalizedPos = 1 - (this.targetScrollData.originPosition.x - this.targetScrollData.width / 2) / offset;
									break;
							}
						}
					}
				}
				this.targetScrollData = null;
				this.targetNormalizedPos = Mathf.Clamp01(this.targetNormalizedPos);

				//根据state来判断如何跳转
				switch (state)
				{
					case State.Directly:
						this.setNormalizedPos(targetNormalizedPos);
						state = State.None;
						break;
					case State.Animated:
						float lerpNormalizedPos = Mathf.Lerp(this.getNormalizedPos(), targetNormalizedPos, Time.deltaTime * scrollSystem.JumpToSpeed);
						var pixelDistance = Mathf.Abs(lerpNormalizedPos - targetNormalizedPos) * scrollSystem.ContentSize;
						if (pixelDistance < 1)
						{
							this.setNormalizedPos(this.targetNormalizedPos);
							state = State.None;
						}
						else
						{
							this.setNormalizedPos(lerpNormalizedPos);
						}
						break;
				}
				return true;
			}
			else
			{
				return false;
			}

		}

		public void Stop()
		{
			state = State.None;
		}

		public void Do(float targetNormalizedPos, bool animated)
		{
			if (!scrollSystem.MoveEnable) { return; }
			scrollSystem.scrollRect.StopMovement();
			state = animated ? State.Animated : State.Directly;
			this.targetScrollData = null;
			targetNormalizedPos = Mathf.Clamp01(targetNormalizedPos);
			if (scrollSystem.ScrollDirection_GET == ScrollSystem.ScrollDirection.Vertical)
			{
				switch (scrollSystem.startCorner)
				{
					case 0: //Left Up
					case 1: //Right Up
						this.targetNormalizedPos = 1 - targetNormalizedPos;
						break;
					case 2: //Left Down
					case 3: //Right Down
						this.targetNormalizedPos = targetNormalizedPos;
						break;
				}
			}
			else
			{
				switch (scrollSystem.startCorner)
				{
					case 0: //Left Up
					case 2: //Left Down
						this.targetNormalizedPos = targetNormalizedPos;
						break;
					case 1: //Right Up
					case 3: //Right Down
						this.targetNormalizedPos = 1 - targetNormalizedPos;
						break;
				}
			}
		}

		public void Do(ScrollData targetScrollData, bool animated)
		{
			if (!scrollSystem.MoveEnable) { return; }
			scrollSystem.scrollRect.StopMovement();
			state = animated ? State.Animated : State.Directly;
			this.targetScrollData = targetScrollData;
			this.targetNormalizedPos = 0;
		}

	}

}