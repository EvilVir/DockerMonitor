using System;

namespace StrangeFog.Docker.Monitor.Configuration
{
    [Flags]
    public enum ContainersGroupState
    {
        AnyDown = 1,
        AnyUp = 2,
        AnyMissing = 4,
        AllDown = 8,
        AllUp = 16,
        AllMissing = 32
    }
}
