/***********************************************************************
* Project :    PS7 - Snake
* File    :    Point.cs
* Name    :    Marko Ljubicic, Preston Balfour
* Date    :    11/22/2016
*
* Description:   This class represents a point object used in the game
*                Snake. It is used to represent a coordinate on the map.
* 
************************************************************************/

using Newtonsoft.Json;

namespace Snake
{
    /// <summary>
    /// The Point class represents a simple coordinate pair (x, y) of integers.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Point
    {
        [JsonProperty]
        private int x; // The x coordinate.
        [JsonProperty]
        private int y; // The y coordinate.

        /// <summary>
        /// Point class constructor. The initialized point becomes (_x, _y).
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        public Point(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        /// <summary>
        /// Returns the x-coordinate of the point.
        /// </summary>
        public int _x
        {
            get { return x; }
        }

        /// <summary>
        /// Returns the y-coordinate of the point.
        /// </summary>
        public int _y
        {
            get { return y; }
        }

        /// <summary>
        /// Compares two Point objects for equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if the points contain the same x- and y- coordinates, else false.</returns>
        public override bool Equals(object obj)
        {
            Point p = obj as Point;

            if (p == null)
                return false;

            if (x == p.x && y == p.y)
                return true;

            return false;
        }
    }
}
