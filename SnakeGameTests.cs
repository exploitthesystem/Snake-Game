using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Snake
{
    /// <summary>
    /// This class imitates a server by providing simplified networking methods. It is used to test
    /// the networking code in NetworkController.cs.
    /// </summary>
    public static class MockServer
    {
        private static TcpListener listener = new TcpListener(IPAddress.Any, Network.DEFAULT_PORT);
        private static Socket client;

        /// <summary>
        /// Start accepting Tcp sockets connections from the client.
        /// </summary>
        public static void StartServer()
        {
            listener.Start();

            // This begins an "event loop".
            // ConnectionRequested will be invoked when the first connection arrives.
            listener.BeginAcceptSocket(ConnectionRequested, null);
        }

        public static void StopServer()
        {
            listener.Stop();
        }

        public static void ConnectionRequested(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;

            // Get the socket
            client = listener.EndAcceptSocket(ar);
            
            SocketState newClient = new SocketState(client);

            // Start listening for a message
            // When a message arrives, handle it on a new thread with ReceiveCallback
            //                              the buffer  buffer offset   max bytes to receive   method to call when data arrives   "state" object representing the socket
            newClient.Socket.BeginReceive(newClient.Buffer, 0, newClient.Buffer.Length, SocketFlags.None, ReceiveCallback, newClient);
        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            // Get the socket state out of the AsyncState
            // This is the object that we passed to BeginReceive that represents the socket
            SocketState sender = (SocketState)ar.AsyncState;

            int bytesRead = sender.Socket.EndReceive(ar);
            
            // If the socket is still open
            if (bytesRead > 0)
            {
                string theMessage = Encoding.UTF8.GetString(sender.Buffer, 0, bytesRead);
                // Append the received data to the growable buffer.
                // It may be an incomplete message, so we need to start building it up piece by piece
                sender.Data.Append(theMessage);

                if (theMessage == "disconnect") sender.Socket.Disconnect(false);
            }

            sender.Callback(sender);
        }
    }

    [TestClass]
    public class SnakeGameTests
    {
        /// <summary>
        /// Updates snake world with a food and snake object.
        /// </summary>
        [TestMethod]
        public void UpdateWorldTest()
        {
            World world = new World(150, 150);
            Food f = new Food(new Point(1, 2), 1);
            Snake s = new Snake(new Point(2, 2), new Point(2, 1), 2, "Dummy");

            world.Update(f);
            world.Update(s);

            Assert.AreEqual(1, world.WorldMap[1, 2].ID);
            Assert.AreEqual(2, world.WorldMap[2, 1].ID);
            Assert.AreEqual(2, world.WorldMap[2, 2].ID);
            Assert.AreEqual(150, world.Width);
            Assert.AreEqual(150, world.Height);
        }

        /// <summary>
        /// Updates snake world with a snake that has moved.
        /// </summary>
        [TestMethod]
        public void UpdateWorldAsSnakeMovesTest1()
        {
            World world = new World(150, 150);
            Snake s = new Snake(new Point(2, 2), new Point(2, 1), 2, "Dummy");

            world.Update(s);
            
            s = new Snake(new Point(3, 2), new Point(2, 2), 2, "Dummy");

            world.Update(s);

            Assert.AreEqual(2, world.WorldMap[3, 2].ID);
            Assert.AreEqual(2, world.WorldMap[2, 2].ID);
            Assert.AreEqual(-1, world.WorldMap[2, 1].ID);
        }

        /// <summary>
        /// Updates snake world with a snake that has moved.
        /// </summary>
        [TestMethod]
        public void UpdateWorldAsSnakeMovesTest2()
        {
            World world = new World(150, 150);
            Snake s = new Snake(new Point(2, 2), new Point(2, 1), 2, "Dummy");

            world.Update(s);

            s = new Snake(new Point(1, 2), new Point(2, 2), 2, "Dummy");

            world.Update(s);

            Assert.AreEqual(2, world.WorldMap[1, 2].ID);
            Assert.AreEqual(2, world.WorldMap[2, 2].ID);
            Assert.AreEqual(-1, world.WorldMap[2, 1].ID);
        }

        /// <summary>
        /// Updates snake world with a snake that has moved.
        /// </summary>
        [TestMethod]
        public void UpdateWorldAsSnakeMovesTest3()
        {
            World world = new World(150, 150);
            Snake s = new Snake(new Point(2, 2), new Point(2, 1), 2, "Dummy");

            world.Update(s);

            s = new Snake(new Point(2, 3), new Point(2, 2), 2, "Dummy");

            world.Update(s);

            Assert.AreEqual(2, world.WorldMap[2, 3].ID);
            Assert.AreEqual(2, world.WorldMap[2, 2].ID);
            Assert.AreEqual(-1, world.WorldMap[2, 1].ID);
        }

        /// <summary>
        /// Updates snake world with a snake that has moved.
        /// </summary>
        [TestMethod]
        public void UpdateWorldAsSnakeMovesTest4()
        {
            World world = new World(150, 150);
            Snake s = new Snake(new Point(2, 4), new Point(2, 5), 2, "Dummy");

            world.Update(s);

            s = new Snake(new Point(2, 3), new Point(2, 4), 2, "Dummy");

            world.Update(s);

            Assert.AreEqual(2, world.WorldMap[2, 3].ID);
            Assert.AreEqual(2, world.WorldMap[2, 4].ID);
            Assert.AreEqual(-1, world.WorldMap[2, 5].ID);
        }

        /// <summary>
        /// Tests the corner case where a dead snake is received witout 
        /// ever having been in the world. It should be ignored.
        /// </summary>
        [TestMethod]
        public void UpdateWorldWithDeadSnakeTest()
        {
            World world = new World(150, 150);
            Snake s = new Snake(new Point(-1, -1), new Point(-1, -1), 0, "Dead");

            world.Update(s);

            Assert.AreEqual(0, world.Snakes.Count);
        }

        /// <summary>
        /// Tests world updating after a snake has died.
        /// </summary>
        [TestMethod]
        public void UpdateWorldAfterSnakeDiesTest()
        {
            World world = new World(150, 150);
            Snake s = new Snake(new Point(2, 2), new Point(2, 1), 0, "Dummy");
            world.Update(s);

            s = new Snake(new Point(-1, -1), new Point(-1, -1), 0, "Dummy");
            world.Update(s);

            Assert.AreEqual(0, world.Snakes.Count);
        }

        /// <summary>
        /// The cells which a snake in the world occupies should reflect the
        /// same color as the snake itself.
        /// </summary>
        [TestMethod]
        public void UpdateWorldCellsWithSnakeColorTest()
        {
            World world = new World(150, 150);
            List<Point> points = new List<Point>();

            Point tail = new Point(25, 25);
            points.Add(new Point(28, 25));
            points.Add(new Point(28, 22));
            Point head = new Point(31, 22);

            Snake s = new Snake(head, tail, points, 1, "Liquid");
            world.Update(s);

            Assert.AreEqual(world.WorldMap[25, 25].color, s.snakeColor);
            Assert.AreEqual(world.WorldMap[26, 25].color, s.snakeColor);
            Assert.AreEqual(world.WorldMap[27, 25].color, s.snakeColor);
            Assert.AreEqual(world.WorldMap[28, 25].color, s.snakeColor);
            Assert.AreEqual(world.WorldMap[28, 24].color, s.snakeColor);
            Assert.AreEqual(world.WorldMap[28, 23].color, s.snakeColor);
            Assert.AreEqual(world.WorldMap[28, 22].color, s.snakeColor);
            Assert.AreEqual(world.WorldMap[29, 22].color, s.snakeColor);
            Assert.AreEqual(world.WorldMap[30, 22].color, s.snakeColor);
            Assert.AreEqual(world.WorldMap[31, 22].color, s.snakeColor);
        }

        /// <summary>
        /// The cells which a snake in the world occupies should reflect the
        /// same color as the snake itself.
        /// </summary>
        [TestMethod]
        public void UpdateWorldCellColorsAfterSnakeHasDiedTest()
        {
            World world = new World(150, 150);
            List<Point> points = new List<Point>();

            Point tail = new Point(25, 25);
            points.Add(new Point(28, 25));
            points.Add(new Point(28, 22));
            Point head = new Point(31, 22);

            Snake s = new Snake(head, tail, points, 1, "Liquid");
            world.Update(s);

            s = new Snake(new Point(-1, -1), new Point(-1, -1), 1, "Liquid");
            world.Update(s);

            Assert.AreEqual(world.WorldMap[25, 25].color, Color.Empty);
            Assert.AreEqual(world.WorldMap[26, 25].color, Color.Empty);
            Assert.AreEqual(world.WorldMap[27, 25].color, Color.Empty);
            Assert.AreEqual(world.WorldMap[28, 25].color, Color.Empty);
            Assert.AreEqual(world.WorldMap[28, 24].color, Color.Empty);
            Assert.AreEqual(world.WorldMap[28, 23].color, Color.Empty);
            Assert.AreEqual(world.WorldMap[28, 22].color, Color.Empty);
            Assert.AreEqual(world.WorldMap[29, 22].color, Color.Empty);
            Assert.AreEqual(world.WorldMap[30, 22].color, Color.Empty);
            Assert.AreEqual(world.WorldMap[31, 22].color, Color.Empty);
        }

        /// <summary>
        /// Verifies that the score of a newly created snake is correct.
        /// </summary>
        [TestMethod]
        public void ScoreboardNewSnakeTest()
        {
            World world = new World(150, 150);
            Snake s = new Snake(new Point(2, 2), new Point(2, 1), 0, "Dummy");
            world.Update(s);

            Assert.AreEqual(2, world.Scoreboard[s._ID]);
            Assert.AreEqual(2, s.SizeOfSnake());
            Assert.AreEqual("Dummy", s._name);
        }

        /// <summary>
        /// The scoreboard should update as the snake grows.
        /// </summary>
        [TestMethod]
        public void ScoreboardAsSnakeGrowsTest()
        {
            World world = new World(150, 150);
            Snake s = new Snake(new Point(2, 2), new Point(2, 1), 0, "Dummy");
            world.Update(s);

            Assert.AreEqual(world.Snakes[s._ID].Length, world.Scoreboard[s._ID]);

            s = new Snake(new Point(3, 2), new Point(2, 1), 0, "Dummy");
            world.Update(s);

            Assert.AreEqual(world.Snakes[s._ID].Length, world.Scoreboard[s._ID]);

            s = new Snake(new Point(4, 2), new Point(2, 1), 0, "Dummy");
            world.Update(s);

            Assert.AreEqual(world.Snakes[s._ID].Length, world.Scoreboard[s._ID]);
        }

        /// <summary>
        /// A food particle should be removed from the world after a snake eats it.
        /// </summary>
        [TestMethod]
        public void SnakeEatsFoodTest()
        {
            World world = new World(150, 150);
            Food f = new Food(new Point(11, 11), 1);
            Snake s = new Snake(new Point(10, 11), new Point(9, 11), 1, "Solid");

            world.Update(f);
            world.Update(s);

            Assert.AreEqual(1, world.Food.Count);

            world.Update(new Snake(new Point(11, 11), new Point(9, 11), 1, "Solid"));
            world.Update(new Food(new Point(-1, -1), 1));

            Assert.AreEqual(0, world.Food.Count);
            Assert.AreEqual(3, world.Scoreboard[s._ID]);
        }

        /// <summary>
        /// Tests two snakes for equality.
        /// </summary>
        [TestMethod]
        public void SnakeEqualityTest1()
        {
            Snake s = new Snake(new Point(10, 10), new Point(15, 10), 2, "Solid");

            Assert.AreEqual(s, s);
        }

        /// <summary>
        /// Tests snake inequality.
        /// </summary>
        [TestMethod]
        public void SnakeEqualityTest2()
        {
            Snake s1 = new Snake(new Point(10, 10), new Point(15, 10), 2, "Solid");
            Snake s2 = new Snake(new Point(10, 9), new Point(10, 5), 3, "Liquid");

            Assert.AreNotEqual(s1, s2);
        }

        /// <summary>
        /// Tests null comparison.
        /// </summary>
        [TestMethod]
        public void SnakeEqualityTest3()
        {
            Snake s = new Snake(new Point(10, 10), new Point(15, 10), 2, "Solid");

            Assert.AreNotEqual(null, s);
        }

        /// <summary>
        /// Tests null comparison.
        /// </summary>
        [TestMethod]
        public void SnakeEqualityTest4()
        {
            Snake s = new Snake(new Point(10, 10), new Point(15, 10), 2, "Solid");
            Snake nullSnake = null;

            Assert.AreNotEqual(nullSnake, s);
        }

        /// <summary>
        /// Tests null comparison.
        /// </summary>
        [TestMethod]
        public void SnakeEqualityTest5()
        {
            Snake s = new Snake(new Point(10, 10), new Point(15, 10), 2, "Solid");
            Snake nullSnake = null;

            Assert.IsFalse(s.Equals(nullSnake));
        }

        /// <summary>
        /// Tests two snakes for equality.
        /// </summary>
        [TestMethod]
        public void SnakeEqualityTest6()
        {
            Snake s = new Snake(new Point(10, 10), new Point(15, 10), 2, "Solid");

            Assert.IsTrue(s.Equals(s));
        }

        /// <summary>
        /// Tests two points for equality.
        /// </summary>
        [TestMethod]
        public void PointEqualityTest1()
        {
            Point p = new Point(1, 1);

            Assert.IsTrue(p.Equals(p));
        }

        /// <summary>
        /// Tests two points for equality.
        /// </summary>
        [TestMethod]
        public void PointEqualityTest2()
        {
            Point p1 = new Point(1, 1);
            Point p2 = null;

            Assert.IsFalse(p1.Equals(p2));
        }

        /// <summary>
        /// Tests two points for equality.
        /// </summary>
        [TestMethod]
        public void PointEqualityTest3()
        {
            Point p1 = new Point(1, 1);
            Point p2 = new Point(2, 2);

            Assert.IsFalse(p1.Equals(p2));
        }

        /// <summary>
        /// Tests the method that adds random food to the world.
        /// </summary>
        [TestMethod]
        public void AddRandomFoodTest()
        {
            int foodAdded = 0;
            World world = new World(150, 150);
            world.AddRandomFood(ref foodAdded);
            world.AddRandomFood(ref foodAdded);
            world.AddRandomFood(ref foodAdded);
            world.AddRandomFood(ref foodAdded);
            world.AddRandomFood(ref foodAdded);
            world.PlayerID = 2;
            
            Assert.AreEqual(foodAdded, world.Food.Count);
            Assert.AreEqual(2, world.PlayerID);
        }

        /// <summary>
        /// Tests the method that recycles dead snakes into food objects.
        /// </summary>
        [TestMethod]
        public void SnakeRecyclingTest()
        {
            World world = new World(150, 150);
            List<Point> vertices = new List<Point>();
            vertices.Add(new Point(22, 16));
            vertices.Add(new Point(22, 14));
            vertices.Add(new Point(20, 14));
            vertices.Add(new Point(20, 20));

            Snake snake = new Snake(new Point(18, 20), new Point(21, 16), vertices, 0, "Solidus");
            List<Snake> deadSnakes = new List<Snake>();
            deadSnakes.Add(snake);

            int id = 0;
            float recycleRate = 0.5f;

            world.RecycleSnakes(deadSnakes, recycleRate, ref id);

            Assert.AreEqual(6, world.Food.Count);
        }

        /// <summary>
        /// Verifies that the world stores pixels properly.
        /// </summary>
        [TestMethod]
        public void WorldPixelTest()
        {
            World world = new World(50, 50);
            Assert.AreEqual(0, world.PixelsPerCell);
        }

        /// <summary>
        /// Tests connecting to remote host.
        /// </summary>
        [TestMethod]
        public void ConnectToServerTest()
        {
            MockServer.StartServer();

            CallbackDelegate callbackFunction = new CallbackDelegate(MockCallbackMethod);
            Socket socket = Network.ConnectToServer(callbackFunction, "localhost");

            Assert.IsTrue(socket.Connected);

            MockServer.StopServer();

            socket.Disconnect(false);
            Assert.IsFalse(socket.Connected);
        }

        /// <summary>
        /// A DNS that cannot be resolved should cause ConnectToServer to throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ConnectToServerExceptionTest()
        {
            try
            {
                MockServer.StartServer();
                CallbackDelegate callbackFunction = new CallbackDelegate(MockCallbackMethod);
                Socket socket = Network.ConnectToServer(callbackFunction, "cannot resolve this");
            }
            finally
            {
                MockServer.StopServer();
            }
        }

        /// <summary>
        /// A simple method used as a callback for network testing purposes.
        /// </summary>
        /// <param name="state"></param>
        private void MockCallbackMethod(SocketState state)
        {
            Console.WriteLine("This was called.");
        }
    }
}
