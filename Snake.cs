/***********************************************************************
* Project :    PS7 - Snake
* File    :    Snake.cs
* Name    :    Marko Ljubicic, Preston Balfour
* Date    :    11/22/2016
*
* Description:   This class represents a snake object used in the game
*                Snake.
* 
************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Drawing;

namespace Snake
{
    /// <summary>
    /// This class models a snake used in the game Snake.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Snake
    {
        [JsonProperty]
        private int ID;      // The snake's ID, associated with each player.
        [JsonProperty]
        private string name; // The name of the player controlling the snake.
        [JsonProperty]
        private LinkedList<Point> vertices;
        private Color c;     // The color assigned to the snake.
        private Random rand = new Random(); // Random number generator for getting a random color.
        private int length;  // The length of the snake.

        /// <summary>
        /// Constructs a Snake object. The points of the head and tail, the player ID,
        /// and the player name are listed as parameters.
        /// </summary>
        /// <param name="head"></param>
        /// <param name="tail"></param>
        /// <param name="ID"></param>
        /// <param name="name"></param>
        public Snake(Point head, Point tail,int ID, string name)
        {
            vertices = new LinkedList<Point>();
            vertices.AddFirst(tail);
            vertices.AddLast(head);
            this.ID = ID;
            this.name = name;
            c = new Color();
            c = Color.FromArgb(rand.Next(50, 205), rand.Next(50, 205), rand.Next(50, 205));
        }

        /// <summary>
        /// Constructs a snake object with a list of vertices that represent its
        /// position.
        /// </summary>
        /// <param name="head"></param>
        /// <param name="tail"></param>
        /// <param name="lp"></param>
        /// <param name="ID"></param>
        /// <param name="name"></param>
        public Snake(Point head, Point tail, List<Point> lp, int ID, string name)
        {
            vertices = new LinkedList<Point>();
            vertices.AddFirst(tail);
            vertices.AddLast(head);

            foreach(Point el in lp)
                vertices.AddBefore(vertices.Last, el);

            this.ID = ID;
            this.name = name;
            c = new Color();
            c = Color.FromArgb(rand.Next(50, 205), rand.Next(50, 205), rand.Next(50, 205));
        }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        /// <summary>
        /// Returns the color of the snake.
        /// </summary>
        public Color snakeColor
        {
            get { return c; }
        }

        /// <summary>
        /// Returns the vertices that comprise the snake as a list.
        /// </summary>
        public LinkedList<Point> Vertices
        {
            get { return vertices; }
            set { this.vertices = value; }
        }

        /// <summary>
        /// Returns the number of vertices in the snake.
        /// </summary>
        /// <returns></returns>
        public int SizeOfSnake()
        {
            return vertices.Count;
        }

        /// <summary>
        /// Returns the player ID associated with the snake.
        /// </summary>
        public int _ID
        {
            get { return ID; }
        }

        /// <summary>
        /// Returns the player name associated with the snake.
        /// </summary>
        public string _name
        {
            get { return name; }
        }

        /// <summary>
        /// Determines whether the object represents a dead snake.
        /// </summary>
        /// <returns>Returns true if the object represents a dead snake, otherwise false.</returns>
        public bool IsDead()
        {
            return (vertices.Last()._x == -1 && vertices.Last()._y == -1) ? true : false;
        }

        /// <summary>
        /// Compares two Snake objects for equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if the IDs, heads, and tails are the same, else false.</returns>
        public override bool Equals(object obj)
        {
            Snake s = obj as Snake;

            if (s == null)
                return false;

            if (ID == s.ID && vertices.First.Equals(s.vertices.First) && vertices.Last.Equals(s.vertices.Last))
                return true;
            return false;
        }
    }
}
