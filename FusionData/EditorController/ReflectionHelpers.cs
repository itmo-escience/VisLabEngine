using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionData._0._2
{
    [AttributeUsage(AttributeTargets.Class)]
    sealed class NodeFactoryAttribute : Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public NodeFactoryAttribute()
        {
            // TODO: Implement code here
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    sealed class NodeConstructorAttribute : Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public NodeConstructorAttribute()
        {
            // TODO: Implement code here
        }
    }
}
