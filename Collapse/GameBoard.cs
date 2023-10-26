using Collapse.Objects;
using Glacier.Common.Engine;
using Glacier.Common.Primitives;
using Glacier.Common.Provider;
using Glacier.Common.Provider.Input;
using Glacier.Common.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using static Collapse.Provider.GameBoardCell;
using static Glacier.Common.Provider.KeyframedLayoutAnimationDefinition;

namespace Collapse.Provider
{
    /// <summary>
    /// Represents a cell in a gameboard -- can have a state, texture, location in the grid.
    /// </summary>
    internal abstract class GameBoardCell : GameObject
    {
        internal GameBoard Parent { get; set; }
        internal GridCoordinate BoardCoordinate { get; set; }

        public bool IsMouseOver { get; private set; } = false;

        internal enum SpecialFlag
        {
            EMPTY,
            BLUE,
            GREEN,
            YELLOW,
            PURPLE, 
            JELLY,
            ANCHOR,
            STAR,
            AQUA
        }
        public SpecialFlag Flag { get; protected set; }

        public virtual string SoundName => "ABBB";

        protected GameBoardCell(SpecialFlag Flag) : base(default(Texture2D), default, default)
        {
            this.Flag = Flag;
        }

        public override void Update(GameTime gt)
        {
            if (Parent == null) return;
            if (Parent.MouseInsidePlayArea) {
                var mouseRect = new Rectangle(Mouse.GetState().Position, new Point(1));
                if (mouseRect.Intersects(TextureDestination)){
                    IsMouseOver = true;
                }
                else IsMouseOver = false;
            }
        }
    }

    internal class GameBoard : GameObject
    {
        private Texture2D accentBottom;

        private LayoutAnimationProvider layoutAnimator;
        private AudioProvider audioProvider;

        private SelectionCursorObject selectionCursorObject;
        public bool MouseInsidePlayArea {  get; private set; }
        private HashSet<GameBoardCell> cells = new HashSet<GameBoardCell>();
        private Queue<JellyfishBlock> queuedExplosions = new Queue<JellyfishBlock>();
        private TimeSpan timeSinceLastExplosion = TimeSpan.Zero;

        private int cpuLastTurn;
        private GameBoardCell selectedCell;
        internal LevelGenerator Parent;        

        public int Rows { get; }
        public int Columns { get; }
        public Point CellSize => (Size.ToVector2() / new Vector2(Columns, Rows)).ToPoint();

        List<GameBoardCell> upcomingLine = new List<GameBoardCell>();
        int linesLeft = 55;
        int score = 0;
        int lastBonusAwardedTileIndex = -1;
        TimeSpan lineSpeed => Parent?.CurrentRuleset.SpawnRate ?? TimeSpan.FromSeconds(1);       
        TimeSpan timeSinceLastUpcomingSpawn = TimeSpan.Zero;
        TimeSpan levelCompleteAnimationTime = TimeSpan.Zero;

        /// <summary>
        /// True when the player is being rewarded for clearing the board.
        /// </summary>
        internal bool BonusAchieved { get; private set; }
        /// <summary>
        /// Gets whether the game is actively being played
        /// </summary>
        internal bool IsGameRunning { get; private set; }
        /// <summary>
        /// Gets whether the level has been successfully completed
        /// </summary>
        internal bool LevelCompleted { get; private set; }
        /// <summary>
        /// Gets whether or not the player has lost this board.
        /// </summary>
        internal bool IsGameOver {  get; private set; }
        /// <summary>
        /// Gets whether or not this board is in the middle of an animation
        /// </summary>
        internal bool IsAnimatingBoard { get;private set;  }
        /// <summary>
        /// The amount of lines left
        /// </summary>
        internal int LinesRemaining
        {
            get => linesLeft;
            set => linesLeft = value;
        }

        public GameBoard(int Rows, int Columns) : base("Backgrounds/IMAGE12", default, default)
        {
            layoutAnimator = ProviderManager.Root.Get<LayoutAnimationProvider>();
            
            this.Rows = Rows;
            this.Columns = Columns;
            
            ProviderManager.Root.Get<InputProvider>().InputEvent += GameBoard_InputEvent;            
        }

        ~GameBoard() // DESTRUCTOR
        {
            
        }

        public override void Dispose()
        {
            while(cells.Any())
                _destoryObject(cells.LastOrDefault());
            ProviderManager.Root.Get<GameObjectManager>().Remove(selectionCursorObject);
            base.Dispose();
        }

        private void GameBoard_InputEvent(InputProvider.InputEventArgs e)
        {
            if (selectedCell != null && e.MouseLeftClick)
            {
                if (!TryDestroyCell(selectedCell))
                {
                    if (selectedCell is AnchorBlock)
                        audioProvider.PlaySoundEffect(selectedCell.SoundName);
                    else
                        audioProvider.PlaySoundEffect("ABADHIT");
                }
            }
        }        

        public override void Initialize()
        {
            audioProvider = ProviderManager.Root.Get<AudioProvider>();
            
            accentBottom = ProviderManager.Root.Get<ContentProvider>().GetTexture("Backgrounds/IMAGE2");
            SetOriginalGameSizingRules();
            selectionCursorObject = new SelectionCursorObject(Position);
            selectionCursorObject.Visible = false;            
            ProviderManager.Root.Get<GameObjectManager>().Add(selectionCursorObject, 2);
            
            score = Scoreboard.Default.Score;
            UpdateScoreboard();

            audioProvider.PlaySoundEffect("ABLCLEAR");            
        }

        public void Resume()
        {
            IsGameRunning = true;
        }

        public void Pause()
        {
            IsGameRunning = false;
        }

        public T AddCell<T>(T Cell, GridCoordinate Coordinate, int IntroHeight = 800) where T : GameBoardCell
        {
            Cell.BoardCoordinate = Coordinate;
            cells.Add(Cell);
            Cell.Scale = (CellSize.ToVector2() / new Vector2(SBCollapseConstants.BlockDimension)).X;
            ProviderManager.Root.Get<GameObjectManager>().Add(Cell, 1);
            Cell.Parent = this;
            BeginIntroAnimation(Cell, IntroHeight);
            return Cell;
        }

        private void BeginIntroAnimation(GameBoardCell block, int IntroHeight)
        {
            var intendedPosition = CalculatePosition(block.BoardCoordinate);
            double time = IntroHeight / 800.0;
            if (time < .25)
                time = .25;
            AnimateObject(block, new Vector2(intendedPosition.X, intendedPosition.Y + IntroHeight),
                intendedPosition, TimeSpan.FromSeconds(time));
        }

        private void AnimateObject(GameBoardCell Cell, Vector2 Start, Vector2 End, TimeSpan Time)
        {
            KeyframedLayoutAnimationDefinition definition =
                new KeyframedLayoutAnimationDefinition(
                    new LayoutAnimationKeyframe("Start", Start, TimeSpan.Zero),
                    new LayoutAnimationKeyframe("End", End, Time)
                );
            layoutAnimator.Animate(
                Cell,
                PositionProperty,
                definition
            );
        }

        private Vector2 CalculatePosition(GridCoordinate Coordinate)
        {
            return SBCollapseInterop.Transform(SBCollapseConstants.PlayareaPosition) + ((Point)Coordinate * CellSize).ToVector2();
        }

        public void SetOriginalGameSizingRules()
        {            
            var fooSize = new Point(341, 409);
            Position = SBCollapseInterop.Transform(SBCollapseConstants.PlayareaPosition);
            Size = SBCollapseInterop.Transform(fooSize);
        }

        private void UpdateScoreboard()
        {
            var scoreBoard = Scoreboard.Default;
            scoreBoard.Level = ProviderManager.Root.Get<BoardProvider>().Level;
            scoreBoard.Score = score;
            scoreBoard.LinesLeft = LinesRemaining;
        }

        private void DoLevelCompleteAnimTick(GameTime time)
        {            
            //THE AMOUNT OF TIME IN BETWEEN BLOCK UPDATES
            TimeSpan animBufferTime = TimeSpan.FromSeconds(.05);

            //THE CURRENT TIME IN THIS ANIMATION
            levelCompleteAnimationTime += time.ElapsedGameTime;

            int freeTiles = (Rows * Columns);
            var totalTime = animBufferTime.TotalSeconds * freeTiles;
            var currentTile = (int)(freeTiles * (levelCompleteAnimationTime.TotalSeconds / totalTime));
            if (currentTile == freeTiles)
            { // WERE AT THE END OF THE BOARD
                IsAnimatingBoard = false;                
                return;
            }
            var coordinate = new GridCoordinate(currentTile / Columns, currentTile - (Columns * (currentTile / Columns)));
            var cell = GetCellAt(coordinate);
            if (cell == null && LevelCompleted) // CHECK IF THE LEVEL WAS COMPLETED & IF WERE ON AN EMPTY SPOT
            { // LEVEL COMPLETED ADD AQUA SHINY BLOCKS AND INCREMENT SCORE
                AddCell(new GameBlock(SpecialFlag.AQUA), coordinate, -200);
                if (lastBonusAwardedTileIndex != currentTile)
                {
                    score += 5;
                    if (currentTile % Columns == 0 && currentTile != 0) // new column 
                    {
                        score += 300;
                        audioProvider.PlaySoundEffect("ABNSROW");
                    }
                    else audioProvider.PlaySoundEffect("ABNSBL");
                    lastBonusAwardedTileIndex = currentTile;
                }
            }
            else if (cell != null && cell.Flag != SpecialFlag.AQUA)
            { // EITHER A GAME OVER OR WE'RE ON A NON-EMPTY BLOCK
                _destoryObject(cell);
                audioProvider.PlaySoundEffect("ADEEPB");
                lastBonusAwardedTileIndex = currentTile; // DO NOT AWARD UNCLEARED TILES!
            }
            else if (cell == null && IsGameOver) // GAME OVER AND NO BLOCK TO SPAWN?
                levelCompleteAnimationTime += TimeSpan.FromSeconds(animBufferTime.TotalSeconds / 3); // WE PUSH THE TIME FORWARD TO ALLOW THE ANIMATION TO COMPLETE QUICKER
        }

        private void CPUTakeTurn()
        {
            GameBoardCell cell = null;
            if (cells.Count == 0) return;
            int index = cpuLastTurn;
            if (index > cells.Count) index = 0;
            cell = cells.ElementAtOrDefault(index);
            if (cell == null)
                index++;
            cpuLastTurn = index + 1;
            if (cell == null) return;
            TryDestroyCell(cell);
        }

        public override void Update(GameTime gt)
        {
            SetOriginalGameSizingRules();
            UpdateMouse();
            selectionCursorObject.Visible = false;
            selectedCell = null;            
            if (IsAnimatingBoard) DoLevelCompleteAnimTick(gt);
            UpdateScoreboard();
            if (!IsGameRunning) return;
            DoUpcomingTick(gt);
            if (cells.Count == 0 && !BonusAchieved)
            {
                BonusAchieved = true;
                score += 1000;
                audioProvider.PlaySoundEffect("ABNSALLCLR");
            }
            else if (cells.Count > 0)
            {
                BonusAchieved = false;
                //CPUTakeTurn();
            }
            for (int i = 0; i < cells.Count; i++)
            {
                GameBoardCell cell = cells.ElementAt(i);
                if (cell == null) continue;
                if (!layoutAnimator.IsObjectAnimating(cell))
                {
                    var intendedPosition = CalculatePosition(cell.BoardCoordinate);
                    if (cell.Position != intendedPosition)
                        AnimateObject(cell, cell.Position, intendedPosition, TimeSpan.FromSeconds(.05f));
                }
                cell.Scale = (CellSize.ToVector2() / cell.Size.ToVector2()).X;
                Fall(cell);
                Elastic(cell);
                if (cell.IsMouseOver)
                {
                    selectedCell = cell;
                    selectionCursorObject.Position = cell.Position;                    
                    selectionCursorObject.Visible = true;
                    selectionCursorObject.Opacity = .85f;
                }              
            }
            if (timeSinceLastExplosion.TotalSeconds > .25 && queuedExplosions.Count > 0)
            {
                TryDestroyCell(queuedExplosions.Dequeue(), queuedExplosions.Count + 1);
                audioProvider.PlaySoundEffect("ACLAP");
                timeSinceLastExplosion = TimeSpan.Zero;
            }
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                selectionCursorObject.Visible = false; // ORIGINAL GAME DETAIL                                                       
            UpdateScoreboard();
            timeSinceLastExplosion += gt.ElapsedGameTime;
        }

        public GameBoardCell GetCellAt(GridCoordinate BoardCoordinate) => cells.FirstOrDefault(x => x.BoardCoordinate.Equals(BoardCoordinate));

        public bool IsValidLocation(GridCoordinate coord)
        {
            return coord.Row >= 0 && coord.Row < Rows && coord.Column >= 0 && coord.Column < Columns;
        }

        private void DoUpcomingTick(GameTime time)
        {
            if (linesLeft == 0)
                GameOver(true);
            timeSinceLastUpcomingSpawn += time.ElapsedGameTime;
            if (timeSinceLastUpcomingSpawn.TotalSeconds > lineSpeed.TotalSeconds)
            {                
                timeSinceLastUpcomingSpawn = TimeSpan.Zero;
                if(upcomingLine.Count == 10)
                {
                    if (linesLeft <= 1)
                        GameOver(true);
                    PushAll();
                    for(int i = 0; i < upcomingLine.Count; i++)
                    {
                        AddCell(upcomingLine[i], new GridCoordinate(Rows - 1, i), 50);
                    }
                    upcomingLine.Clear();
                    audioProvider.PlaySoundEffect("ANEWRW");
                    linesLeft--;
                    return;
                }
                var block = Parent.GetNext();
                block.Update(time);
                upcomingLine.Add(block);
                audioProvider.PlaySoundEffect("ABNSROW");
            }
        }

        private void PushAll(int RowAmount = 1)
        {
            for(int i = 0; i < cells.Count; i++)
            {
                var block = cells.ElementAtOrDefault(i);
                if (block == null) continue;
                var newPosition = block.BoardCoordinate - new GridCoordinate(1, 0);
                block.BoardCoordinate = newPosition;
                if (!IsValidLocation(newPosition))
                {
                    _destoryObject(block);
                    GameOver();
                    return;
                }                
            }
        }

        private void GameOver(bool Winner = false)
        {
            IsGameRunning = false;
            if (Winner)
                LevelCompleted = true;
            else
                IsGameOver = true;
            IsAnimatingBoard = true;
            levelCompleteAnimationTime = TimeSpan.Zero;
        }

        private void SpawnBigBubbleParticles(SpecialFlag Flag, Vector2 Position)
        {
            SBCollapseConstants.SBCollapseBubbleTexAtlasColumns column = default;
            switch (Flag)
            {
                case SpecialFlag.BLUE:
                    column = SBCollapseConstants.SBCollapseBubbleTexAtlasColumns.BLUE;
                    break;
                case SpecialFlag.GREEN:
                    column = SBCollapseConstants.SBCollapseBubbleTexAtlasColumns.GREEN;
                    break;
                case SpecialFlag.PURPLE:
                    column = SBCollapseConstants.SBCollapseBubbleTexAtlasColumns.PINK;
                    break;
                case SpecialFlag.YELLOW:
                    column = SBCollapseConstants.SBCollapseBubbleTexAtlasColumns.YELLOW;
                    break;
            }
            var particleProvider = ProviderManager.Root.Get<ParticleProvider>();
            var particle = new BubbleParticle(column, Position + new Vector2(200,400));
            particle.Scale = 1.5f;
            particle.Opacity = .75f;
            particle.Velocity = new Vector2(0, -400);
            particleProvider.Add(particle);
            particle = new BubbleParticle(column, Position + (SBCollapseConstants.BubbleTextures.TextureSize.ToVector2() / new Vector2(2 / 2.5f)))
            {
                Scale = 2.5f
            };
            particleProvider.Add(particle);
        }

        private void SpawnBubbleParticles(SpecialFlag Flag, Vector2 Position)
        {
            SBCollapseConstants.SBCollapseBubbleTexAtlasColumns column = default;
            switch (Flag)
            {
                case SpecialFlag.BLUE:
                    column = SBCollapseConstants.SBCollapseBubbleTexAtlasColumns.BLUE;
                    break;
                case SpecialFlag.GREEN:
                    column = SBCollapseConstants.SBCollapseBubbleTexAtlasColumns.GREEN;
                    break;
                case SpecialFlag.PURPLE:
                    column = SBCollapseConstants.SBCollapseBubbleTexAtlasColumns.PINK;
                    break;
                case SpecialFlag.YELLOW:
                    column = SBCollapseConstants.SBCollapseBubbleTexAtlasColumns.YELLOW;
                    break;
            }
            var particleProvider = ProviderManager.Root.Get<ParticleProvider>();
            var particle = new BubbleParticle(column, Position + SBCollapseConstants.BubbleTextures.TextureSize.ToVector2() / new Vector2(2))
            {
                Scale = 1f
            };
            particleProvider.Add(particle);
        }

        private void Fall(GameBoardCell Cell)
        {
            var destination = Cell.BoardCoordinate + new GridCoordinate(1, 0);
            if (!IsValidLocation(destination))
                return;
            var south = GetCellAt(destination);
            if (south == null || south.Flag == GameBoardCell.SpecialFlag.EMPTY)            
                Cell.BoardCoordinate = destination;            
        }

        private void Elastic(GameBoardCell Cell)
        {
            GridCoordinate destination = default;
            if (Cell.BoardCoordinate.Column < Columns / 2)
            { // GO RIGHT
                destination = Cell.BoardCoordinate + new GridCoordinate(0, 1);
            }
            else if (Cell.BoardCoordinate.Column > Columns / 2)
            { // GO LEFT
                destination = Cell.BoardCoordinate - new GridCoordinate(0, 1);
            }
            else return;
            if (!IsValidLocation(destination))
                return;
            var neighbor = GetCellAt(destination); 
            if (neighbor == null)
                Cell.BoardCoordinate = destination;
        }

        private void _destoryObject(GameBoardCell obj)
        {
            if (obj == null) return;
            SpawnBubbleParticles(obj.Flag, obj.Position);
            ProviderManager.Root.Get<GameObjectManager>().Remove(obj);
            ProviderManager.Root.Get<AnimatedObjectProvider>().StopAnimation(obj);
            obj.Dispose();
            cells.Remove(obj);
        }

        private void NestingFindAllMatches(GameBoardCell Cell, ref List<GameBoardCell> matches)
        {
            var north = GetCellAt(Cell.BoardCoordinate - new GridCoordinate(1, 0));
            var south = GetCellAt(Cell.BoardCoordinate + new GridCoordinate(1, 0));
            var west = GetCellAt(Cell.BoardCoordinate - new GridCoordinate(0, 1));
            var east = GetCellAt(Cell.BoardCoordinate + new GridCoordinate(0, 1));
            if (north != null && north.Flag == Cell.Flag)
            {
                if (!matches.Contains(north))
                {
                    matches.Add(north);
                    NestingFindAllMatches(north, ref matches);
                }
            }
            if (south != null && south.Flag == Cell.Flag)
            {
                if (!matches.Contains(south))
                {
                    matches.Add(south);
                    NestingFindAllMatches(south, ref matches);
                }
            }
            if (east != null && east.Flag == Cell.Flag)
            {
                if (!matches.Contains(east))
                {
                    matches.Add(east);
                    NestingFindAllMatches(east, ref matches);
                }
            }
            if (west != null && west.Flag == Cell.Flag)
            {
                if (!matches.Contains(west))
                {
                    matches.Add(west);
                    NestingFindAllMatches(west, ref matches);
                }
            }
        }        

        private IEnumerable<GameBoardCell> GetArea(GridCoordinate Center, int Area)
        {
            GridCoordinate topLeft = new GridCoordinate(Center.Row - Area / 2, Center.Column - Area / 2);
            for (int row = 0; row < Area; row++)
                for (int col = 0; col < Area; col++)
                    yield return GetCellAt(topLeft + new GridCoordinate(row, col));
        }

        private bool TryDestroyCell(GameBoardCell Cell, int ScoreMultiplier = 1)
        {
            List<GameBoardCell> matches = new List<GameBoardCell>();
            int amountRequired = 0;
            if (Cell is GameBubble)
                matches = cells.Where(x => x.Flag == Cell.Flag).ToList();
            else if (Cell is JellyfishBlock)
            {
                matches = GetArea(Cell.BoardCoordinate, 6).ToList();
                SpawnBigBubbleParticles(SpecialFlag.AQUA, Cell.Position);
            }
            else if (Cell is AnchorBlock)
            {
                return false;
            }
            else
            {
                amountRequired = 3;
                NestingFindAllMatches(Cell, ref matches);
            }
            if (matches.Count >= amountRequired)
            {
                foreach (var match in matches)
                {
                    if (match == null) continue;
                    if (match is JellyfishBlock jelly && match != Cell)
                    {
                        queuedExplosions.Enqueue(jelly);
                        jelly.IsExploding = true;
                        timeSinceLastExplosion = TimeSpan.Zero;
                    }
                    else _destoryObject(match);
                    if (match is AnchorBlock)
                        audioProvider.PlaySoundEffect(match.SoundName);
                }
                if (Cell is GameBubble)
                    score += 5;
                score += matches.Count + (matches.Count * 2 * (matches.Count - 3)) - 1;
                if (matches.Count < 10)
                    audioProvider.PlaySoundEffect(Cell.SoundName);
                else audioProvider.PlaySoundEffect("ABNSBIGCK");
                return true;
            }            
            return false;
        }

        private void UpdateMouse()
        {
            var state = Mouse.GetState();
            var mousePos = state.Position;
            var mouseRect = new Rectangle(mousePos.X, mousePos.Y,1,1);
            if (mouseRect.Intersects(TextureDestination))
            { // MOUSE IS INSIDE PLAYAREA
                MouseInsidePlayArea = true;
            }
            else MouseInsidePlayArea = false;
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);
            batch.Draw(accentBottom,
                new Rectangle(
                    SBCollapseInterop.Transform(
                    SBCollapseConstants.BottomBoardAccentPosition),
                    SBCollapseInterop.Transform(
                    new Point(accentBottom.Width, accentBottom.Height))), 
                Color.White);
            for(int i = 0; i < upcomingLine.Count; i++)
            {
                var line = upcomingLine[i];
                line.Size = SBCollapseInterop.Transform(new Point(25));
                line.Position = SBCollapseInterop.Transform(
                        SBCollapseConstants.BottomBoardAccentPosition.ToVector2() + 
                        new Vector2(9 + (i * (25 + 9)), 5)
                    );
                line.Draw(batch);
            }
        }
    }
}
