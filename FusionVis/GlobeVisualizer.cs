using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using FusionData.Data;

namespace FusionVis
{
    class GlobeVisualizer : IVisualizer
    {
        public Dictionary<string, InputSlot> Inputs { get; }
        public Dictionary<string, InputIndexSlot> IndexInputs { get; }
        public RenderLayer VisLayer { get; }
        public bool ValidateInputs()
        {
            return true;
        }

        public void Prepare()
        {
        }

        public void LoadData()
        {
        }

        public void UpdateFrame(GameTime gameTime)
        {
        }

        public bool Ready { get; }
        public TargetTexture Render { get; }
        public void SetScreenArea(int x, int y, int width, int height)
        {
        }
    }
}
