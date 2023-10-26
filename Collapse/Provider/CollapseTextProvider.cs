using Glacier.Common.Primitives;
using Glacier.Common.Provider;
using Glacier.Common.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collapse.Provider
{
    internal class CollapseTextProvider : IProvider
    {
        internal class TextRef
        {            
            internal TextureAtlas Source { get; private set; }
            public string Text { get; set; }
            public Vector2 Position { get; set; }
            public float Scale { get; set; } = 1;
            public bool Centered { get; set; } = false;
            public bool Hidden { get; set; } = false;

            internal TextRef(TextureAtlas TextureRef, string Text, Vector2 Position)
            {
                this.Source = TextureRef;
                this.Text = Text;
                this.Position = Position;
            }
        }

        public ProviderManager Parent { set; get; }
        private List<TextRef> references = new List<TextRef>();

        public void Add(in TextRef Reference) => references.Add(Reference);

        public void Refresh(GameTime time)
        {
            
        }

        public void Render(GlacierSpriteBatch Batch)
        {
            for(int i = 0; i < references.Count; i++)
            {                
                TextRef text = references[i];
                if (text.Hidden) continue;
                renderReference(Batch, text);
            }    
        }

        private void renderReference(GlacierSpriteBatch Batch, TextRef text)
        {
            var rect = new Rectangle(text.Position.ToPoint(), 
                (text.Source.CellSize.ToVector2() * new Vector2(text.Scale)).ToPoint());
            if (text.Centered)
                rect.X -= (int)((text.Text.Length * rect.Width) / 2.0);
            foreach(char c in text.Text)
            {
                int column = 0;
                if (c == '-')
                    column = 1;
                if (c == ',')
                    column = 2;
                if (c == '.')
                    column = 3;
                if (char.IsDigit(c))
                    column = 5 + c - '0';
                var textureAtlas = text.Source;
                Batch.Draw(textureAtlas.Texture,
                    rect, textureAtlas.GetFrame(new GridCoordinate(0, column)), Color.White);
                rect.X += rect.Width;
            }
        }
    }
}
