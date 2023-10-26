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
    internal class GameBlock : GameBoardCell
    {
        bool textureSet = false;
        /// <summary>
        /// Creates a block of the assigned color
        /// <para>If anything other than a color is used here, undefined behavior is expected.</para>
        /// </summary>
        /// <param name="Color"></param>
        public GameBlock(SpecialFlag Color) : base(Color)
        {

        }

        public override void Initialize()
        {
            
        }

        private void SetTexture()
        {
            int row = 0;
            bool animate = false;
            switch (Flag)
            {
                case SpecialFlag.BLUE:
                    row = 0;
                    break;
                case SpecialFlag.GREEN:
                    row = 2;
                    break;
                case SpecialFlag.PURPLE:
                    row = 1;
                    break;
                case SpecialFlag.YELLOW:
                    row = 3;
                    break;
                case SpecialFlag.AQUA:
                    row = (int)SBCollapseConstants.SBCollapseTexAtlasIndices.AQUA_BLOCK_ANIM;
                    animate = true;
                    break;
            }
            SBCollapseConstants.BlockTextures.ApplyFrame(this, new Glacier.Common.Primitives.GridCoordinate(row, 0));
            if (animate)
            {
                ProviderManager.Root.Get<AnimatedObjectProvider>().Animate(
                new SourceFrameAnimationDefinition(this,
                       SBCollapseConstants.BlockTextures,
                       new Glacier.Common.Primitives.GridCoordinate(row, 0),
                       new Glacier.Common.Primitives.GridCoordinate(row + 4, 0),
                       AnimationDimension.Rows
                )
                {
                    Infinite = true,
                    Paused = false,
                    Timestep = TimeSpan.FromSeconds(.2),
                    StoryboardMode = AnimationStoryboard.ForwardReverse
                });
            }
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
