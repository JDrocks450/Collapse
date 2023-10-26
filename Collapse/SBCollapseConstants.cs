using Glacier.Common.Primitives;
using Glacier.Common.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collapse
{
    internal class SBCollapseConstants
    {
        public static TextureAtlas BlockTextures { get; internal set; }
        public static TextureAtlas BubbleTextures { get; internal set; }
        public static TextureAtlas ScoreboardText { get; internal set; }
        public static TextureAtlas LevelText { get; internal set; }  

        public static Point GameResolution => GameResources.Screen.Size;
        public static Point ScoreTextLocation => new Point(200, 279);
        public static Point LevelTextLocation => new Point(200, 346);
        public static Point LinesTextLocation => new Point(200, 413);
        public static Point BonusScoreDialogLocation => new Point(348, 70);
        public static Point GameOverDialogLocation => new Point(330, 101);
        public static Point StartDialogLocation => new Point(322, 53);
        public static Point StartButtonLocation => new Point(331, 252);
        public static Point LevelDialogLocation => new Point(342, 65);

        public static Point Level1Position => new Point(445, 65);
        public static Point Level2Position => new Point(442, 65);
        public static Point Level3Position => new Point(440, 65);
        public static Point Level4Position => new Point(461, 65);
        public static Point Level5Position => new Point(446, 105);
        public static Point Level6Position => new Point(436, 75);
        public static Vector2 LevelNumberLocation => new Vector2(400,110);              

        public const int BlockDimension = 34, Rows = 13, Columns = 10;
        public static readonly Point OriginalResolution = new Point(640, 480);
        public static readonly Vector2 PlayareaPosition = new Vector2(287, 17);
        public const string BonusMusic = "MUSIC18", GameMusic1 = "MUSIC19", GameMusic2 = "MUSIC20", intersital = "MUSIC21";
        public static Point BottomBoardAccentPosition = new Point(283, 426);

        public enum SBCollapseBubbleTexAtlasColumns
        {
            AQUA,
            BLUE,
            PINK,
            GREEN,
            YELLOW
        }

        /// <summary>
        /// Texture Atlas coordinates from the texture found in SBCollapse.dll, IMAGE7.png
        /// </summary>
        public enum SBCollapseTexAtlasIndices
        {
            BLOCK_BLU,
            BLOCK_PUR,
            BLOCK_GRE,
            BLOCK_YEL,
            BUBBLE_BLU = 4,
            BUBBLE_PUR = 9,
            BUBBLE_GRE = 14,
            BUBBLE_YEL = 19,
            ANCHOR = 24,
            STAR = 25,
            JELLYFISH_ANIM = 26,
            AQUA_BLOCK_ANIM = 31,
            BLUE_SPLASH = 37,
            PUR_SPLASH = 42,
            GRE_SPLASH = 47,
            YEL_SPLASH = 52,
            END = 57
        }
    }
}
