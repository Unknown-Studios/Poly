using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class ThreadedJob
{
    public Queue<ProceduralSphere.ProcCallback> queue;
    public int Seed;

    private Thread _thread;
    private ProceduralSphere.ProcCallback current;

    public virtual void Add(ProceduralSphere.ProcCallback item)
    {
        if (queue == null)
        {
            queue = new Queue<ProceduralSphere.ProcCallback>();
        }
        queue.Enqueue(item);
    }

    public virtual void Abort()
    {
        _thread.Abort();
    }

    public virtual IEnumerator Start(int seed)
    {
        Seed = seed;
        bool done = false;

        ProceduralSphere.MeshData data = new ProceduralSphere.MeshData();

        while (queue.Count > 0)
        {
            if (done)
            {
                current.callback(data);
                current = null;
                data = new ProceduralSphere.MeshData();
            }
            done = false;
            if (current == null)
            {
                current = queue.Dequeue();
                ThreadStart threadStart = delegate
                {
                    data = current.Function();
                    done = true;
                };
                _thread = new Thread(threadStart);
                _thread.Start();
            }
            yield return null;
        }
    }
}