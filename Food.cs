/***********************************************************************
* Project :    PS7 - Snake
* File    :    Food.cs
* Name    :    Marko Ljubicic, Preston Balfour
* Date    :    11/22/2016
*
* Description:   This class represents a food object used in the game
*                Snake. It simply contains the ID and coordinates of
*                a Food object, which are stored in a Point object,
*                as well as accessor properties and an IsEaten method
*                that determines whether a Food object is still in play.
* 
************************************************************************/

using Newtonsoft.Json;

namespace Snake
{
    /// <summary>
    /// This class represents a food object used in the game Snake. It contains the
    /// ID and coordinates of a Food object on the map, as well as accessor properties
    /// and an IsEaten method that determines whether a Food object is still in play.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Food
    {
        [JsonProperty]
        private int ID;
        [JsonProperty]
        private Point loc;

        /// <summary>
        /// The Food class constructor. The constructor takes
        /// a coordinate point and ID as parameters.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ID"></param>
        public Food(Point p, int ID)
        {
            loc = p;
            this.ID = ID;
        }

        /// <summary>
        /// Returns the Point object containing the (x, y) coordinate pair 
        /// location of the Food object in the game world.
        /// </summary>
        public Point _loc
        {
            get { return loc; }
        }

        /// <summary>
        /// Returns the ID associated with the Food object.
        /// </summary>
        public int _ID
        {
            get { return ID; }
        }

        /// <summary>
        /// Determines if the Food object represents an eaten food.
        /// </summary>
        /// <returns>True if object represents an eaten food, else false.</returns>
        public bool IsEaten()
        {
            return (loc._x == -1 && loc._y == -1) ? true : false;
        }
    }
}
