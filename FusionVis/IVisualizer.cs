using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;
using FusionData._0._2;

namespace FusionVis._0._2
{
    public interface IVisualizer : FusionData.DataModel.Public.IDataConsumer
    {
        VisLayerHolder VisHolder { get; }

        void UpdateVis(GameTime gameTime);
    }



}
