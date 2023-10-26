using Collapse.Provider;
using Collapse.Screens;
using Glacier.Common.Engine;
using Glacier.Common.Provider;
using Glacier.Common.Provider.Input;
using Glacier.Common.Util;
using GUI2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collapse
{
    internal class CollapseGame : GlacierGame
    {
        private ProviderManager Manager => manager;        
        private GameObjectManager ObjectManager;
        private AnimatedObjectProvider AnimatedObjectProvider;
        private LayoutAnimationProvider LayoutAnimationProvider;
        private InputProvider InputProvider;
        private ParticleProvider ParticleProvider;
        private CollapseTextProvider TextProvider;
        private GUIProvider GUIProvider;
        private ContentProvider ContentProvider;
        private AudioProvider AudioProvider;
        private GameplayScreen GameplayScreen;

        private SpriteFont Default;
        private Scoreboard Scoreboard;

        internal CollapseGame() : base(new Point(1024,800))
        {

        }

        protected override void Initialize()
        {
            //GLACIER SYS
            CreateGameResources();
            GameResources.Debug_HighlightHitboxes = false;      
            
            //REGISTER HANDLERS
            ObjectManager = Manager.Register(new GameObjectManager());
            AnimatedObjectProvider = Manager.Register(new AnimatedObjectProvider());
            LayoutAnimationProvider = Manager.Register(new LayoutAnimationProvider());
            InputProvider = Manager.Register(new InputProvider());
            ParticleProvider = Manager.Register(new ParticleProvider());
            TextProvider = Manager.Register(new CollapseTextProvider());

            //WINDOW SETTINGS
            Window.AllowAltF4 = true;
            Window.Title = "Spongebob SquarePants Collapse! Recreated by Jeremy Glazebrook";            
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
            
            base.Initialize();
        }

        protected override void LoadContent()
        {                        
            //REGISTER GLACIER CONTENT
            ContentProvider = Manager.Register(new ContentProvider(Content));
            GUIProvider = Manager.Register(new GUIProvider(GUI2.Controls.GLayer.FilteringModes.BilinearClamp));
            AudioProvider = Manager.Register(new AudioProvider());
            AudioProvider.Volume = .4f;

            //LOAD SPECIAL CONTENT
            var texture = ContentProvider.GetTexture("Assets/SprSheet");
            SBCollapseConstants.BlockTextures = //(int)SBCollapseConstants.SBCollapseTexAtlasIndices.END
                new Glacier.Common.Primitives.TextureAtlas(texture, 55, 1);
            texture = ContentProvider.GetTexture("Assets/IBUBBLES");
            SBCollapseConstants.BubbleTextures =
                new Glacier.Common.Primitives.TextureAtlas(texture, 8, 5);
            texture = ContentProvider.GetTexture("Assets/IMAGE8");
            SBCollapseConstants.ScoreboardText =
                new Glacier.Common.Primitives.TextureAtlas(texture, 1, 15);
            texture = ContentProvider.GetTexture("Assets/ILEVELNUM");
            SBCollapseConstants.LevelText =
                new Glacier.Common.Primitives.TextureAtlas(texture, 1, 15);
            Default = ContentProvider.GetContent<SpriteFont>("Text/Font_Regular");

            //CREATE A SCOREBOARD
            Scoreboard = new Scoreboard();

            //CREATE GAMESCREEN
            GameplayScreen = ObjectManager.Add(new GameplayScreen(GameResources.Screen));
            GameplayScreen.Initialize();

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //REFRESH ALL HANDLERS FOR THIS FRAME
            Manager.Refresh(gameTime);

            AddDebugString(Scoreboard.PrintDebugStrings());
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);
            spriteBatch.Begin(Microsoft.Xna.Framework.Graphics.SpriteSortMode.Deferred, null, Microsoft.Xna.Framework.Graphics.SamplerState.AnisotropicWrap);
            {
                //DRAW THE OBJECT MANAGER
                ObjectManager.Draw(spriteBatch);
                ParticleProvider.Draw(spriteBatch);
                TextProvider.Render(spriteBatch);
#if FALSE
            float DB_OPACITY = .85f;
            DrawGeneralDebuggingInformation(Default, DB_OPACITY);
#endif
            }
            spriteBatch.End();
            GUIProvider.Draw(GraphicsDevice);
            base.Draw(gameTime);
        }
    }
}
