﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using SFMLproject.Tiles;
using SFML.Graphics;
using SFML.System;

using SFMLproject.TextureFolder;
using SFMLproject.StaticFields;

using SFML.Window;
using SFMLproject.Object;
using System.Runtime.InteropServices;

namespace SFMLproject.Map
{
    class Map : Source, IDisposable
    {

        private Tile[,] tiles;
        //TODO: change row / column for x/y
        private uint mapX, mapY;
        private static TileFactory tileFactory = TileFactory.getInstance();
        private static View mapView = new View(new FloatRect(0, 0, Constants.tileSize * Constants.camRow, Constants.tileSize * Constants.camCol));
        private Vector2i characState = new Vector2i(0, 1);
        private bool disposeFlag = false;

        /*
            Load map from textfile
            EntryX, EntryY are character initial position
        */

        public Map(string filePath)
        {

            int spawnPointX = 0;
            int spawnPointY = 0;
            char buffer;
            string line;
            //string stringBuffer = "";
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.ASCII))
                {

                    line = streamReader.ReadLine();
                    mapX = uint.Parse(line);
                    line = streamReader.ReadLine();
                    mapY = uint.Parse(line);

                    line = streamReader.ReadLine();
                    spawnPointX = int.Parse(line);

                    line = streamReader.ReadLine();
                    spawnPointY = int.Parse(line);


                    tiles = new Tile[mapX, mapY];

                    // Reading map
                    for (uint j = 0; j < mapY; j++)
                    {
                        for (uint i = 0; i < mapX; i++)
                        {
                            buffer = (char) streamReader.Read();
                            while (buffer == '\r' || buffer == '\n')
                            {
                                buffer = (char) streamReader.Read();
                            }
                            if (buffer == '2')
                            {
                                line = streamReader.ReadLine();
                                line = streamReader.ReadLine();
                                Object.Character template = new Object.Character(line);
                                tiles[i, j] = tileFactory.generateTile(template,
                                    tileFactory.generateTile((int) TileType.empty));
                                //create character. Line = pathfile
                            }
                            else if (buffer == '4')
                            {
                                line = streamReader.ReadLine();
                                line = streamReader.ReadLine();
                                tiles[i, j] = tileFactory.generateTile(buffer, line);

                            }
                            else tiles[i, j] = tileFactory.generateTile(buffer - '0');


                            tiles[i, j].moveSprite(new Vector2f(i*Constants.tileSize, j*Constants.tileSize));
                        }
                    }
                    /*
                TODO
                Init a changer
            */

                    Object.Character c = new Object.Character("NouvelEtudiant", new Vector2i(spawnPointX, spawnPointY));
                    //Object.Character d = new Object.Character("File\\Perso\\perso 1.png", new Vector2i(4, 3));
                    //tiles[d.getMapPos().X, d.getMapPos().Y] = tileFactory.generateTile(d, tiles[d.getMapPos().X, d.getMapPos().Y]);
                    tiles[c.getMapPos().X, c.getMapPos().Y] = tileFactory.generateTile(c,
                        tiles[c.getMapPos().X, c.getMapPos().Y]);
                    Attach(tiles[c.getMapPos().X, c.getMapPos().Y]);
                    Map.mapView.Center = c.sprite.Position;
                    //tiles[3, 9] = tileFactory.generateTile((int)TileType.eventTrigger);

                    fileStream.Close();
                }
            }
        }

        public View getMapview()
        {
            return mapView;
        }

        public void moveMapView(Vector2f move)
        {
            mapView.Move(move); 
        }

        public void setTile(Vector2i pos, Tile tile)
        {
            tiles[pos.X, pos.Y] = tile;
        }

        public Tile getTile(Vector2i pos)
        {
            return tiles[pos.X, pos.Y];
        }

        /*
           Move character WASD
        */
        public bool moveCharac(Keyboard.Key e)
        {

            switch (e)
            {
                case Keyboard.Key.D:
                    notify(new Vector2i(1, 0));
                    characState = new Vector2i(1, 0);
                    return true;
                   

                case Keyboard.Key.A:
                    notify(new Vector2i(-1, 0));
                    characState = new Vector2i(-1, 0);
                    return true;
                    

                case Keyboard.Key.W:
                    notify(new Vector2i(0, -1));
                    characState = new Vector2i(0, -1);
                    return true;
                    

                case Keyboard.Key.S:
                    notify(new Vector2i(0, 1));
                    characState = new Vector2i(0, 1);
                    return true;
                    
            }
            return false;
        }

        /*
            Draw all tile in camera sight
        */
        public void draw(RenderWindow window)
        {
            for (int i = (int)mapY - 1; i >= 0; --i)
            {
                for (int j = (int)mapX - 1; j >= 0; --j) {
                    tiles[j, i].draw(window);
                }
            }
        }

        public bool actionButton(Keyboard.Key e)
        {
            switch (e)
            {
                case Keyboard.Key.E:
                    notifyAction(characState);
                    return true;
                case Keyboard.Key.Q:
                    Console.WriteLine("Stop the action/exit menu.");
                    return true;
                case Keyboard.Key.Return:
                    Console.WriteLine("MENU");
                    return true;
            }
            return false;
        }

        public override void notifyAction(Vector2i m)
        {
            obt.ForEach(delegate (Observer obs)
            {
                obs.updateOnReact(m);
            });
            //KillAll();
            Dequeue();
        }

        public override void notify(Vector2i m)
        {
            obt.ForEach(delegate (Observer obs)
            {
                obs.updateOnLeave(m);
            });
            KillAll();
            Dequeue();
        }
    
        public override void Dispose()
        {
            for (uint j = 0; j < mapY; j++)
            {
                for (uint i = 0; i < mapX; i++)
                {
                    tiles[i, j].Dispose();
                    tiles[i, j] = null;
                }
            }
            base.Dispose();
        }
    
    }
}
