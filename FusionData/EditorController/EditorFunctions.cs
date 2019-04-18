using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FusionData.DataModel.Public;
using FusionData._0._2;

namespace FusionData.Editor
{
    public static class EditorFunctions
    {

        public static ICollection<MethodInfo> GetAllDefaultNodeConstructors()
        {
            return Assembly.GetAssembly(typeof(EditorFunctions)).GetTypes()
                .Where(a => a.GetCustomAttributes<NodeFactoryAttribute>().Any()).SelectMany(a =>
                    a.GetMethods().Where(m => m.GetCustomAttributes<NodeConstructorAttribute>().Any() && m.ReturnType.GetInterfaces().Contains(typeof(INode)))).ToList();
        }

        static Type GetOutputLinkageType(object output)
        {
            if (output is IDataChannel o) return o.Type;
            return output.GetType();
        }

        public static void LinkNodes(IDataProvider from, IDataConsumer to, string fromChannelName, string toSlotName)
        {
            from.ReCalc();
            to.ReCalc();
            if (!from.OutputChannels.ContainsKey(fromChannelName)) throw new ArgumentException("No such channel on 'from' node");
            var toSlot = to.InputSlots.First(a => a.Name.Equals(toSlotName));
            if (toSlot == null) throw new ArgumentException("No such slot on 'to' node");
            var fromSlot = from.OutputChannels[fromChannelName];
            if (GetOutputLinkageType(fromSlot).IsSubclassOf(toSlot.Type)) throw new ArgumentException("Channel and slot types mismatch");

            if (toSlot is IChannelSlot)
            {
                if (fromSlot is IDataChannel) toSlot.Content = fromSlot;
                else toSlot.Default = fromSlot;
            } else if (toSlot is IParameterSlot)
            {
                if (fromSlot is IDataChannel) throw new ArgumentException("Trying to assign channel co parameter slot");
                else toSlot.Content = fromSlot;
            }
            else
            {
                throw new NotImplementedException("Such case is not implemented yet");
            }






        }



    }
}
