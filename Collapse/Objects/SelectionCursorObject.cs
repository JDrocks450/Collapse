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
    internal class SelectionCursorObject : GameObject
    {
        bool textureSet = false;
        /// <summary>
        /// Creates a block of the assigned color
        /// <para>If anything other than a color is used here, undefined behavior is expected.</para>
        /// </summary>
        /// <param name="Color"></param>
        public SelectionCursorObject(Vector2 Position) : base("Assets/IHIGHLGT", Position, SBCollapseInterop.Transform(new Point(36,36)))
        {

        }

        public override void Initialize()
        {
            
        }        

        public override void Update(GameTime gt)
        {
            Size = SBCollapseInterop.Transform(new Point(36, 36));
        }
    }
}
