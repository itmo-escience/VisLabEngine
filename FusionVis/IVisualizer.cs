using System.Collections.Generic;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using FusionData.Data;

namespace FusionVis
{



    public struct VisualizerErrorType
    {
        //TODO

        //temporary
        public string ErrorText;
    }


    public interface IVisualizer
    {
        Dictionary<string, InputSlot> Inputs { get; }
        Dictionary<string, InputIndexSlot> IndexInputs { get; }

        RenderLayer VisLayer { get; }

        bool ValidateInputs();

        void Prepare();

        void LoadData();
        void UpdateFrame(GameTime gameTime);

        bool Ready { get; }

        TargetTexture Render { get; }

        void SetScreenArea(int x, int y, int width, int height);


    }



    //public abstract class VisualizerBase : IVisualizer
    //{
    //    public abstract List<InputSlot<DataOutputChannel>> Inputs { get; }
    //    public abstract List<InputSlot<Indexer>> IndexInputs { get; }


    //    public abstract RenderLayer VisLayer { get; protected set; }

    //    public abstract bool ValidateInputs();

    //    public abstract bool Prepare();

    //    public abstract bool UpdateData();

    //    public abstract bool UpdateFrame();



    //    public abstract void Activate();
    //    public abstract void Deactivate();
    //}
}
