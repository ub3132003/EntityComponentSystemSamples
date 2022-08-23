using System;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
public class StatVitalityEventBuffers<T> : IBufferElementData where T : unmanaged, IComparable<T>
{
    public NativeList<T> Previous;
    public NativeList<T> Current;

    public StatVitalityEventBuffers()
    {
        Previous = new NativeList<T>(Allocator.Persistent);
        Current = new NativeList<T>(Allocator.Persistent);
    }

    public void Dispose()
    {
        if (Previous.IsCreated) Previous.Dispose();
        if (Current.IsCreated) Current.Dispose();
    }

    public void SwapBuffers()
    {
        var tmp = Previous;
        Previous = Current;
        Current = tmp;
        Current.Clear();
    }

    public static void GetStatefulEvents(NativeList<T> previousEvents, NativeList<T> currentEvents, NativeList<T> statefulEvents, bool sortCurrent = true)
    {
        if (sortCurrent) currentEvents.Sort();

        statefulEvents.Clear();

        int c = 0;
        int p = 0;
        while (c < currentEvents.Length && p < previousEvents.Length)
        {
            int r = previousEvents[p].CompareTo(currentEvents[c]);
            if (r == 0)
            {
                var currentEvent = currentEvents[c];

                statefulEvents.Add(currentEvent);
                c++;
                p++;
            }
            else if (r < 0)
            {
                var previousEvent = previousEvents[p];

                statefulEvents.Add(previousEvent);
                p++;
            }
            else //(r > 0)
            {
                var currentEvent = currentEvents[c];

                statefulEvents.Add(currentEvent);
                c++;
            }
        }
        if (c == currentEvents.Length)
        {
            while (p < previousEvents.Length)
            {
                var previousEvent = previousEvents[p];

                statefulEvents.Add(previousEvent);
                p++;
            }
        }
        else if (p == previousEvents.Length)
        {
            while (c < currentEvents.Length)
            {
                var currentEvent = currentEvents[c];

                statefulEvents.Add(currentEvent);
                c++;
            }
        }
    }
}
