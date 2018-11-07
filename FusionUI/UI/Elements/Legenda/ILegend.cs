using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FusionUI.UI;

namespace FusionUI.Legenda
{
    public interface ILegend
    {
        void Init();        

        string Name { get; set; }

        float Width { get; set; }

        ScalableFrame LegendFrame { get; }
    }
}
