/***********************************************************************
* Project :    PS7 - Snake
* File    :    Form1.cs
* Name    :    Marko Ljubicic, Preston Balfour
* Date    :    11/22/2016
*
* Description:   This code creates the player's view for the game Snake.
*                It (1) uses the world model defined in the World class, 
*                (2) draws the GUI controls and game scene, (3) handles
*                user input, and (4) utilizes the NetworkController library
*                to handle communication with the server.
* 
************************************************************************/

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Snake;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

namespace View
{
    public partial class Form1 : Form
    {
        // The player ID assigned to the client by the server.
        private int playerID;
        // The height and width (in units of grid cells) of the game world.
        private int height, width;
        // The world object that models the game.
        private World world;
        // The socket used to communicate with the server.
        private Socket theServer;
        // The last direction sent to the server.
        private int lastSentDirection;
        // The number of pixels per each cell in the world.
        private const int pixelsPerCell = 5;
        // Is the socket still connected?
        private bool isConnected;

        /// <summary>
        /// The entry point into the View.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            MaximizeBox = false;
            MinimizeBox = false;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            // If the server address text box is empty, prompt user to enter an address or hostname.
            if (serverTextBox.Text == "")
            {
                MessageBox.Show("Please enter a server address.");
                return;
            }

            try
            {
                // Disable the controls and try to connect
                connectButton.Enabled = false;
                serverTextBox.Enabled = false;
                nameTextBox.Enabled = false;

                // Create a delegate that determines what to do after the connection is completed
                // and pass it to a static ConnectToServer method.
                CallbackDelegate callbackFunction = new CallbackDelegate(FirstContact);
                theServer = Network.ConnectToServer(callbackFunction, serverTextBox.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not connect to server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Re-enable the controls and allow for re-connect
                connectButton.Enabled = true;
                serverTextBox.Enabled = true;
                nameTextBox.Enabled = true;

                return;
            }
        }

        /// <summary>
        /// A controller method that is called by Network.ConnectedToServer method
        /// after establishing a connection with the server. It sends the 
        /// client socket and player name to the server.
        /// </summary>
        /// <param name="state"></param>
        private void FirstContact(SocketState state)
        {
            state.Callback = ReceiveStartup;
            Network.Send(state.Socket, nameTextBox.Text + "\n");
        }

        /// <summary>
        /// A controller method that is sent as a delegate by FirstContact after
        /// the initial connection has been made and the player name has been sent
        /// to the server. It expects to receive the player ID along with the
        /// world height and width. It then extracts this data and requests more 
        /// from the server, passing ReceiveWorld as a delgate.
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveStartup(SocketState state)
        {
            Int32.TryParse(state.Messages[0], out playerID); // get player ID
            Int32.TryParse(state.Messages[1], out height);   // get world height
            Int32.TryParse(state.Messages[2], out width);    // get world width

            world = new World(height, width);
            world.PixelsPerCell = pixelsPerCell;

            // Pass world to panels.
            drawingPanel1.SetWorld(world);
            writingPanel1.SetWorld(world);

            // Resize form.
            this.Invoke(new MethodInvoker(UpdateSize));

            state.Callback = ReceiveWorld;
            Network.GetData(state);

            isConnected = true;
            world.PlayerID = playerID;
        }

        /// <summary>
        /// A controller method that receives and extracts world information
        /// from the server.
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveWorld(SocketState state)
        {
            for (int i = 0; i < state.DataLength; i++)
            {
                JObject obj = JObject.Parse(state.Messages[i]);
                JToken jAttribute = obj["vertices"];

                if (jAttribute != null)
                {
                    lock (this)
                    {
                        // If the received message deserializes to a snake, update the world with a new snake.
                        Snake.Snake snake = JsonConvert.DeserializeObject<Snake.Snake>(state.Messages[i]);
                        if (snake._ID == playerID && snake.Vertices.Last.Value._x == -1 && snake.Vertices.Last.Value._y == -1)
                        {
                            isConnected = false;
                            Invoke(new MethodInvoker(AllowReconnect));
                        }
                        world.Update(snake);
                    }
                }
                else // else update the world with a food object.
                {
                    lock (this)
                    {
                        Food food = JsonConvert.DeserializeObject<Food>(state.Messages[i]);
                        world.Update(food);
                    }
                }
            }
            UpdateFrame();
            BeginInvoke(new MethodInvoker(UpdateScoreboard));
            Network.GetData(state);
        }

        /// <summary>
        /// This method updates the frame by having it re-drawn.
        /// </summary>
        private void UpdateFrame()
        {
            drawingPanel1.Invalidate();
        }


        /// <summary>
        /// Updates the scoreboard by referencing a sorted dictionary of top scores.
        /// </summary>
        private void UpdateScoreboard()
        {
            writingPanel1.Invalidate();
            writingPanel1.Update();
        }

        /// <summary>
        /// Sets the size of the drawing panel and resizes the form.
        /// </summary>
        private void UpdateSize()
        {
            // 231 and 92 are constants determined from the fixed proportions of the GUI window.
            drawingPanel1.Size = new Size(width * pixelsPerCell, height * pixelsPerCell);
            this.Size = new Size(width * pixelsPerCell + 231, height * pixelsPerCell + 92);
        }

        /// <summary>
        /// Re-enables the re-connect button after death.
        /// </summary>
        private void AllowReconnect()
        {
            connectButton.Enabled = true;
            nameTextBox.Enabled = true;
        }

        /// <summary>
        /// Enter key press activates connect button handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendConnectRequestOnEnterPress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && nameTextBox.Text.Length >= 1)
                connectButton_Click(sender, e);
        }

        /// <summary>
        /// Direction change event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArrowKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (isConnected)
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        ValidateDirectionChangeRequest(4);
                        break;
                    case Keys.Up:
                        ValidateDirectionChangeRequest(1);
                        break;
                    case Keys.Right:
                        ValidateDirectionChangeRequest(2);
                        break;
                    case Keys.Down:
                        ValidateDirectionChangeRequest(3);
                        break;
                    case Keys.W:
                        ValidateDirectionChangeRequest(1);
                        break;
                    case Keys.A:
                        ValidateDirectionChangeRequest(4);
                        break;
                    case Keys.S:
                        ValidateDirectionChangeRequest(3);
                        break;
                    case Keys.D:
                        ValidateDirectionChangeRequest(2);
                        break;
                }
        }

        /// <summary>
        /// Prevents sending the same direction request repeatedly in succession.
        /// </summary>
        /// <param name="x"></param>
        private void ValidateDirectionChangeRequest(int x)
        {
            if (x != world.WorldMap[world.Snakes[playerID].Vertices.Last.Value._x, world.Snakes[playerID].Vertices.Last.Value._y].direction)
            {
                Network.Send(theServer, "(" + x.ToString() + ")\n");
                lastSentDirection = x;
            }
        }
    }


    /// <summary>
    /// This is a helper class for drawing a world.
    /// One of these panels is placed in our GUI, alongside other controls.
    /// Anything drawn within this panel will use a local coordinate system.
    /// </summary>
    public class DrawingPanel : Panel
    {
        /// We need a reference to the world, so we can draw the objects in it
        private World world;

        /// <summary>
        /// Constructs a DrawingPanel object.
        /// </summary>
        public DrawingPanel()
        {
            // Setting this property to true prevents flickering
            this.DoubleBuffered = true;
        }

        /// <summary>
        /// Pass in a reference to the world, so we can draw the objects in it
        /// </summary>
        /// <param name="_world"></param>
        public void SetWorld(World _world)
        {
            world = _world;
        }

        /// <summary>
        /// Override the behavior when the panel is redrawn
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // If we don't have a reference to the world yet, nothing to draw.
            if (world == null)
                return;

            // Turn on anti-aliasing for smooth round edges
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush drawBrush = new SolidBrush(Color.Black))
            {
                //Zooming feature. Calculates the necessary scales and translations for
                //zooming on the player's snake.
                if (world.Snakes.ContainsKey(world.PlayerID))
                {
                    float centerX = world.Width / 2;
                    float centerY = world.Height / 2;

                    float scaleX = world.Width / (world.Snakes[world.PlayerID].Length * 2);
                    float scaleY = world.Height / (world.Snakes[world.PlayerID].Length * 2);

                    float translateX = (centerX / scaleX) - world.Snakes[world.PlayerID].Vertices.Last.Value._x;
                    float translateY = (centerY / scaleX) - world.Snakes[world.PlayerID].Vertices.Last.Value._y;

                    translateX *= world.PixelsPerCell;
                    translateY *= world.PixelsPerCell;

                    if (translateX != world.Width && translateY != world.Height)
                    {
                        e.Graphics.ScaleTransform((int)scaleX, (int)scaleY);
                        e.Graphics.TranslateTransform((int)translateX, (int)translateY);
                    }
                }

                // Draw the top wall
                Rectangle topWall = new Rectangle(0, 0, Size.Width, world.PixelsPerCell);
                e.Graphics.FillRectangle(drawBrush, topWall);

                // Draw the right wall
                Rectangle rightWall = new Rectangle((world.Width - 1) * world.PixelsPerCell, 0, world.PixelsPerCell, Size.Height);
                e.Graphics.FillRectangle(drawBrush, rightWall);

                // Draw the left wall
                Rectangle leftWall = new Rectangle(0, 0, world.PixelsPerCell, Size.Height);
                e.Graphics.FillRectangle(drawBrush, leftWall);

                // Draw the bottom wall
                Rectangle bottomWall = new Rectangle(0, (world.Height - 1) * world.PixelsPerCell, Size.Width, world.PixelsPerCell);
                e.Graphics.FillRectangle(drawBrush, bottomWall);

                // Draw the snakes and food.
                for (int i = 0; i < world.Height; i++)
                    for (int j = 0; j < world.Width; j++)
                    {
                        Rectangle newCell = new Rectangle(i * world.PixelsPerCell, j * world.PixelsPerCell, world.PixelsPerCell, world.PixelsPerCell);
                        e.Graphics.FillEllipse(new SolidBrush(world.WorldMap[i, j].color), newCell);
                    }
            }
        }
    }

    /// <summary>
    /// This is a helper class for drawing text to a panel.
    /// One of these panels is placed in our GUI, alongside other controls.
    /// Anything drawn within this panel will use a local coordinate system.
    /// </summary>
    public class WritingPanel : Panel
    {
        /// We need a reference to the world, so we can draw the objects in it
        private World world;

        /// <summary>
        /// Constructs a DrawingPanel object.
        /// </summary>
        public WritingPanel()
        {
            // Setting this property to true prevents flickering
            this.DoubleBuffered = true;
        }

        /// <summary>
        /// Pass in a reference to the world, so we can draw the objects in it
        /// </summary>
        /// <param name="_world"></param>
        public void SetWorld(World _world)
        {
            world = _world;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // If we don't have a reference to the world yet, nothing to draw.
            if (world == null)
                return;

            // Turn on anti-aliasing for smooth round edges
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw the text for the scoreboard.
            using (Font font = new Font("Microsoft Sans Serif", 14, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                var sortedDict = (from entry in world.Scoreboard orderby entry.Value descending select entry).Take(10);
                int i = 0;
                foreach (KeyValuePair<int, int> el in sortedDict)
                {
                    string scoreMessage = world.Snakes[el.Key]._name + "        " + el.Value.ToString();
                    System.Drawing.Point point1 = new System.Drawing.Point(0, 10 + i);
                    TextRenderer.DrawText(e.Graphics, scoreMessage, font, point1, world.Snakes[el.Key].snakeColor);
                    i += 30;
                }
            }
        }
    }
}
