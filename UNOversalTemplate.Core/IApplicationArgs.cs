using System;
using System.Collections.Generic;
using System.Text;

namespace UNOversal
{
    public interface IApplicationArgs
    {
        object Arguments { get; }
        StartCauses StartCause { get; }
    }
}
