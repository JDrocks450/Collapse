using Glacier.Common.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collapse.Objects
{
    public class Billboard : GameObject
    {
        public Billboard(string texKey, Vector2 Position, Point Size) : base(texKey, Position, Size)
        {

        }

        public Billboard(Texture2D texture, Vector2 Position, Point Size) : base(texture, Position, Size)
        {

        }

        public override void Initialize()
        {
            
        }

        public override void Update(GameTime gt)
        {
            if (Size == Point.Zero)
                Size = new Point(Texture.Width, Texture.Height);
        }
    }
}
