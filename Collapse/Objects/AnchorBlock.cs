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
    internal class AnchorBlock : GameBoardCell
    {
        bool textureSet = false;

        public override string SoundName => "AANCHOR"; 
        /// <summary>
        /// Creates a block of the assigned color
        /// <para>If anything other than a color is used here, undefined behavior is expected.</para>
        /// </summary>
        /// <param name="Color"></param>
        public AnchorBlock() : base(SpecialFlag.JELLY)
        {

        }

        public override void Initialize()
        {
            
        }

        private void SetTexture()
        {
            int row = (int)SBCollapseConstants.SBCollapseTexAtlasIndices.ANCHOR;
            SBCollapseConstants.BlockTextures.ApplyFrame(this, new Glacier.Common.Primitives.GridCoordinate(row, 0));            
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
