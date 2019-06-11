using System.Collections.Generic;

namespace StrangeFog.Docker.Monitor.Extensions
{
    public static class EnumExtensions
    {
        public static int CombineFlags(IEnumerable<int> values)
        {
            var output = 0;

            foreach(var value in values)
            {
                output |= value;
            }

            return output;
        }
    }
}
