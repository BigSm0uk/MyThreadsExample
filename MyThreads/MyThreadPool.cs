namespace MyThreads;

public sealed class MyThreadPool : IDisposable
{
    private readonly Thread[] _threads;
    private readonly Queue<Action> _actions;
    private readonly object _syncObject = new();
    private bool IsDisposed { get; set; }

    public MyThreadPool(int maxThreads = 4)
    {
        _threads = new Thread[maxThreads];
        _actions = new Queue<Action>();
        for (var i = 0; i < _threads.Length; i++)
        {
            _threads[i] = new Thread(ThreadProc)
            {
                IsBackground = true,
                Name = $"MyThreadPool Thread {i}"
            };
            _threads[i].Start();
        }
    }

    private void ThreadProc()
    {
        while (true)
        {
            Action action;
            Monitor.Enter(_syncObject);
            try
            {
                if (IsDisposed)
                {
                    return;
                }
                if (_actions.Count > 0)
                {
                    action = _actions.Dequeue();
                }
                else
                {
                    Monitor.Wait(_syncObject);
                    continue;
                }
            }
            finally
            {
                Monitor.Exit(_syncObject);
            }
            action.Invoke();
        }
    }


    public void Queue(Action action)
    {
        Monitor.Enter(_syncObject);
        try
        {
            _actions.Enqueue(action);
            if (_actions.Any())
            {
                Monitor.Pulse(_syncObject);
            }
        }
        finally
        {
            Monitor.Exit(_syncObject);
        }
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        var isDisposing = false;
        Monitor.Enter(_syncObject);
        try
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Monitor.PulseAll(_syncObject);
                isDisposing = true;
            }
        }
        finally
        {
            Monitor.Exit(_syncObject);
        }

        if (!isDisposing) return;
        foreach (var t in _threads)
        {
            t.Join();
        }
    }
}