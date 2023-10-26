using Glacier.Common.Engine;
using Glacier.Common.Provider;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collapse.Objects
{
    internal class BubbleParticle : Particle
    {
        internal BubbleParticle(SBCollapseConstants.SBCollapseBubbleTexAtlasColumns Type, Vector2 StartPosition)
            : base(default(Texture2D), StartPosition, Point.Zero)
        {
            this.Type = Type;
            SetTexture();
        }

        public SBCollapseConstants.SBCollapseBubbleTexAtlasColumns Type { get; }

        public override void Initialize()
        {
            Acceleration = new Vector2(0, -100);
            Velocity = new Vector2(0, -250);
            FadeOutTime = TimeSpan.FromSeconds(1);
            ExpireTime = TimeSpan.FromSeconds(1.5);
            base.Initialize();
        }

        private void SetTexture()
        {
            ProviderManager.Root.Get<AnimatedObjectProvider>().Animate(
                new SourceFrameAnimationDefinition(this,
                       SBCollapseConstants.BubbleTextures,
                       new Glacier.Common.Primitives.GridCoordinate(0, (int)Type),
                       new Glacier.Common.Primitives.GridCoordinate(SBCollapseConstants.BubbleTextures.Rows, (int)Type),
                       AnimationDimension.Rows
                )
                {
                    Paused = false,
                    Timestep = TimeSpan.FromSeconds(.2),
                    StoryboardMode = AnimationStoryboard.Forward
                });
        }
    }
}
