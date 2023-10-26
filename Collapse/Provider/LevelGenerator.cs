using Collapse.Objects;
using Glacier.Common.GlacerMath;
using Glacier.Common.Util;
using static Collapse.Provider.GameBoardCell;
using Glacier.Common.Primitives;
using System;

namespace Collapse.Provider
{
    internal struct GenerationRuleset
    {
        public bool JellyfishEnabled, AnchorEnabled, BubblesEnabled;
        public uint StartingRows;
        public bool GreenEnabled, BlueEnabled, PurpleEnabled, YellowEnabled;
        public bool GreenBubblesEnabled, BlueBubblesEnabled, PurpleBubblesEnabled, YellowBubblesEnabled;
        public TimeSpan SpawnRate;
        public int LinesRemaining;
    }

    internal class LevelGenerator
    {
        private static NoiseGenerator noiseGenerator;
        internal GenerationRuleset CurrentRuleset { get; set; }

        internal LevelGenerator()
        {
            
        }

        internal GameBoardCell GetNext()
        {
            return GetNext(CurrentRuleset, 0, 0);
        }

        internal static GameBoardCell GetNext(GenerationRuleset ruleset, double col, double row)
        {
            var value = GameResources.Rand.Next(0, 80);             
            if (GameSettings.Default.UseNoise)
            {
                if (noiseGenerator == null)
                    noiseGenerator = new NoiseGenerator(1,.2,1, 1, GameResources.Rand.Next());
                value = (int)noiseGenerator.Get2D(col, row);
            }
            bool isBubble = false;
            if (value % 21 == 0)
                isBubble = true;
            if (value == 57)
                return new JellyfishBlock();
            if (value == 74)
                return new AnchorBlock();
            value = (int)Math.Floor(value / 20.0);
            value++;
            SpecialFlag flag = (SpecialFlag)(int)value;
            switch (flag)
            {
                case SpecialFlag.BLUE:
                    if (!ruleset.BlueEnabled)
                        flag = SpecialFlag.GREEN;
                    break;
                case SpecialFlag.GREEN:
                    if (!ruleset.GreenEnabled)
                        flag = SpecialFlag.BLUE;
                    break;
                case SpecialFlag.PURPLE:
                    if (!ruleset.PurpleEnabled)
                        flag = SpecialFlag.YELLOW;
                    break;
                case SpecialFlag.YELLOW:
                    if (!ruleset.YellowEnabled)
                        flag = SpecialFlag.PURPLE;
                    break;
            }
            if (isBubble)
            {
                return new GameBubble(flag);
            }
            return new GameBlock(flag);            
        }

        internal bool GenerateNew(GenerationRuleset ruleset, out GameBoard board)
        {
            CurrentRuleset = ruleset;
            board = new GameBoard(SBCollapseConstants.Rows, SBCollapseConstants.Columns)
            {
                LinesRemaining = CurrentRuleset.LinesRemaining
            };
            board.Parent = this;
            for(int row = 0; row < SBCollapseConstants.Rows; row++)
            {
                if (row > ruleset.StartingRows - 1) break;
                var foorow = SBCollapseConstants.Rows - 1 - row;                
                for(int col = 0; col < SBCollapseConstants.Columns; col++)
                {
                    var block = GetNext(ruleset, col, row);
                    board.AddCell(block, new GridCoordinate(foorow, col));
                }
            }
            return true;
        }

        internal bool GenerateTESTBoard(out GameBoard board)
        {
            board = new GameBoard(SBCollapseConstants.Rows, SBCollapseConstants.Columns);
            board.AddCell(new GameBlock(SpecialFlag.BLUE), new GridCoordinate(1, 1));
            board.AddCell(new GameBlock(SpecialFlag.GREEN), new GridCoordinate(1, 2));
            board.AddCell(new GameBlock(SpecialFlag.YELLOW), new GridCoordinate(1, 3));
            board.AddCell(new GameBlock(SpecialFlag.PURPLE), new GridCoordinate(1, 4));
            board.AddCell(new GameBubble(SpecialFlag.BLUE), new GridCoordinate(2, 1));
            board.AddCell(new GameBubble(SpecialFlag.GREEN), new GridCoordinate(2, 2));
            board.AddCell(new GameBubble(SpecialFlag.YELLOW), new GridCoordinate(2, 3));
            board.AddCell(new GameBubble(SpecialFlag.PURPLE), new GridCoordinate(2, 4));
            board.AddCell(new JellyfishBlock(), new GridCoordinate(3, 1));
            board.AddCell(new AnchorBlock(), new GridCoordinate(3, 2));
            board.AddCell(new StarBlock(), new GridCoordinate(3, 3));
            board.AddCell(new GameBlock(SpecialFlag.AQUA), new GridCoordinate(3, 4));
            return true;
        }
    }
}
