﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using MegaMan.Common;

namespace MegaMan.Engine
{
    public class ScreenHandler : IScreenInformation
    {
        private readonly MapSquare[][] tiles;
        public Screen Screen { get; private set; }
        private readonly List<BlocksPattern> patterns;
        private GameEntity[] entities;
        private readonly List<GameEntity> spawnedEntities;
        private bool[] spawnable;
        private readonly List<JoinHandler> joins;
        private readonly List<bool> teleportEnabled;

        private readonly PositionComponent PlayerPos;

        public Music Music { get; private set; }

        public float OffsetX { get; private set; }
        public float OffsetY { get; private set; }

        public int TileSize { get { return Screen.Tileset.TileSize; } }

        public event Action<JoinHandler> JoinTriggered;
        public event Action<TeleportInfo> Teleport;

        public event Action BossDefeated;

        public ScreenHandler(Screen screen, PositionComponent playerPos, IEnumerable<Join> mapJoins)
        {
            Screen = screen;
            patterns = new List<BlocksPattern>();
            spawnedEntities = new List<GameEntity>();

            tiles = new MapSquare[Screen.Height][];
            for (int y = 0; y < Screen.Height; y++)
            {
                tiles[y] = new MapSquare[Screen.Width];
                for (int x = 0; x < Screen.Width; x++)
                {
                    try
                    {
                        Tile tile = Screen.TileAt(x, y);
                        tiles[y][x] = new MapSquare(Screen, tile, x, y, x * Screen.Tileset.TileSize, y * Screen.Tileset.TileSize);
                    }
                    catch
                    {
                        throw new GameEntityException("There's an error in map " + screen.Map.Name + ", screen file " + screen.Name + ".scn,\nthere's a bad tile number somewhere.");
                    }
                }
            }

            foreach (BlockPatternInfo info in Screen.BlockPatternInfo)
            {
                BlocksPattern pattern = new BlocksPattern(info);
                patterns.Add(pattern);
            }

            PlayerPos = playerPos;

            joins = new List<JoinHandler>();
            foreach (Join join in mapJoins)
            {
                if (join.screenOne == Screen.Name || join.screenTwo == Screen.Name)
                {
                    JoinHandler handler = JoinHandler.Create(join, this);
                    handler.Start(this);
                    joins.Add(handler);
                }
            }

            teleportEnabled = new List<bool>(screen.Teleports.Select(info => false));

            string intropath = (screen.MusicIntroPath != null) ? screen.MusicIntroPath.Absolute : null;
            string looppath = (screen.MusicLoopPath != null) ? screen.MusicLoopPath.Absolute : null;
            if (intropath != null || looppath != null) Music = Engine.Instance.SoundSystem.LoadMusic(intropath, looppath, 1);
        }

        public JoinHandler GetJoinHandler(Join join)
        {
            return joins.FirstOrDefault(myjoin => myjoin.JoinInfo.Equals(join));
        }

        public void Start()
        {
            entities = new GameEntity[Screen.EnemyInfo.Count];
            spawnable = new bool[Screen.EnemyInfo.Count];

            // place persistent entities
            for (int i = 0; i < Screen.EnemyInfo.Count; i++)
            {
                if (entities[i] != null) continue; // already on screen

                PlaceEntity(i);
            }

            foreach (BlocksPattern pattern in patterns)
            {
                pattern.Start();
            }

            Engine.Instance.GameThink += Instance_GameThink;
        }

        // these frames only happen if we are not paused / scrolling
        public void Update()
        {
            foreach (JoinHandler join in joins)
            {
                if (join.Trigger(PlayerPos.Position))
                {
                    if (JoinTriggered != null) JoinTriggered(join);
                    return;
                }
            }

            // check for teleports
            for (int i = 0; i < Screen.Teleports.Count; i++)
            {
                TeleportInfo teleport = Screen.Teleports[i];

                if (teleportEnabled[i])
                {
                    if (Math.Abs(PlayerPos.Position.X - teleport.From.X) <= 2 && Math.Abs(PlayerPos.Position.Y - teleport.From.Y) <= 8)
                    {
                        if (Teleport != null) Teleport(teleport);
                        break;
                    }
                }
                else if (Math.Abs(PlayerPos.Position.X - teleport.From.X) >= 16 || Math.Abs(PlayerPos.Position.Y - teleport.From.Y) >= 16)
                {
                    teleportEnabled[i] = true;
                }
            }

            // if the player is not colliding, they'll be allowed to pass through the walls (e.g. teleporting)
            if ((Game.CurrentGame.CurrentMap.Player.GetComponent<CollisionComponent>()).Enabled)
            {
                // now if we aren't scrolling, hold the player at the screen borders
                if (PlayerPos.Position.X >= Screen.PixelWidth - Const.PlayerScrollTrigger)
                {
                    PlayerPos.SetPosition(new PointF(Screen.PixelWidth - Const.PlayerScrollTrigger, PlayerPos.Position.Y));
                }
                else if (PlayerPos.Position.X <= Const.PlayerScrollTrigger)
                {
                    PlayerPos.SetPosition(new PointF(Const.PlayerScrollTrigger, PlayerPos.Position.Y));
                }
                else if (PlayerPos.Position.Y > Screen.PixelHeight - Const.PlayerScrollTrigger)
                {
                    if (Game.CurrentGame.GravityFlip) PlayerPos.SetPosition(new PointF(PlayerPos.Position.X, Screen.PixelHeight - Const.PlayerScrollTrigger));
                    // bottomless pit death!
                    else if (PlayerPos.Position.Y > Game.CurrentGame.PixelsDown + 32) PlayerPos.Parent.Die();
                }
                else if (PlayerPos.Position.Y < Const.PlayerScrollTrigger)
                {
                    if (!Game.CurrentGame.GravityFlip) PlayerPos.SetPosition(new PointF(PlayerPos.Position.X, Const.PlayerScrollTrigger));
                    else if (PlayerPos.Position.Y < -32) PlayerPos.Parent.Die();
                }
            }
        }

        public void AddSpawnedEntity(GameEntity entity)
        {
            spawnedEntities.Add(entity);
        }

        // because it is a thinking event, it happens every frame
        private void Instance_GameThink()
        {
            // place any entities that have just appeared on screen
            for (int i = 0; i < Screen.EnemyInfo.Count; i++)
            {
                if (entities[i] != null) continue; // already on screen
                if (!IsOnScreen(Screen.EnemyInfo[i].screenX, Screen.EnemyInfo[i].screenY))
                {
                    spawnable[i] = true;    // it's off-screen, so it can spawn next time it's on screen
                    continue;
                }
                if (!spawnable[i]) continue;

                PlaceEntity(i);
            }
        }

        public bool IsOnScreen(float x, float y)
        {
            return x >= OffsetX && y >= OffsetY &&
                x <= OffsetX + Game.CurrentGame.PixelsAcross &&
                y <= OffsetY + Game.CurrentGame.PixelsDown;
        }

        private void PlaceEntity(int index)
        {
            spawnable[index] = false;
            EnemyCopyInfo info = Screen.EnemyInfo[index];

            GameEntity enemy = GameEntity.Get(info.enemy);
            if (enemy == null) return;
            PositionComponent pos = enemy.GetComponent<PositionComponent>();
            if (!pos.PersistOffScreen && !IsOnScreen(info.screenX, info.screenY)) return; // what a waste of that allocation...

            pos.SetPosition(new PointF(info.screenX, info.screenY));
            if (info.state != "Start")
            {
                StateMessage msg = new StateMessage(null, info.state);
                enemy.SendMessage(msg);
            }
            enemy.Start();
            if (info.boss)
            {
                HealthComponent health = enemy.GetComponent<HealthComponent>();
                health.DelayFill(120);
                BossFightTimer();
                enemy.Death += () =>
                {
                    if (Music != null) Music.FadeOut(30);
                    (Game.CurrentGame.CurrentMap.Player.GetComponent<InputComponent>()).Paused = true;
                    Engine.Instance.DelayedCall(() => Game.CurrentGame.CurrentMap.Player.SendMessage(new StateMessage(null, "TeleportStart")), null, 120);
                    Engine.Instance.DelayedCall(() => { if (BossDefeated != null) BossDefeated(); }, null, 240);
                };
            }
            if (info.pallete != "Default" && info.pallete != null)
            {
                (enemy.GetComponent<SpriteComponent>()).ChangeGroup(info.pallete);
            }
            entities[index] = enemy;
            enemy.Stopped += () => entities[index] = null;
        }

        private void BossFightTimer()
        {
            InputComponent input = Game.CurrentGame.CurrentMap.Player.GetComponent<InputComponent>();
            input.Paused = true;
            Engine.Instance.DelayedCall(() => { input.Paused = false; }, null, 200);
        }

        public void Stop()
        {
            for (int i = 0; i < entities.Length; i++ )
            {
                if (entities[i] != null) entities[i].Stop();
                entities[i] = null;
            }

            foreach (GameEntity entity in spawnedEntities)
            {
                entity.Stop();
            }
            spawnedEntities.Clear();

            foreach (BlocksPattern pattern in patterns)
            {
                pattern.Stop();
            }

            Engine.Instance.GameThink -= Instance_GameThink;
        }

        public void Clean()
        {
            foreach (JoinHandler join in joins)
            {
                join.Stop();
            }
        }

        public MapSquare SquareAt(int x, int y)
        {
            if (y < 0 || y >= tiles.GetLength(0)) return null;
            if (x < 0 || x >= tiles[y].GetLength(0)) return null;
            return tiles[y][x];
        }

        public IEnumerable<MapSquare> Tiles
        {
            get 
            {
                return tiles.SelectMany(row => row);
            }
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch batch, float adj_x = 0, float adj_y = 0, float off_x = 0, float off_y = 0)
        {
            int width = Screen.PixelWidth;
            int height = Screen.PixelHeight;

            OffsetX = OffsetY = 0;

            float cx = PlayerPos.Position.X + adj_x;
            float cy = PlayerPos.Position.Y + adj_y;

            if (cx > Game.CurrentGame.PixelsAcross / 2)
            {
                OffsetX = cx - Game.CurrentGame.PixelsAcross / 2;
                if (OffsetX > width - Game.CurrentGame.PixelsAcross) OffsetX = width - Game.CurrentGame.PixelsAcross;
            }

            if (cy > Game.CurrentGame.PixelsDown / 2)
            {
                OffsetY = cy - Game.CurrentGame.PixelsDown / 2;
                if (OffsetY > height - Game.CurrentGame.PixelsDown) OffsetY = height - Game.CurrentGame.PixelsDown;
                if (OffsetY < 0) OffsetY = 0;
            }

            OffsetX += off_x;
            OffsetY += off_y;

            Screen.DrawXna(batch, Engine.Instance.OpacityColor, -OffsetX, -OffsetY, Game.CurrentGame.PixelsAcross, Game.CurrentGame.PixelsDown);
        }

        public Tile TileAt(int tx, int ty)
        {
            return Screen.TileAt(tx, ty);
        }
    }
}
