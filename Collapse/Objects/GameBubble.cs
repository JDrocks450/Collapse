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
    internal class GameBubble : GameBoardCell
    {
        bool textureSet = false;
        /// <summary>
        /// Creates a block of the assigned color
        /// <para>If anything other than a color is used here, undefined behavior is expected.</para>
        /// </summary>
        /// <param name="Color"></param>
        public GameBubble(SpecialFlag Color) : base(Color)
        {

        }

        public override void Initialize()
        {
            
        }

        private void SetTexture()
        {
            int row = 0;
            switch (Flag)
            {
                case SpecialFlag.BLUE:
                    row = (int)SBCollapseConstants.SBCollapseTexAtlasIndices.BUBBLE_BLU;
                    break;
                case SpecialFlag.GREEN:
                    row = (int)SBCollapseConstants.SBCollapseTexAtlasIndices.BUBBLE_GRE;
                    break;
                case SpecialFlag.PURPLE:
                    row = (int)SBCollapseConstants.SBCollapseTexAtlasIndices.BUBBLE_PUR;
                    break;
                case SpecialFlag.YELLOW:
                    row = (int)SBCollapseConstants.SBCollapseTexAtlasIndices.BUBBLE_YEL;
                    break;
            }
            SBCollapseConstants.BlockTextures.ApplyFrame(this, new Glacier.Common.Primitives.GridCoordinate(row, 0));
            ProviderManager.Root.Get<AnimatedObjectProvider>().Animate(
                new SourceFrameAnimationDefinition(this,
                       SBCollapseConstants.BlockTextures,
                       new Glacier.Common.Primitives.GridCoordinate(row, 0),
                       new Glacier.Common.Primitives.GridCoordinate(row + 5, 0),
                       AnimationDimension.Rows
                )
                {
                    Infinite = true,
                    Paused = false,
                    Timestep = TimeSpan.FromSeconds(.2)
                });
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
