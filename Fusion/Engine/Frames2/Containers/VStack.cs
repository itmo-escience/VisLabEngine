using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;

namespace Fusion.Engine.Frames2.Containers
{
    public class VStack : UIContainer
    {
        public VStack(float x, float y, float width, float height) : base(x, y, width, height)
        {
        }

        public override void Add(UIComponent child)
        {
            base.Add(child);
            RecalculateChildrenPositions();
        }

        public override bool Remove(UIComponent child)
        {
            var result = base.Remove(child);
            if(result)
                RecalculateChildrenPositions();

            return result;
        }

        private void RecalculateChildrenPositions()
        {
            var y = 0;
            foreach (var child in Children)
            {
                //child.
            }
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
