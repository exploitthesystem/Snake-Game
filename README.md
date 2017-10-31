# Snake-Game
A networked version of the game Snake, completed as a collaborative project.
# Authors: Marko Ljubicic, Preston Balfour

Implemented in C# using MS Visual Studio. The core Snake game platform consists of world objects, a network controller, a server, and a GUI client. A battery of unit tests is also included. The application-layer network protocol between the game client and server was transmited using JSON file format. Additional information pertaining to game state was preserved using XML.

World objects:
food - Represents a food item in the game.
Snake - Represents a snake object in the game.
World - Represents the world map.

Networking:
NetworkController - Manages client-server interfacing.
Server - Game server that hosts a multi-client session of Snake.

View - Client-side graphical user interface. It connects to a Snake server and maintains a player's session.
