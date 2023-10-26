using Collapse.Objects;
using Collapse.Provider;
using Glacier.Common.Engine;
using Glacier.Common.Primitives;
using Glacier.Common.Provider;
using Glacier.Common.Util;
using GUI2;
using GUI2.Controls;
using GUI2.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Collapse.Provider.CollapseTextProvider;

namespace Collapse.Screens
{
    internal class GameplayScreen : GameObject
    {
        private enum GameplayScreenState
        {
            GAMEPLAY,
            START,
            NEXTLEVEL,
            OPTIONS,
            HOWTO,
            LEVELCOMPLETE,
            GAMEOVER,
            BONUS
        }
        private GameplayScreenState _state = GameplayScreenState.START;
        TimeSpan timeSinceLastStateSwitch = TimeSpan.Zero;

        Vector2 LevelDialogOffset = new Vector2(70, 300);
        private Billboard BonusDialog, GameOverDialog, LevelCompleteDialog, StartDialog, LevelDialog, LevelCharacter;
        private GButton StartButton;

        private GameObjectManager ObjectManager;
        private GameBoard gameBoard;
        private ProviderManager Manager;
        private BoardProvider BoardProvider;
        protected override bool AutoInitialize => false;

        public int Level { get; set; } = 1;

        private TextRef ScoreText, LinesText, LevelText, LevelTitle;

        public GameplayScreen(Rectangle ScreenBoundary) : base("Backgrounds/IMAGE1", new Vector2(), default)
        {
            Manager = ProviderManager.Root;
            BoardProvider = Manager.Register(new BoardProvider());
            ObjectManager = Manager.Get<GameObjectManager>();
        }

        public override void Initialize()
        {
            Level = GameSettings.Default.StartingLevel;
            SetupScreen();
        }

        private void CleanPreviousSession()
        {
            if(gameBoard != null)
            {
                ObjectManager.Remove(gameBoard);
                gameBoard.Dispose();
                gameBoard = null;
            }
        }

        private void StartGame()
        {            
            // CLEAN UP
            CleanPreviousSession();

            //HIDE UI ELEMENTS
            switchState(GameplayScreenState.GAMEPLAY);

            //GENERATE LEVEL
            int LEVEL = Level;
            gameBoard = BoardProvider.Generate(LEVEL);                
            ObjectManager.Add(gameBoard);
            gameBoard.Resume();

            //PLAY MUSIC
            ProviderManager.Root.Get<AudioProvider>().PlayMusic(SBCollapseConstants.GameMusic1);
        }

        private void SetupScreen()
        {                        
            //SETUP SCOREBOARD TEXT
            var textProvider = ProviderManager.Root.Get<CollapseTextProvider>();
            var atlas = SBCollapseConstants.ScoreboardText;
            float textScale = 1.5f;
            ScoreText =
                new TextRef(atlas, $"0", SBCollapseInterop.Transform(SBCollapseConstants.ScoreTextLocation).ToVector2())
                {
                    Scale = textScale,
                    Centered = true
                };
            LevelText =
                new TextRef(atlas, Scoreboard.Default.Level.ToString(), 
                SBCollapseInterop.Transform(SBCollapseConstants.LevelTextLocation).ToVector2())
                {
                    Scale = textScale,
                    Centered = true
                };
            LinesText =
                new TextRef(atlas, "0", 
                    SBCollapseInterop.Transform(SBCollapseConstants.LinesTextLocation).ToVector2())
                {
                    Scale = textScale,
                    Centered = true
                };
            atlas = SBCollapseConstants.LevelText;
            LevelTitle =
                new TextRef(atlas, "0", default)
                {
                    Hidden = true,
                    Centered = true
                };
            textProvider.Add(ScoreText);
            textProvider.Add(LinesText);
            textProvider.Add(LevelText);
            textProvider.Add(LevelTitle);

            //SETUP BILLBOARDS
            SetupUILayer();

            //SETUP GUI2 CONTROLS            
            SetupGUI2Layer();

            switchState(GameplayScreenState.GAMEPLAY);            
            switchState(GameplayScreenState.START);
        }

        private void SetupUILayer()
        {
            BonusDialog = new Billboard("Assets/IMAGE6",
                new Vector2(),
                new Point())
            {
                Visible = false
            };
            GameOverDialog = new Billboard("Assets/IMAGE4",
                new Vector2(),
                new Point())
            {
                Visible = false
            };
            LevelCompleteDialog = new Billboard("Assets/IMAGE5",
                new Vector2(),
                new Point())
            {
                Visible = false
            };
            StartDialog = new Billboard("Assets/ISTARTDLG",
                new Vector2(),
                new Point())
            {
                Visible = false
            };
            LevelDialog = new Billboard("Assets/ILEVELBG", 
                default,
                default)
            {
                Visible = false
            };
            LevelCharacter = new Billboard("Assets/LEVELCH1",
                default, default)
            {
                Visible = false
            };
            ObjectManager.Add(BonusDialog, 3);
            ObjectManager.Add(GameOverDialog, 3);
            ObjectManager.Add(LevelCompleteDialog, 3);
            ObjectManager.Add(StartDialog, 3);
            ObjectManager.Add(LevelDialog, 3);
            ObjectManager.Add(LevelCharacter, 3);
        }

        private void SetupGUI2Layer()
        { // SETUP GUI2 CONTROLS
            var content = ProviderManager.Root.Get<ContentProvider>();
            var gui2 = ProviderManager.Root.Get<GUIProvider>();

            var texture = content.GetTexture("Assets/ISTARTBTN");
            var atlas = new TextureAtlas(texture, 2, 1);
            var Gtexture = texture.ToGTexture();
            var frame = atlas.GetFrame(new GridCoordinate(0, 0));
            Gtexture.Source = frame;
            StartButton = new GButton(Gtexture)
            {
                DesiredSize = SBCollapseInterop.Transform(
                    new Point(frame.Width, frame.Height)),
                AutosizeToChild = false
            };
            StartButton.OnMouseEnter += delegate { Gtexture.Source = atlas.GetFrame(new GridCoordinate(1, 0)); };
            StartButton.OnMouseLeave += delegate { Gtexture.Source = frame; };
            StartButton.OnClick += delegate {
                Scoreboard.Default.Score = 0;    
                StartGame(); 
            };
            Gtexture.DesiredSize = StartButton.DesiredSize;
            Gtexture.SizeMode = GTexture.SizeModes.Stretch;
            gui2.Default.AddChild(StartButton);
        }

        private void UpdateUI(GameTime gt)
        {
            Size = SBCollapseInterop.Transform(SBCollapseConstants.OriginalResolution);
            BonusDialog.Position =
                SBCollapseInterop.Transform(SBCollapseConstants.BonusScoreDialogLocation + new Point(45, 0)).ToVector2();
            GameOverDialog.Position =
                SBCollapseInterop.Transform(SBCollapseConstants.GameOverDialogLocation + new Point(45, 0)).ToVector2();
            LevelCompleteDialog.Position =
                SBCollapseInterop.Transform(SBCollapseConstants.GameOverDialogLocation + new Point(25, 0)).ToVector2();
            StartDialog.Position =
                SBCollapseInterop.Transform(SBCollapseConstants.StartDialogLocation).ToVector2();
            StartDialog.Size =
                SBCollapseInterop.Transform(new Point(270));
            LevelDialog.Position = LevelDialogOffset +
                SBCollapseInterop.Transform(SBCollapseConstants.LevelDialogLocation).ToVector2();
            StartButton.DesiredPosition = SBCollapseInterop.Transform(SBCollapseConstants.StartButtonLocation);
            LevelTitle.Position = LevelDialogOffset + SBCollapseInterop.Transform(SBCollapseConstants.LevelNumberLocation)
                - new Vector2(40,25);
            LevelTitle.Text = Level.ToString();

            Scoreboard.Default.Update(gt);

            LinesText.Text = Scoreboard.Default.LinesLeft.ToString();
            LevelText.Text = Scoreboard.Default.Level.ToString();
            ScoreText.Text = Scoreboard.Default.DrawScore.ToString();
        }

        public override void Update(GameTime gt)
        {
            UpdateUI(gt);

            if (gameBoard != null)
            {
                if (gameBoard.IsGameOver)
                {
                    if (gameBoard.IsAnimatingBoard)
                        switchState(GameplayScreenState.GAMEOVER);
                    else switchState(GameplayScreenState.START);
                }
                else if (gameBoard.LevelCompleted)
                {
                    if (gameBoard.IsAnimatingBoard)
                        switchState(GameplayScreenState.LEVELCOMPLETE);
                    else switchState(GameplayScreenState.NEXTLEVEL);
                }
                else if (gameBoard.BonusAchieved)
                    switchState(GameplayScreenState.BONUS);
                else if (gameBoard.IsGameRunning && timeSinceLastStateSwitch.TotalSeconds > 3)
                    switchState(GameplayScreenState.GAMEPLAY);
            }
            timeSinceLastStateSwitch += gt.ElapsedGameTime;
        }

        private void switchState(GameplayScreenState State)
        {
            if (State != _state)
                timeSinceLastStateSwitch = TimeSpan.Zero;
            switch (State)
            {
                case GameplayScreenState.START:                    
                    StartDialog.Visible = true;
                    StartButton.Availability = GUIComponent.Availabilities.Enabled;
                    break;
                case GameplayScreenState.NEXTLEVEL:                    
                    LevelDialog.Visible = true;
                    Point location = SBCollapseConstants.Level1Position;
                    switch (Level)
                    {
                        case 2:
                            location = SBCollapseConstants.Level2Position;
                            break;
                        case 3:
                            location = SBCollapseConstants.Level3Position;
                            break;
                        case 4:
                            location = SBCollapseConstants.Level4Position;
                            break;
                        case 5:
                            location = SBCollapseConstants.Level5Position;
                            break;
                        case 6:
                            location = SBCollapseConstants.Level6Position;
                            break;
                    }
                    var _level = Level;
                    if (Level > 6)
                        _level -= Level * (Level / 6);
                    if (_level < 1) _level = 1;
                    LevelCharacter.SetTexture("Assets/LEVELCH" + _level);
                    LevelCharacter.Visible = true;
                    LevelCharacter.Position = LevelDialogOffset + 
                        SBCollapseInterop.Transform(location - new Point((LevelCharacter.Width / 4) + 3,0)).ToVector2();
                    LevelCharacter.Size = new Point(LevelCharacter.Texture.Width, LevelCharacter.Texture.Height);
                    LevelTitle.Hidden = false;
                    if (timeSinceLastStateSwitch.TotalSeconds > 2)
                    {
                        StartGame();
                        State = GameplayScreenState.GAMEPLAY;
                        return;
                    }
                    break;
                case GameplayScreenState.GAMEOVER:
                    GameOverDialog.Visible = true;
                    break;
                case GameplayScreenState.BONUS:
                    BonusDialog.Visible = true;
                    break;
                case GameplayScreenState.LEVELCOMPLETE:
                    Level = Scoreboard.Default.Level + 1;
                    LevelCompleteDialog.Visible = true;
                    break;
                default:
                case GameplayScreenState.GAMEPLAY:
                    LevelTitle.Hidden = true;
                    LevelCharacter.Visible = false;
                    LevelDialog.Visible = false;
                    BonusDialog.Visible = false;
                    LevelCompleteDialog.Visible = false;
                    GameOverDialog.Visible = false;
                    StartDialog.Visible = false;
                    StartButton.Availability = GUIComponent.Availabilities.Invisible;
                    break;
            }            
            _state = State;
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);
        }
    }
}
