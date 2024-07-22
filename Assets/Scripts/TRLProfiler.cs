using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 
/// <code>使用方式：
/// using (new TRLProfiler("test"))
/// {
///     xxx
/// }
/// </code>
/// </summary>
public class TRLProfiler : IDisposable
{
    private string key;
    private System.Diagnostics.Stopwatch m_SW = null;
    public TRLProfiler(string profileKey)
    {
        key = profileKey;
        m_SW = new System.Diagnostics.Stopwatch();

        m_SW.Reset();
        m_SW.Start();
    }

    // Start is called before the first frame update
    public void Dispose()
    {
        m_SW.Stop();
        Debug.Log($"{key}"+"耗时:"+m_SW.Elapsed.TotalMilliseconds.ToString()+"毫秒");
    }
}
