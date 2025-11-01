using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	public class Battle : MonoBehaviour
	{
		#region Singleton
		static Battle instance;
		public static Battle Instance => instance;
		#endregion

		#region Life cycle
		protected void Awake()
		{
			instance = this;
		}

		protected void OnDestroy()
		{
			instance = null;
		}
		#endregion

		#region Game data
		/// <summary>
		/// 会被 <c>GameInstance</c> 在退出战役时调用；
		/// 返回值会被应用到实际游戏数据上。
		/// </summary>
		public BattleResult YieldResult()
		{
			// TODO: 结算战役。
			return new();
		}
		#endregion

		#region Functions
		/// <summary>给 UI 控件暴露的接口方法。</summary>
		public void ExitBattle()
		{
			GameInstance.Instance.ExitBattle();
		}
		#endregion
	}
}
