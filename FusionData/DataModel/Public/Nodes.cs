using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionData.DataModel.Public
{



    public interface INode
    {
        bool CheckValidity();
        void ReCalc();
    }

    public interface IDataProvider : INode
    {
        /// <summary>
        /// List of channels
        /// </summary>
        Dictionary<string, object> OutputChannels { get; }

        IDataChannel<string> KeyChannel { get; }

        string GetKeyByIndex(int index);
    }

    public interface IDataConsumer : INode
    {
        List<ISlot> InputSlots { get; }

    }

    public interface IIONode : IDataProvider, IDataConsumer
    {
    }


}
