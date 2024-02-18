using MyThreads;

using (new Mutex(true, "MyMutextPrivet", out var mutex))
{
    if (!mutex) return;

    using (var pool = new MyThreadPool())
    {
        pool.Queue(() => Console.WriteLine("From ThreadPool"));
    }

    Console.ReadKey();
}

