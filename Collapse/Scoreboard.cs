using Glacier.Common.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collapse
{
    internal class Scoreboard : IGameComponent
    {
        public static Scoreboard Default { get; private set;  }

        public int Score {  get; set; }
        public int DrawScore = 0;
        public int LinesLeft {  get; set; }
        public int Level {  get; set;}
        public bool IsLoaded { get; set; }
        public bool Destroyed => false;

        TimeSpan lastScoreUpdateTime = TimeSpan.Zero;

        public Scoreboard()
        {
            Default = this;
        }

        public string PrintDebugStrings()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("SCORE: " + Score);
            builder.AppendLine("LINESLEFT: " + LinesLeft);
            builder.AppendLine("LEVEL: " + Level);
            return builder.ToString();
        }

        public void Initialize()
        {
            
        }

        public void Update(Microsoft.Xna.Framework.GameTime gt)
        {
            lastScoreUpdateTime += gt.ElapsedGameTime;
            if (lastScoreUpdateTime.TotalSeconds < .15) return;
            var difference = Score - DrawScore;
            difference = Math.Abs(difference);
            if (difference == 0)
                return;
            var amount = (difference / 20.0);
            amount = Math.Ceiling(amount);
            if (amount == 0) amount = 1;
            if (Score > DrawScore)
                DrawScore += (int)amount;
            else if (Score < DrawScore) 
                DrawScore -= (int)amount;            
        }

        public void Dispose()
        {
            
        }
    }
}
