using System;
using System.Collections.Generic;
using System.Text;

namespace MyUtils.Event
{
    /// <summary>
    /// https://stackoverflow.com/a/52651996/4684232
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventArgs<T> : EventArgs
    {
        public T Value { get; private set; }

        public EventArgs(T val)
        {
            Value = val;
        }

    }
}
