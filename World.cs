/***********************************************************************
* Project :    PS7 - Snake Client, PS8 - Snake Server
* File    :    World.cs
* Name    :    Marko Ljubicic, Preston Balfour
* Date    :    12/08/2016
*
* Description:   This class represents the world model for the snake game.
*                It stores snake and food objects and manages the updating
*                of the world as the game proceeds.
* 
************************************************************************/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Timers;

namespace Snake
{
    /// <summary>
    /// Represents a cell object on the world map. Each point on the map corresponds to a cell.
    /// </summary>
    public struct Cell
    {
        /// <summary>
        /// The ID stored in a cell.
        /// </summary>
        public int ID;
        /// <summary>
        /// The direction of the snake part in the cell.
        /// </summary>
        public int direction;
        /// <summary>
        /// The color of the object in the cell.
        /// </summary>
        public Color color;
        /// <summary>
        /// The type of object the cell contains.
        /// </summary>
        public int cellType;
    }

    /// <summary>
    /// This class represents the world model for the snake game.
    /// </summary>
    public class World
    {
        //Scoreboard Dictionary
        private Dictionary<int, int> scoreboard;
        // The snakes present in the game world.
        private Dictionary<int, Snake> snakes;
        // The food present in the game world.
        private Dictionary<int, Food> food;
        // A 2D array of cells that represents the game map.
        private Cell[,] world;
        // The height and width of the game world.
        private int height, width;
        // From the client's perspective, the player ID is useful inside the World class.
        private int playerID;

        /// <summary>
        /// The constructor for instantiating new World objects.
        /// It specifies two int parameters that are meant for
        /// establishing the height and width of a world map.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="width"></param>
        public World(int height, int width)
        {
            world = new Cell[height, width];
            snakes = new Dictionary<int, Snake>();
            food = new Dictionary<int, Food>();
            scoreboard = new Dictionary<int, int>();
            this.height = height;
            this.width = width;

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    world[i, j].ID = -1;
        }

        /// <summary>
        /// Gets or sets the player ID associated with this world. Useful mainly
        /// from the client's perspective. This is shared code, after all.
        /// </summary>
        public int PlayerID
        {
            get { return playerID; }
            set { playerID = value; }
        }

        /// <summary>
        /// The scores of all the current players in the world.
        /// </summary>
        public Dictionary<int, int> Scoreboard
        {
            get { return scoreboard; }
        }

        /// <summary>
        /// Returns the game world if called within the same namespace.
        /// </summary>
        /// <returns>The model of the game world.</returns>
        public Cell[,] WorldMap
        {
            get { return world; }
        }

        /// <summary>
        /// Returns the live snakes in the world.
        /// </summary>
        public Dictionary<int, Snake> Snakes
        {
            get { return snakes; }
        }

        /// <summary>
        /// The food objects stored in the game world, keyed with their IDs.
        /// </summary>
        public Dictionary<int, Food> Food
        {
            get { return food; }
        }

        /// <summary>
        /// Gets the height of the game world.
        /// </summary>
        public int Height { get { return height; } }

        /// <summary>
        /// Gets the width of the game world.
        /// </summary>
        public int Width { get { return width; } }

        /// <summary>
        /// Gets the pixels per cell in the world.
        /// </summary>
        public int PixelsPerCell { get; set; }

        /// <summary>
        /// Adds a random food pellet.
        /// Used by the server only.
        /// </summary>
        public void AddRandomFood(ref int ID)
        {
            Random rand = new Random();

            int x, y;

            do
            {
                x = rand.Next(1, width - 1);
                y = rand.Next(1, height - 1);
            } while (WorldMap[x, y].ID != -1);

            food.Add(ID, new Food(new Point(x, y), ID));
            WorldMap[x, y].ID = ID;
            ID++;
        }

        /// <summary>
        /// Turns the average multiple of grid cells specified by the recycleRate parameter
        /// into food for each dead snake in the given list.
        /// </summary>
        /// <param name="deadSnakes"></param>
        /// <param name="recycleRate"></param>
        /// <param name="ID"></param>
        public void RecycleSnakes(List<Snake> deadSnakes, float recycleRate, ref int ID)
        {
            Random rand = new Random();
            HashSet<Point> snakeCells = new HashSet<Point>();

            if (deadSnakes.Count != 0)
            {
                foreach (Snake s in deadSnakes)
                { 
                    for (var node = s.Vertices.First; node != null; node = node.Next)
                    {
                        if (node.Next != null)
                        {
                            if (node.Value._x == node.Next.Value._x)
                            {
                                if (node.Value._y < node.Next.Value._y)
                                {
                                    for (int i = node.Value._y; i < node.Next.Value._y; i++)
                                    {
                                        snakeCells.Add(new Point(node.Value._x, i));
                                    }
                                }
                                else
                                {
                                    for (int i = node.Value._y; i > node.Next.Value._y; i--)
                                    {
                                        snakeCells.Add(new Point(node.Value._x, i));
                                    }
                                }
                            }
                            else
                            {
                                if (node.Value._x < node.Next.Value._x)
                                {
                                    for (int i = node.Value._x; i < node.Next.Value._x; i++)
                                    {
                                        snakeCells.Add(new Point(i, node.Value._y));
                                    }
                                }
                                else
                                {
                                    for (int i = node.Value._x; i > node.Next.Value._x; i--)
                                    {
                                        snakeCells.Add(new Point(i, node.Value._y));
                                    }
                                }
                            }
                        }
                    }

                    Point[] vertices = snakeCells.ToArray();
                    HashSet<Point> pointsToRecyle = new HashSet<Point>();

                    int numberOfFoodToSpawn = (int)Math.Round((vertices.Length * recycleRate));
                    int j = 0;
                    while (j < numberOfFoodToSpawn)
                    {
                        Point p = vertices[rand.Next(0, vertices.Length)];

                        if (!pointsToRecyle.Contains(p))
                        {
                            pointsToRecyle.Add(p);
                            Update(new Food(p, ID));
                            ID++;
                            j++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the game world with the given Food object. The food is
        /// added or removed based on conditions provided by the server.
        /// </summary>
        /// <param name="f"></param>
        public void Update(Food f)
        {
            if (!f.IsEaten())
            {
                if (!snakes.ContainsKey(world[f._loc._x, f._loc._y].ID))
                {
                    if (!food.ContainsKey(f._ID))
                        food.Add(f._ID, f);
                    world[food[f._ID]._loc._x, food[f._ID]._loc._y].ID = f._ID;
                    world[food[f._ID]._loc._x, food[f._ID]._loc._y].direction = 0;
                    world[food[f._ID]._loc._x, food[f._ID]._loc._y].color = Color.Black;
                }
            }
            else
                food.Remove(f._ID); 
        }

        /// <summary>
        /// Updates the game world with the given Snake object. The snake moves,
        /// grows, or dies in the world based on conditions provided by the server. 
        /// </summary>
        /// <param name="s"></param>
        public void Update(Snake s)
        {
            if (s.IsDead() && snakes.ContainsKey(s._ID))
            {
                SnakeCellUpdate(snakes[s._ID], -1);
                snakes.Remove(s._ID);
                scoreboard.Remove(s._ID);
            }
            else if (!s.IsDead() && !snakes.ContainsKey(s._ID))
            {
                snakes.Add(s._ID, s);
                int snakeSize = SnakeCellUpdate(s, s._ID);
                scoreboard.Add(s._ID, snakeSize);
                snakes[s._ID].Length = snakeSize;
            }
            else if (snakes.ContainsKey(s._ID))
            {
                if (!s.Equals(snakes[s._ID]))
                {

                    Point oldSnakeHead = snakes[s._ID].Vertices.Last.Value;
                    Point newSnakeHead = s.Vertices.Last.Value;
                    Point oldSnakeTail = snakes[s._ID].Vertices.First.Value;
                    Point newSnakeTail = s.Vertices.First.Value;
                    
                    // If tails match its a grow command, else it's a move command
                    if (s.Vertices.First.Value.Equals(snakes[s._ID].Vertices.First.Value))
                    {
                        world[newSnakeHead._x, newSnakeHead._y].ID = s._ID;
                        world[newSnakeHead._x, newSnakeHead._y].color = snakes[s._ID].snakeColor;
                        scoreboard[s._ID]++;
                        snakes[s._ID].Length++;
                    }
                    else // process move command
                    {
                        world[oldSnakeTail._x, oldSnakeTail._y].ID = -1;
                        world[oldSnakeTail._x, oldSnakeTail._y].color = Color.Empty;
                        world[newSnakeHead._x, newSnakeHead._y].ID = s._ID;
                        world[newSnakeHead._x, newSnakeHead._y].color = snakes[s._ID].snakeColor;
                    }

                    if (newSnakeHead._x > oldSnakeHead._x)
                        world[newSnakeHead._x, newSnakeHead._y].direction = 2;
                    else if (newSnakeHead._x < oldSnakeHead._x)
                        world[newSnakeHead._x, newSnakeHead._y].direction = 4;
                    else if (newSnakeHead._y > oldSnakeHead._y)
                        world[newSnakeHead._x, newSnakeHead._y].direction = 3;
                    else if (newSnakeHead._y < oldSnakeHead._y)
                        world[newSnakeHead._x, newSnakeHead._y].direction = 1;
                    else
                        world[newSnakeHead._x, newSnakeHead._y].direction = -1;
                    
                    snakes[s._ID].Vertices = s.Vertices;
                }

                snakes.TryGetValue(s._ID, out s);
            }
        }

        /// <summary>
        /// Updates snake cells on the map. It traces the vertices occupied by the
        /// given snake and modifies them to suit the new snake position.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="ID"></param>
        /// <returns>The length of the snake.</returns>
        private int SnakeCellUpdate(Snake s, int ID)
        {
            int size = 0;

            for (int i = 0; i < s.Vertices.Count - 1; i++)
            {
                int currentXVal = s.Vertices.ElementAt(i)._x;
                int nextXVal = s.Vertices.ElementAt(i + 1)._x;
                int currentYVal = s.Vertices.ElementAt(i)._y;
                int nextYVal = s.Vertices.ElementAt(i + 1)._y;

                

                if (currentXVal > nextXVal)
                    size += WalkVertices(nextXVal, currentXVal, currentYVal, nextYVal, ID == -1 ? 0 : 4, ID, ID == -1 ? Color.Empty : snakes[s._ID].snakeColor);
                else if (currentXVal < nextXVal)
                    size += WalkVertices(currentXVal, nextXVal, currentYVal, nextYVal, ID == -1 ? 0 : 2, ID, ID == -1 ? Color.Empty : snakes[s._ID].snakeColor);
                else if (currentYVal > nextYVal)
                    size += WalkVertices(currentXVal, nextXVal, nextYVal, currentYVal, ID == -1 ? 0 : 1, ID, ID == -1 ? Color.Empty : snakes[s._ID].snakeColor);
                else if (currentYVal < nextYVal)
                    size += WalkVertices(currentXVal, nextXVal, currentYVal, nextYVal, ID == -1 ? 0 : 3, ID, ID == -1 ? Color.Empty : snakes[s._ID].snakeColor);
            }

            return size;
        }

        /// <summary>
        /// A helper method for SnakeCellUpdate that populates the cells of the snake with correct values
        /// by navigating each vertex.
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="stopX"></param>
        /// <param name="startY"></param>
        /// <param name="stopY"></param>
        /// <param name="direction"></param>
        /// <param name="ID"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private int WalkVertices(int startX, int stopX, int startY, int stopY, int direction, int ID, Color c)
        {
            int size = 0;
            for (int i = startX; i <= stopX; i++)
                for (int j = startY; j <= stopY; j++)
                {
                    world[i, j].direction = direction;
                    world[i, j].ID = ID;
                    world[i, j].color = c;

                    size += 1;
                }

            return size;
        }
    }
}
