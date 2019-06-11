using System;

namespace StrangeFog.Docker.Monitor.Extensions
{
    public static class ArrayExtensions
    {
        public static T TryGetAt<T>(this T[] array, int idx, T defaultValue = default(T), Func<T, bool> filter = null)
        {
            var output = array != null && array.Length > idx ? array[idx] : defaultValue;
            return filter == null || filter.Invoke(output) ? output : defaultValue;
        }
    }
}
