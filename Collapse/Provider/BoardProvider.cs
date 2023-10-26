using Glacier.Common.Provider;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collapse.Provider
{    
    /// <summary>
    /// Serves data about the current GameBoard
    /// </summary>
    internal class BoardProvider : IProvider
    {
        private Dictionary<int, GenerationRuleset> rulesets = new Dictionary<int, GenerationRuleset>()
        {
            {1, new GenerationRuleset()
                {
                    GreenEnabled = true, BlueEnabled = true, PurpleEnabled = true,
                    BlueBubblesEnabled = true, GreenBubblesEnabled= true, PurpleBubblesEnabled= true,
                    SpawnRate = TimeSpan.FromSeconds(.5f),
                    StartingRows = 4
                }
            },
            {2, new GenerationRuleset()
                {
                    GreenEnabled = true, BlueEnabled = true, PurpleEnabled = true,
                    BlueBubblesEnabled = true, GreenBubblesEnabled= true, PurpleBubblesEnabled= true,
                    JellyfishEnabled = true,
                    SpawnRate = TimeSpan.FromSeconds(.45f),
                    StartingRows = 5
                }
            },
            {3, new GenerationRuleset()
                {
                    GreenEnabled = true, BlueEnabled = true, PurpleEnabled = true,
                    BlueBubblesEnabled = true, GreenBubblesEnabled= true, PurpleBubblesEnabled= true,
                    JellyfishEnabled = true, AnchorEnabled = true,
                    SpawnRate = TimeSpan.FromSeconds(.4f),
                    StartingRows = 6
                }
            },
            {4, new GenerationRuleset()
            {
                GreenEnabled = true, BlueEnabled = true, PurpleEnabled = true, YellowEnabled = true,
                    BlueBubblesEnabled = true, GreenBubblesEnabled= true, PurpleBubblesEnabled= true, YellowBubblesEnabled = true,
                    JellyfishEnabled = true, AnchorEnabled = true,
                    SpawnRate = TimeSpan.FromSeconds(.37f),
                    StartingRows = 5
            } 
            },
            {5, new GenerationRuleset()            
            {
                GreenEnabled = true, BlueEnabled = true, PurpleEnabled = true, YellowEnabled = true,
                    BlueBubblesEnabled = true, GreenBubblesEnabled= true, PurpleBubblesEnabled= true, YellowBubblesEnabled = true,
                    JellyfishEnabled = true, AnchorEnabled = true,
                    SpawnRate = TimeSpan.FromSeconds(.32f),
                    StartingRows = 6
            }
            },
            {6, new GenerationRuleset()
            {
                GreenEnabled = true, BlueEnabled = true, PurpleEnabled = true, YellowEnabled = true,
                    BlueBubblesEnabled = true, GreenBubblesEnabled= true, PurpleBubblesEnabled= true, YellowBubblesEnabled = true,
                    JellyfishEnabled = true, AnchorEnabled = true,
                    SpawnRate = TimeSpan.FromSeconds(.30f),
                    StartingRows = 7
            }
            },
            {7, new GenerationRuleset()
            {
                GreenEnabled = true, BlueEnabled = true, PurpleEnabled = true, YellowEnabled = true,
                    BlueBubblesEnabled = true, GreenBubblesEnabled= true, PurpleBubblesEnabled= true, YellowBubblesEnabled = true,
                    JellyfishEnabled = true, AnchorEnabled = true,
                    SpawnRate = TimeSpan.FromSeconds(.25f),
                    StartingRows = 7
            }
            },
        };
        private LevelGenerator _levelGenerator;
        public ProviderManager Parent { get; set; }
        public int Level { get; internal set; }

        internal BoardProvider()
        {
            _levelGenerator = new LevelGenerator();
        }

        /// <summary>
        /// Generates a new <see cref="GameBoard"/> using a predefined generation ruleset based on the given <c>Level</c>
        /// </summary>
        /// <param name="Level">The predefined ruleset to use to generate this board.</param>
        /// <returns></returns>
        public GameBoard Generate(int Level)
        {
            GenerationRuleset ruleset = GetRulesetByLevel(Level);
            this.Level = Level;
            if (_levelGenerator.GenerateNew(ruleset, out GameBoard Board))
                return Board;
            else return null;
        }

        public GenerationRuleset GetRulesetByLevel(int Level)
        {
            int getLinesRemaining() => 10 + (Level * 5);
            if (!rulesets.ContainsKey(Level))
            {
                if (Level > 7)
                {
                    var ruleset = rulesets[7];
                    var newSpawnRate = ruleset.SpawnRate.TotalSeconds - (Level - 7 * .5f);
                    if (newSpawnRate < .05f)
                        newSpawnRate = .05f;
                    ruleset.SpawnRate = TimeSpan.FromSeconds(newSpawnRate);
                    ruleset.LinesRemaining = getLinesRemaining();
                    return ruleset;
                }
            }          
            var set = rulesets[Level];
            set.LinesRemaining = getLinesRemaining();
            return set;
        }

        public void Refresh(GameTime time)
        {
            
        }
    }
}
