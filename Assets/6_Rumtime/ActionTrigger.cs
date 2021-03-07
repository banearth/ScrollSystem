using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件接口
/// </summary>
public interface IActionTrigger : IDisposable
{
    void Add(Delegate del);
    void Remove(Delegate del);
    void OnAction(params object[] args);
}

/// <summary>
/// 无参数
/// </summary>
public sealed class ActionTrigger : IActionTrigger
{
    private Action action;

    public ActionTrigger(Action action)
    {
        Add(action);
    }

    public void Dispose()
    {
        this.action = null;
    }

    public void Add(Delegate del)
    {
        this.action += (Action)del;
    }

    public void Remove(Delegate del)
    {
        this.action -= (Action)del;
    }

    public void OnAction(params object[] args)
    {
        this.action?.Invoke();
    }
}

/// <summary>
/// 带一个参数
/// </summary>
public sealed class ActionTrigger<T> : IActionTrigger
{
    private Action<T> action;

    public ActionTrigger(Action<T> action)
    {
        Add(action);
    }

    public void Dispose()
    {
        this.action = null;
    }

    public void Add(Delegate del)
    {
        this.action += (Action<T>)del;
    }

    public void Remove(Delegate del)
    {
        this.action -= (Action<T>)del;
    }

    public void OnAction(params object[] args)
    {
        this.action?.Invoke((T)args[0]);
    }
}

/// <summary>
/// 带两个参数
/// </summary>
public sealed class ActionTrigger<T1, T2> : IActionTrigger
{
    private Action<T1, T2> action;

    public ActionTrigger(Action<T1, T2> action)
    {
        Add(action);
    }

    public void Dispose()
    {
        this.action = null;
    }

    public void Add(Delegate del)
    {
        this.action += (Action<T1, T2>)del;
    }

    public void Remove(Delegate del)
    {
        this.action -= (Action<T1, T2>)del;
    }

    public void OnAction(params object[] args)
    {
        this.action?.Invoke((T1)args[0], (T2)args[1]);
    }
}
