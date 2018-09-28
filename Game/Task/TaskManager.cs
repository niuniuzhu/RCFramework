using System.Collections.Generic;
using UnityEngine;

namespace Game.Task
{
	public class TaskManager : MonoBehaviour
	{
		public delegate void TaskHandler( float dt );

		static TaskManager _instance;

		public static TaskManager Instantiate()
		{
			if ( _instance == null )
			{
				GameObject go = new GameObject( "TaskManager" );
				go.hideFlags = HideFlags.HideInHierarchy;
				DontDestroyOnLoad( go );
				_instance = go.AddComponent<TaskManager>();
			}
			return _instance;
		}

		public static TaskManager instance
		{
			get
			{
				if ( _instance == null )
					Instantiate();
				return _instance;
			}
		}

		#region 注册Schedule
		private readonly List<ScheduleEntry> _schedules = new List<ScheduleEntry>();
		private readonly List<ScheduleEntry> _schedulesToRemove = new List<ScheduleEntry>();

		public void RegisterSchedule( float[] times, ScheduleEntry.ScheduleHandler timerCallback,
									  ScheduleEntry.CompleteHandler completeCallback, object param = null )
		{
			ScheduleEntry schedule = new ScheduleEntry( times, timerCallback, completeCallback, param );
			this._schedules.Add( schedule );
		}

		public void UnregisterSchedule( ScheduleEntry scheduleEntry )
		{
			this._schedulesToRemove.Add( scheduleEntry );
		}

		public void UnregisterSchedule( ScheduleEntry.ScheduleHandler callback )
		{
			int count = this._schedules.Count;
			for ( int i = 0; i < count; i++ )
			{
				ScheduleEntry scheduleEntry = this._schedules[i];
				if ( scheduleEntry.scheduleCallback == callback )
					this._schedulesToRemove.Add( scheduleEntry );
			}
		}
		#endregion

		#region 注册Timer
		private readonly List<TimerEntry> _timers = new List<TimerEntry>();
		private readonly List<TimerEntry> _timersToRemove = new List<TimerEntry>();

		public void RegisterTimer( float interval, int repeat, bool startImmediately, TimerEntry.TimerHandler timerCallback,
								   TimerEntry.CompleteHandler completeCallback, object param = null )
		{
			TimerEntry timer = new TimerEntry( interval, repeat, startImmediately, timerCallback, completeCallback, param );
			this._timers.Add( timer );
		}

		public void UnregisterTimer( TimerEntry timerEntry )
		{
			this._timersToRemove.Add( timerEntry );
		}

		public void UnregisterTimer( TimerEntry.TimerHandler callback )
		{
			int count = this._timers.Count;
			for ( int i = 0; i < count; i++ )
			{
				TimerEntry timerEntry = this._timers[i];
				if ( timerEntry.timerCallback == callback )
					this._timersToRemove.Add( timerEntry );
			}
		}
		#endregion

		#region Register Update Method

		private readonly List<TaskHandler> _updates = new List<TaskHandler>();
		private readonly List<TaskHandler> _updatesToRemove = new List<TaskHandler>();

		/// <summary>
		/// 注册在每个更新帧的回调函数
		/// </summary>
		/// <param name="callback">回调函数</param>
		public void RegisterUpdateMethod( TaskHandler callback )
		{
			this._updatesToRemove.Remove( callback );
			if ( this._updates.Contains( callback ) )
				return;
			this._updates.Add( callback );
		}

		/// <summary>
		/// 注销在每个更新帧的回调函数
		/// </summary>
		/// <param name="callback">回调函数</param>
		public void UnregisterUpdateMethod( TaskHandler callback )
		{
			if ( this._updatesToRemove.Contains( callback ) )
				return;
			this._updatesToRemove.Add( callback );
		}

		public bool ExistUpdateMethod( TaskHandler callback )
		{
			return this._updates.Contains( callback );
		}

		/// <summary>
		/// 清空所有每个更新帧的回调函数
		/// </summary>
		public void ClearUpdateMethods()
		{
			this._updates.Clear();
		}
		#endregion

		#region Register FixedUpdate Method

		private readonly List<TaskHandler> _fixedUpdates = new List<TaskHandler>();
		private readonly List<TaskHandler> _fixedUpdatesToRemove = new List<TaskHandler>();

		/// <summary>
		/// 注册在每个物理模拟帧的回调函数
		/// </summary>
		/// <param name="callback">回调函数</param>
		public void RegisterFixedUpdateMethod( TaskHandler callback )
		{
			this._fixedUpdatesToRemove.Remove( callback );
			if ( this._fixedUpdates.Contains( callback ) )
				return;
			this._fixedUpdates.Add( callback );
		}

		/// <summary>
		/// 注销在每个物理模拟帧的回调函数
		/// </summary>
		/// <param name="callback">回调函数</param>
		public void UnregisterFixedUpdateMethod( TaskHandler callback )
		{
			if ( this._fixedUpdatesToRemove.Contains( callback ) )
				return;
			this._fixedUpdatesToRemove.Add( callback );
		}

		public bool ExistFixedUpdateMethod( TaskHandler callback )
		{
			return this._fixedUpdates.Contains( callback );
		}

		/// <summary>
		/// 清空所有每个物理模拟帧的回调函数
		/// </summary>
		public void ClearFixedUpdateMethods()
		{
			this._fixedUpdates.Clear();
		}
		#endregion

		#region Register LateUpdate Method

		private readonly List<TaskHandler> _lateUpdates = new List<TaskHandler>();
		private readonly List<TaskHandler> _lateUpdatesToRemove = new List<TaskHandler>();

		/// <summary>
		/// 注册在每个延迟更新帧的回调函数
		/// </summary>
		/// <param name="callback">回调函数</param>
		public void RegisterLateUpdateMethod( TaskHandler callback )
		{
			this._lateUpdatesToRemove.Remove( callback );
			if ( this._lateUpdates.Contains( callback ) )
				return;
			this._lateUpdates.Add( callback );
		}

		/// <summary>
		/// 注销在每个延迟更新帧的回调函数
		/// </summary>
		/// <param name="callback">回调函数</param>
		public void UnregisterLateUpdateMethod( TaskHandler callback )
		{
			if ( this._lateUpdatesToRemove.Contains( callback ) )
				return;
			this._lateUpdatesToRemove.Add( callback );
		}

		public bool ExistLateUpdateMethod( TaskHandler callback )
		{
			return this._lateUpdates.Contains( callback );
		}

		/// <summary>
		/// 清空所有每个延迟更新帧的回调函数
		/// </summary>
		public void ClearLateUpdateMethods()
		{
			this._lateUpdates.Clear();
		}
		#endregion

		#region Register OnGUI Method

		private readonly List<TaskHandler> _onGUIs = new List<TaskHandler>();
		private readonly List<TaskHandler> _onGUIsToRemove = new List<TaskHandler>();

		/// <summary>
		/// 注册在每个延迟更新帧的回调函数
		/// </summary>
		/// <param name="callback">回调函数</param>
		public void RegisterOnGUIMethod( TaskHandler callback )
		{
			this._onGUIsToRemove.Remove( callback );
			if ( this._onGUIs.Contains( callback ) )
				return;
			this._onGUIs.Add( callback );
		}

		/// <summary>
		/// 注销在每个延迟更新帧的回调函数
		/// </summary>
		/// <param name="callback">回调函数</param>
		public void UnregisterOnGUIMethod( TaskHandler callback )
		{
			if ( this._onGUIsToRemove.Contains( callback ) )
				return;
			this._onGUIsToRemove.Add( callback );
		}

		public bool ExistOnGUIMethod( TaskHandler callback )
		{
			return this._onGUIs.Contains( callback );
		}

		/// <summary>
		/// 清空所有每个延迟更新帧的回调函数
		/// </summary>
		public void ClearOnGUIMethods()
		{
			this._onGUIs.Clear();
		}
		#endregion

		void Update()
		{
			int count = this._updatesToRemove.Count;
			for ( int i = 0; i < count; i++ )
				this._updates.Remove( this._updatesToRemove[i] );
			this._updatesToRemove.Clear();

			count = this._updates.Count;
			for ( int i = 0; i < count; i++ )
				this._updates[i].Invoke( Time.deltaTime );

			////////////////schedule

			count = this._schedulesToRemove.Count;
			for ( int i = 0; i < count; i++ )
				this._schedules.Remove( this._schedulesToRemove[i] );
			this._schedulesToRemove.Clear();

			count = this._schedules.Count;
			for ( int i = 0; i < count; i++ )
			{
				ScheduleEntry scheduleEntry = this._schedules[i];
				scheduleEntry.OnUpdate( Time.deltaTime );
				if ( scheduleEntry.finished )
					this.UnregisterSchedule( scheduleEntry );
			}

			count = this._schedulesToRemove.Count;
			for ( int i = 0; i < count; i++ )
				this._schedules.Remove( this._schedulesToRemove[i] );
			this._schedulesToRemove.Clear();

			/////////////////// timer

			count = this._timersToRemove.Count;
			for ( int i = 0; i < count; i++ )
				this._timers.Remove( this._timersToRemove[i] );
			this._timersToRemove.Clear();

			count = this._timers.Count;
			for ( int i = 0; i < count; i++ )
			{
				TimerEntry timerEntry = this._timers[i];
				timerEntry.OnUpdate( Time.deltaTime );
				if ( timerEntry.finished )
					this.UnregisterTimer( timerEntry );
			}

			count = this._timersToRemove.Count;
			for ( int i = 0; i < count; i++ )
				this._timers.Remove( this._timersToRemove[i] );
			this._timersToRemove.Clear();
		}

		void FixedUpdate()
		{
			int count = this._fixedUpdatesToRemove.Count;
			for ( int i = 0; i < count; i++ )
				this._fixedUpdates.Remove( this._fixedUpdatesToRemove[i] );
			this._fixedUpdatesToRemove.Clear();

			count = this._fixedUpdates.Count;
			for ( int i = 0; i < count; i++ )
				this._fixedUpdates[i].Invoke( Time.deltaTime );
		}

		void LateUpdate()
		{
			int count = this._lateUpdatesToRemove.Count;
			for ( int i = 0; i < count; i++ )
				this._lateUpdates.Remove( this._lateUpdatesToRemove[i] );
			this._lateUpdatesToRemove.Clear();

			count = this._lateUpdates.Count;
			for ( int i = 0; i < count; i++ )
				this._lateUpdates[i].Invoke( Time.deltaTime );
		}

		void OnGUI()
		{
			int count = this._onGUIsToRemove.Count;
			for ( int i = 0; i < count; i++ )
				this._onGUIs.Remove( this._onGUIsToRemove[i] );
			this._onGUIsToRemove.Clear();

			count = this._onGUIs.Count;
			for ( int i = 0; i < count; i++ )
				this._onGUIs[i].Invoke( Time.deltaTime );
		}
	}
}