/***********************************************************************
* Project :    PS8 - Snake Server
* File    :    Server.cs
* Name    :    Marko Ljubicic, Preston Balfour
* Date    :    12/08/2016
*
* Description:   The server is a standalone program that can run on a 
*                separate machine from any client. The server program contains 
*                a world, and uses the appropriate world methods to keep it
*                up-to-date on every frame. It is up to the server to determine 
*                how often frames "tick."
* 
************************************************************************/

using Snake;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Xml;
using System.Timers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Server
{
    /// <summary>
    /// 
    /// </summary>
    class Server
    {
        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            string filename = "..\\..\\settings.xml";

            Server server = new Server(filename);
            server.StartServer();

            server.frameTimer = new Timer();
            server.frameTimer.Interval = ((server.frameRate) > 0 ? server.frameRate : 33);
            server.frameTimer.Elapsed += server.UpdateFrame;
            server.frameTimer.Start();

            server.foodGenerationTimer = new Timer();
            server.foodGenerationTimer.Interval = 1000 / ((server.world.Snakes.Count > 0) ? server.world.Snakes.Count : 1);
            server.foodGenerationTimer.Elapsed += server.SpawnNewFood;
            server.foodGenerationTimer.Start();

            // Sleep to prevent the program from closing,
            // since all the real work is done in separate threads
            // StartServer is non-blocking
            Console.Read();
        }

        // A list of clients that are connected.
        /// <summary>
        /// The clients
        /// </summary>
        private List<SocketState> clients;
        // The width and height of the game world.
        /// <summary>
        /// The board width
        /// </summary>
        private int boardWidth, boardHeight;
        // The rate at which new frames will be updated.
        /// <summary>
        /// The frame rate
        /// </summary>
        private int frameRate;
        // The number of food particles to spawn per player.
        /// <summary>
        /// The food density
        /// </summary>
        private int foodDensity;
        // The average multiple of grid cells that turn into food after the snake occupying those cells has died.
        /// <summary>
        /// The snake recycle rate
        /// </summary>
        private float snakeRecycleRate;
        // The world object that models the game.
        /// <summary>
        /// The world
        /// </summary>
        private World world;
        // The number of players/clients connected to the server.
        /// <summary>
        /// The player count
        /// </summary>
        private int playerCount;

        /// <summary>
        /// The food generation timer
        /// </summary>
        private Timer foodGenerationTimer;

        /// <summary>
        /// The frame timer
        /// </summary>
        private Timer frameTimer;

        /// <summary>
        /// The alt game play enable
        /// </summary>
        private bool altGamePlayEnable;

        private Random randomGenerator = new Random();

        /// <summary>
        /// Creates an instance of a server. Takes in a string file path to
        /// the server settings file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public Server(string filename)
        {
            clients = new List<SocketState>();
            ReadSettingsFile(filename);
            world = new World(boardHeight, boardWidth);
        }

        /// <summary>
        /// Start accepting Tcp sockets connections from clients
        /// </summary>
        public void StartServer()
        {
            Console.WriteLine("Server waiting for client");

            CallbackDelegate callMe = new CallbackDelegate(ConnectionRequested);

            // This begins an "event loop."
            // ConnectionRequested will be invoked when the first connection arrives.
            Network.ServerAwaitingClientLoop(callMe);
        }

        /// <summary>
        /// A callback that is invoked when a socket connection is accepted.
        /// </summary>
        /// <param name="newClient">The new client.</param>
        private void ConnectionRequested(SocketState newClient)
        {
            Console.WriteLine("Contact from client");

            // Callback delegate set to receive the player's name.
            newClient.Callback = ReceivePlayerName;

            try
            {
                // Start listening for a message
                // When a message arrives, handle it on a new thread with ReceiveCallback
                // the buffer   buffer offset   max bytes to receive     method to call when data arrives    "state" object representing the socket
                newClient.Socket.BeginReceive(newClient.Buffer, 0, newClient.Buffer.Length, SocketFlags.None, Network.ReceiveCallback, newClient);
            }
            catch(Exception e)
            {
                Debug.WriteLine("Could not establish communication with the client. " + e);
            }
        }

        /// <summary>
        /// This is a delegate callback that handles the server's side of the initial handshake.
        /// It receives the player's name and sends the client startup data, then requests direciton
        /// change information from the client.
        /// </summary>
        /// <param name="client">The client.</param>
        private void ReceivePlayerName(SocketState client)
        {
            // Get player's name.
            string playerName = client.Messages[0];

            // Create snake object corresponding to new player.
            CreateNewPlayerSnake(playerName);

            // Change callback to method that handles direction change requests.
            client.Callback = ReceiveDirection;

            // Store the assigned ID into the client state.
            client.ID = playerCount;

            // Send startup information to client.
            Network.Send(client.Socket, playerCount + "\n" + boardWidth + "\n" + boardHeight + "\n");

            playerCount++;

            // Can't have the server modifying the clients list if it's braodcasting a message.
            lock (clients)
            {
                clients.Add(client);
            }

            try
            {
                Network.GetData(client);
            }
            catch (SocketException e)
            {
                Debug.WriteLine("Could not receive from client due to disconnect. " + e);
                client.Socket.Shutdown(SocketShutdown.Both);
                client.Socket.Close();
            }
        }

        /// <summary>
        /// Handles data from the client - this is a delegate callback for processing client direction commands.
        /// It processes the command, then asks for more data.
        /// </summary>
        /// <param name="client">The client.</param>
        private void ReceiveDirection(SocketState client)
        {
            lock (world)
            {
                // Process direction change requests.
                if (world.Snakes.ContainsKey(client.ID))
                {
                    // Get the x and y coordinates of the client snake's head.
                    int snakesHeadX = world.Snakes[client.ID].Vertices.Last.Value._x;
                    int snakesHeadY = world.Snakes[client.ID].Vertices.Last.Value._y;

                    string s = Regex.Match(client.Messages[0], @"\(([^)]*)\)").Groups[1].Value;

                    int direction;
                    // Process the message as an integer representing the direction.
                    if (int.TryParse(s, out direction))
                    {
                        // Change the direction of the head on the world map. This allows us to
                        // receive multiple direction requests during the same frame cycle and
                        // save only the most recent one.
                        int currentDirection = world.WorldMap[snakesHeadX, snakesHeadY].direction;

                        if (!(currentDirection % 2 == 0 && direction % 2 == 0) && !(currentDirection % 2 == 1 && direction % 2 == 1))
                            world.WorldMap[snakesHeadX, snakesHeadY].direction = direction;
                    }
        
                    try
                    {
                        Network.GetData(client);
                    }
                    catch (SocketException e)
                    {
                        Debug.WriteLine("Could not receive from client due to disconnect. " + e);
                    }
                }
            }
        }

        /// <summary>
        /// This method is called after the initial handshake. Its purpose is to create a
        /// snake object for the new client. It finds a random location that is sufficiently
        /// removed from the border and orients the snake in a random direction.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        private void CreateNewPlayerSnake(string playerName)
        {
            lock (world)
            {
                

                int headX = randomGenerator.Next((int)0.12 * boardWidth, (int)(boardWidth - (boardWidth * 0.12)));
                int headY = randomGenerator.Next((int)0.12 * boardHeight, (int)(boardHeight - (boardHeight * 0.12)));
                int tailDirection = randomGenerator.Next(1, 4);


                bool done = false;

                while (!done)
                {
                    done = true;
                    //Sometimes throws exception
                    for (int i = 0; i < 16; i++)
                    {
                        if (tailDirection == 1)
                        {
                            if (world.WorldMap[headX, headY - i].ID != -1)
                                done = false;
                        }
                        else if (tailDirection == 2)
                        {
                            if (world.WorldMap[headX + i, headY].ID != -1)
                                done = false;
                        }
                        else if (tailDirection == 3)
                        {
                            if (world.WorldMap[headX, headY + i].ID != -1)
                                done = false;
                        }
                                    else if (tailDirection == 4)
                                        if (world.WorldMap[headX - i, headY].ID != -1)
                                            done = false;
                    }

                    if (!done)
                    {
                        headX = randomGenerator.Next((int)0.12 * boardWidth, (int)(boardWidth - (boardWidth * 0.12)));
                        headY = randomGenerator.Next((int)0.12 * boardHeight, (int)(boardHeight - (boardHeight * 0.12)));
                        tailDirection = randomGenerator.Next(1, 4);
                    }
                }

                Snake.Snake newPlayerSnake;

                if (tailDirection == 1)
                    newPlayerSnake = new Snake.Snake(new Point(headX, headY), new Point(headX, headY - 16), playerCount, playerName);
                else if (tailDirection == 2)
                    newPlayerSnake = new Snake.Snake(new Point(headX, headY), new Point(headX + 16, headY), playerCount, playerName);
                else if (tailDirection == 3)
                    newPlayerSnake = new Snake.Snake(new Point(headX, headY), new Point(headX, headY + 16), playerCount, playerName);
                else
                    newPlayerSnake = new Snake.Snake(new Point(headX, headY), new Point(headX - 16, headY), playerCount, playerName);

                world.Update(newPlayerSnake);
            }
        }

        /// <summary>
        /// Updates the frame.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void UpdateFrame(object sender, ElapsedEventArgs e)
        {
            lock (world)
            {
                List<SocketState> clientsToRemove = new List<SocketState>(); // A list to temporarily store clients that have disconnected.
                List<Snake.Snake> snakesToDie = new List<Snake.Snake>();      // A list to temporarily store snakes that must die for the updated frame.
                List<Food> eatenFood = new List<Food>();

                // Update the position of every snake. Pass the list to update it with newly dead snakes.
                UpdateSnakePositions(snakesToDie, eatenFood);

                // Kill the snakes in the list.
                List<Snake.Snake> deadSnakes = KillSnakes(snakesToDie);

                // If the dead snake list is not empty, populate food where they died.
                world.RecycleSnakes(snakesToDie, snakeRecycleRate, ref playerCount);

                // Send the relevant information out to all clients.
                foreach (SocketState client in clients)
                {
                    if (!client.Socket.Connected)
                    {
                        clientsToRemove.Add(client);
                        continue;
                    }

                    try
                    {
                        foreach (KeyValuePair<int, Snake.Snake> el in world.Snakes)
                        {
                            string message = JsonConvert.SerializeObject(el.Value);
                            Network.Send(client.Socket, message + "\n");
                        }

                        foreach (Snake.Snake el in deadSnakes)
                        {
                            string message = JsonConvert.SerializeObject(el);
                            Network.Send(client.Socket, message + "\n");
                        }

                        foreach (KeyValuePair<int, Food> el in world.Food)
                        {
                            string message = JsonConvert.SerializeObject(el.Value);
                            Network.Send(client.Socket, message + "\n");
                        }

                        foreach (Food el in eatenFood)
                        {
                            string message = JsonConvert.SerializeObject(el);
                            Network.Send(client.Socket, message + "\n");
                        }
                    }
                    catch(SocketException ex)
                    {
                        Debug.WriteLine("Unable to establish further communication with the client. Client disconnected: " + ex);
                        client.Socket.Shutdown(SocketShutdown.Both);
                        client.Socket.Close();
                        clientsToRemove.Add(client);
                    }
                }

                // Remove the clients that are no longer in use.
                RemoveDisconnectedClients(clientsToRemove);
            }

            //Check to see if the limit on number of food has been reached, if not add food
        }

        /// <summary>
        /// A helper method for updating the position of each snake in the world for the next frame.
        /// </summary>
        /// <param name="deadSnakes">The dead snakes.</param>
        /// <param name="eatenFood">The eaten food.</param>
        private void UpdateSnakePositions(List<Snake.Snake> deadSnakes, List<Food> eatenFood)
        {
            foreach (KeyValuePair<int, Snake.Snake> el in world.Snakes)
            {
                int snakeHeadX = el.Value.Vertices.Last.Value._x;
                int snakeHeadY = el.Value.Vertices.Last.Value._y;
                int direction = world.WorldMap[snakeHeadX, snakeHeadY].direction;

                if (direction == 1)
                {
                    Point p = new Point(snakeHeadX, snakeHeadY - 1);

                    DetectCollision(el.Value, p, deadSnakes, eatenFood);
                }
                else if (direction == 2)
                {
                    Point p = new Point(snakeHeadX + 1, snakeHeadY);

                    DetectCollision(el.Value, p, deadSnakes, eatenFood);
                }
                else if (direction == 3)
                {
                    Point p = new Point(snakeHeadX, snakeHeadY + 1);

                    DetectCollision(el.Value, p, deadSnakes, eatenFood);
                }
                else
                {
                    Point p = new Point(snakeHeadX - 1, snakeHeadY);

                    DetectCollision(el.Value, p, deadSnakes, eatenFood);
                }

                if (world.Snakes[el.Value._ID].Vertices.Count >= 3)
                {
                    if (world.Snakes[el.Value._ID].Vertices.Last.Value._x == world.Snakes[el.Value._ID].Vertices.Last.Previous.Value._x)
                    {
                        if (world.Snakes[el.Value._ID].Vertices.Last.Previous.Value._x == world.Snakes[el.Value._ID].Vertices.Last.Previous.Previous.Value._x)
                            world.Snakes[el.Value._ID].Vertices.Remove(world.Snakes[el.Value._ID].Vertices.Last.Previous);
                    }
                    else if (world.Snakes[el.Value._ID].Vertices.Last.Value._y == world.Snakes[el.Value._ID].Vertices.Last.Previous.Value._y)
                    {
                        if (world.Snakes[el.Value._ID].Vertices.Last.Previous.Value._y == world.Snakes[el.Value._ID].Vertices.Last.Previous.Previous.Value._y)
                            world.Snakes[el.Value._ID].Vertices.Remove(world.Snakes[el.Value._ID].Vertices.Last.Previous);
                    }  
                }
            }
        }

        /// <summary>
        /// Detects whether a snake has collided with the border or another snake.
        /// If yes, it adds the snake to the given list of dead snakes.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="p">The p.</param>
        /// <param name="deadSnakeList">The dead snake list.</param>
        /// <param name="eatenFood">The eaten food.</param>
        private void DetectCollision(Snake.Snake s,Point p, List<Snake.Snake> deadSnakeList, List<Food> eatenFood)
        {
            if (p._x == 0 || p._x == boardWidth || p._y == 0 || p._y == boardHeight)
            {
                deadSnakeList.Add(s);
            }
            else
            {
                if (world.WorldMap[p._x, p._y].ID != -1)
                {
                    if (world.Snakes.ContainsKey(world.WorldMap[p._x, p._y].ID))
                    {
                        deadSnakeList.Add(s);
                    }
                    else//Eat food command.
                    {
                        if (altGamePlayEnable)
                        {
                            int randomFoodValue = randomGenerator.Next(0, 3);

                            eatenFood.Add(new Food(new Point(-1, -1), world.WorldMap[p._x, p._y].ID));
                            world.Food.Remove(world.WorldMap[p._x, p._y].ID);

                            for (int i = 0; i <= randomFoodValue; i++)
                            {
                                world.WorldMap[p._x, p._y].ID = s._ID;
                                world.WorldMap[p._x, p._y].direction = world.WorldMap[world.Snakes[s._ID].Vertices.Last.Value._x, world.Snakes[s._ID].Vertices.Last.Value._y].direction;
                                world.Snakes[s._ID].Vertices.AddAfter(s.Vertices.Last, new Point(p._x, p._y));

                                if (world.WorldMap[p._x, p._y].direction == 1)
                                    p = new Point(p._x, p._y - 1);
                                else if (world.WorldMap[p._x, p._y].direction == 2)
                                    p = new Point(p._x + 1, p._y);
                                else if (world.WorldMap[p._x, p._y].direction == 3)
                                    p = new Point(p._x, p._y + 1);
                                else if (world.WorldMap[p._x, p._y].direction == 4)
                                    p = new Point(p._x - 1, p._y);
                                if (world.Snakes.ContainsKey(world.WorldMap[p._x, p._y].ID))
                                {
                                    deadSnakeList.Add(s);
                                    break;
                                }
                                else if (world.Food.ContainsKey(world.WorldMap[p._x, p._y].ID) || (p._x == 0 || p._x == boardWidth || p._y == 0 || p._y == boardHeight))
                                    DetectCollision(s, p, deadSnakeList, eatenFood);
                            }
                        }
                        else
                        {
                            world.Food.Remove(world.WorldMap[p._x, p._y].ID);
                            eatenFood.Add(new Food(new Point(-1, -1), world.WorldMap[p._x, p._y].ID));
                            world.WorldMap[p._x, p._y].ID = s._ID;
                            world.WorldMap[p._x, p._y].direction = world.WorldMap[world.Snakes[s._ID].Vertices.Last.Value._x, world.Snakes[s._ID].Vertices.Last.Value._y].direction;
                            world.Snakes[s._ID].Vertices.AddAfter(s.Vertices.Last, new Point(p._x, p._y));
                        }
                    }
                }
                else
                {//need to move the tail/remove old one
                    world.WorldMap[p._x, p._y].direction = world.WorldMap[world.Snakes[s._ID].Vertices.Last.Value._x , world.Snakes[s._ID].Vertices.Last.Value._y].direction;
                    world.Snakes[s._ID].Vertices.AddAfter(s.Vertices.Last, new Point(p._x, p._y));

                    int tailX = s.Vertices.First.Value._x;
                    int tailY = s.Vertices.First.Value._y;
                    int nextVX = s.Vertices.First.Next.Value._x;
                    int nextVY = s.Vertices.First.Next.Value._y;

                    if ((tailX - nextVX == 1 || nextVX - tailX == 1) && (tailY == nextVY))
                    {
                        world.WorldMap[world.Snakes[s._ID].Vertices.First.Value._x, world.Snakes[s._ID].Vertices.First.Value._y].ID = -1;
                        world.Snakes[s._ID].Vertices.RemoveFirst();
                    }
                    else if ((tailY - nextVY == 1 || nextVY - tailY == 1) && (tailX == nextVX))
                    {
                        world.WorldMap[world.Snakes[s._ID].Vertices.First.Value._x, world.Snakes[s._ID].Vertices.First.Value._y].ID = -1;
                        world.Snakes[s._ID].Vertices.RemoveFirst();
                    }
                    else
                    {
                        world.WorldMap[world.Snakes[s._ID].Vertices.First.Value._x, world.Snakes[s._ID].Vertices.First.Value._y].ID = -1;
                        world.Snakes[s._ID].Vertices.RemoveFirst();

                        if (tailX < nextVX)
                            world.Snakes[s._ID].Vertices.AddFirst(new Point(tailX + 1, tailY));
                        else if (tailX > nextVX)
                            world.Snakes[s._ID].Vertices.AddFirst(new Point(tailX - 1, tailY));
                        else if (tailY < nextVY)
                            world.Snakes[s._ID].Vertices.AddFirst(new Point(tailX, tailY + 1));
                        else if (tailY > nextVY)
                            world.Snakes[s._ID].Vertices.AddFirst(new Point(tailX, tailY - 1));
                    }

                    world.WorldMap[p._x, p._y].ID = s._ID;
                }
            }
        }

        /// <summary>
        /// Removes the clients in the list.
        /// </summary>
        /// <param name="clients">The clients.</param>
        private void RemoveDisconnectedClients(List<SocketState> clients)
        {
            foreach (SocketState client in clients)
                this.clients.Remove(client);
        }

        /// <summary>
        /// Takes the list of snakes and updates the world with dead versions.
        /// </summary>
        /// <param name="ls">The ls.</param>
        /// <returns></returns>
        private List<Snake.Snake> KillSnakes(List<Snake.Snake> ls)
        {
            List<Snake.Snake> deadSnakes = new List<Snake.Snake>();

            foreach (Snake.Snake s in ls)
            {
                Snake.Snake newDeadSnake = new Snake.Snake(new Point(-1, -1), new Point(-1, -1), s._ID, s._name);
                world.Update(newDeadSnake);
                deadSnakes.Add(newDeadSnake);
            }
            return deadSnakes;
        }

        /// <summary>
        /// Generates new food at an interval determined by the number of snakes.
        /// The maximum capacity is specified as food density in the settings file.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void SpawnNewFood(object sender, ElapsedEventArgs e)
        {
            lock (world)
            {
                if (world.Snakes.Count > 0 && (foodGenerationTimer.Interval * world.Snakes.Count) != 1000)
                {
                    foodGenerationTimer.Stop();
                    foodGenerationTimer.Interval = 1000 / world.Snakes.Count;
                    foodGenerationTimer.Start();
                }

                if (world.Food.Count < (foodDensity * world.Snakes.Count))
                    world.AddRandomFood(ref playerCount);
            }
        }

        /// <summary>
        /// Reads in the settings file (.xml) for configuring server parameters.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <exception cref="System.ArgumentNullException">'filename' cannot reference null</exception>
        private void ReadSettingsFile(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException("'filename' cannot reference null");

            try
            {
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.ReadToFollowing("SnakeSettings"))
                    {
                        reader.ReadToFollowing("BoardWidth");
                        boardWidth = reader.ReadElementContentAsInt();
                        reader.ReadToFollowing("BoardHeight");
                        boardHeight = reader.ReadElementContentAsInt();
                        reader.ReadToFollowing("MSPerFrame");
                        frameRate = reader.ReadElementContentAsInt();
                        reader.ReadToFollowing("FoodDensity");
                        foodDensity = reader.ReadElementContentAsInt(); ;
                        reader.ReadToFollowing("SnakeRecycleRate");
                        snakeRecycleRate = reader.ReadElementContentAsFloat();
                        reader.ReadToFollowing("EnableAltGamePlay");
                        altGamePlayEnable = reader.ReadElementContentAsBoolean();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to read the settings file. Check the path or contents: " + e);
                throw;
            }
        }
    }
}