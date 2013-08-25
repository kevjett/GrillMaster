using System;
using Microsoft.SPOT;

namespace GrillMaster
{
    public interface IProbe
    {
        double CurrentTemp { get; }
    }
}
