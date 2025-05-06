using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetQuikConnector
{
    /// <summary>
    /// Marks method as used in Quik Lua
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class QuikLuaMethod : Attribute
    {
    }
}
