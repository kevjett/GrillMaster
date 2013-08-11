using System;
using Microsoft.SPOT;

namespace GrillMaster
{
    public interface IProbe
    {
        string Name { get; }
        Config.ProbeType ProbeType { get; }
        int TargetTemp { get; set; }
        int TargetPercentRemaining { get; }
        long StateChangedTime { get; set; }
        bool HasTemp();
        void PreReadTemp();
        double GetTemp();
        bool IsTargetReached();
        double GetPercentUnderTarget();
    }
}
