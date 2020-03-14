using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Utilities
{
    public class CachingFunction<T, R>
    {
        private readonly Func<T, R> _function;
        private readonly Dictionary<T, CacheValue> _cache;

        public class CacheValue
        {
            public CacheValue(R value)
            {
                Stopwatch = new Stopwatch();
                Stopwatch.Start();
                Value = value;
            }

            public Stopwatch Stopwatch { get; }

            public R Value { get; }
        }

        public CachingFunction(Func<T, R> function, int timeLimit)
        {
            _function = function;
            _cache = new Dictionary<T, CacheValue>();

            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(500);
                    updateCache(timeLimit);
                }
            }).Start();
        }

        public R Apply(T arg)
        {
            if (_cache.ContainsKey(arg))
            {
                return _cache[arg].Value;
            }
            else
            {
                var res = _function.Invoke(arg);
                _cache.Add(arg, new CacheValue(res));

                return res;
            }
        }

        private void updateCache(int timeLimit)
        {
            foreach (var keyValuePair in _cache)
            {
                var stopwatch = keyValuePair.Value.Stopwatch;
                stopwatch.Stop();

                if (stopwatch.ElapsedMilliseconds >= timeLimit)
                {
                    _cache.Remove(keyValuePair.Key);
                }
                else
                {
                    stopwatch.Start();
                }
            }
        }
    }
}
