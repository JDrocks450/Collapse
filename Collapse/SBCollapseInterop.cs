using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collapse
{
    internal class SBCollapseInterop
    {
        /// <summary>
        /// Converts a point from the original game's resolution to the provided one.
        /// </summary>
        /// <param name="Resolution"></param>
        /// <returns></returns>
        public static Point Transform(Point Point, Point Resolution = default)
        {
            if (Resolution == default)
                Resolution = SBCollapseConstants.GameResolution;
            var origRes = SBCollapseConstants.OriginalResolution;
            var xRes = Point.X / (float)origRes.X; 
            var yRes = Point.Y / (float)origRes.Y;
            return (Resolution.ToVector2() * new Vector2(xRes, yRes)).ToPoint();
        }
        /// <summary>
        /// Converts a point from the original game's resolution to the provided one.
        /// </summary>
        /// <param name="Resolution"></param>
        /// <returns></returns>
        public static Vector2 Transform(Vector2 Point, Point Resolution = default)
        {
            if (Resolution == default)
                Resolution = SBCollapseConstants.GameResolution;
            var origRes = SBCollapseConstants.OriginalResolution;
            var xRes = Point.X / origRes.X;
            var yRes = Point.Y / origRes.Y;
            return (Resolution.ToVector2() * new Vector2(xRes, yRes));
        }

        internal static object Transform(object levelDialogLocation)
        {
            throw new NotImplementedException();
        }
    }
}
