using Collapse.Provider;
using Glacier.Common.Engine;
using Glacier.Common.Provider;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collapse.Objects
{
    internal class StarBlock : GameBoardCell
    {
        bool textureSet = false;
        /// <summary>
        /// Creates a block of the assigned color
        /// <para>If anything other than a color is used here, undefined behavior is expected.</para>
        /// </summary>
        /// <param name="Color"></param>
        public StarBlock() : base(SpecialFlag.JELLY)
        {

        }

        public override void Initialize()
        {
            
        }

        private void SetTexture()
        {
            int row = (int)SBCollapseConstants.SBCollapseTexAtlasIndices.STAR;
            SBCollapseConstants.BlockTextures.ApplyFrame(this, new Glacier.Common.Primitives.GridCoordinate(row, 0));
            /*ProviderManager.Root.Get<AnimatedObjectProvider>().Animate(
                new SourceFrameAnimationDefinition<GameObject>(this,
                       SBCollapseConstants.BlockTextures,
                       new Glacier.Common.Primitives.GridCoordinate(row, 0),
                       new Glacier.Common.Primitives.GridCoordinate(row + 5, 0),
                       AnimationDimension.Rows
                )
                {
                    Infinite = true,
                    Paused = false,
                    Timestep = TimeSpan.FromSeconds(.2)
                });*/
            textureSet = true;
        }

        public override void Update(GameTime gt)
        {
            if (!textureSet)
                SetTexture();
            base.Update(gt);
        }
    }
}
