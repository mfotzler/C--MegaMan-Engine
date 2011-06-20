﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using MegaMan;

namespace Mega_Man
{
    public class HealthMeter : IHandleGameEvents
    {
        private float value;
        private float maxvalue;
        private float tickSize;
        private Texture2D meterTexture;
        private Texture2D tickTexture;
        private string sound;
        private int tickframes;
        private int stopvalue;

        private Point tickOffset;

        private bool running;
        private bool animating;

        private float positionX;
        private float positionY;

        private bool horizontal;

        // true if the meter is shown overlaid on gameplay, not in a pause screen
        private bool inGamePlay;

        private RectangleF bounds;
        public RectangleF Bounds { get { return bounds; } }

        private static readonly List<HealthMeter> allMeters = new List<HealthMeter>();

        public static IEnumerable<HealthMeter> AllMeters
        {
            get { return allMeters.AsReadOnly(); }
        }

        public static void Unload()
        {
            allMeters.Clear();
        }

        // this is just needed so we can pause the game when filling
        public bool IsPlayer { get; set; }

        /// <summary>
        /// Do not rely on this as the actual health. It can differ while it animates. It only reflects the value present in the bar itself.
        /// </summary>
        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                if (running && value > this.value) // tick up slowly
                {
                    tickframes = 0;
                    stopvalue = (int)value;
                    if (!animating)
                    {
                        Engine.Instance.GameLogicTick += GameTick;
                        animating = true;
                        if (sound != null) Engine.Instance.SoundSystem.PlaySfx(sound);
                    }
                }
                else
                {
                    this.value = value;
                    if (this.value < 0) this.value = 0;
                }
            }
        }

        void UpTick()
        {
            tickframes++;
            if (tickframes > 1 && IsPlayer) Game.CurrentGame.Pause();
            if (tickframes >= 3)
            {
                tickframes = 0;
                value += tickSize;
            }
        }

        public float MaxValue
        {
            get { return maxvalue; }
            set
            {
                maxvalue = value;
                tickSize = maxvalue / 28;
            }
        }

        public static HealthMeter Create(MeterInfo info, bool inGamePlay)
        {
            var meter = new HealthMeter();
            meter.LoadInfo(info);
            meter.inGamePlay = inGamePlay;
            if (inGamePlay) allMeters.Add(meter);
            return meter;
        }

        public static HealthMeter Create(XElement node, bool inGamePlay)
        {
            var meter = new HealthMeter();
            meter.LoadXml(node);
            meter.inGamePlay = inGamePlay;
            if (inGamePlay) allMeters.Add(meter);
            return meter;
        }

        private HealthMeter()
        {
            value = maxvalue;
            running = false;
        }

        private int filldelay;
        public void DelayedFill(int frames)
        {
            Value = 0;
            filldelay = frames;
            Engine.Instance.GameThink += DelayFill;
        }

        void DelayFill()
        {
            filldelay--;
            if (filldelay == 0)
            {
                Engine.Instance.GameThink -= DelayFill;
                Value = MaxValue;
            }
        }

        private void LoadInfo(MeterInfo info)
        {
            positionX = info.Position.X;
            positionY = info.Position.Y;

            if (tickTexture != null) tickTexture.Dispose();
            StreamReader srTick = new StreamReader(info.TickImage.Absolute);
            tickTexture = Texture2D.FromStream(Engine.Instance.GraphicsDevice, srTick.BaseStream);

            if (info.Background != null)
            {
                StreamReader srMeter = new StreamReader(info.Background.Absolute);
                meterTexture = Texture2D.FromStream(Engine.Instance.GraphicsDevice, srMeter.BaseStream);
                bounds = new RectangleF(positionX, positionY, meterTexture.Width, meterTexture.Height);
            }

            horizontal = (info.Orient == MeterInfo.Orientation.Horizontal);
            tickOffset = info.TickOffset;

            if (info.Sound != null) sound = Engine.Instance.SoundSystem.EffectFromInfo(info.Sound);
        }

        private void LoadXml(XElement node)
        {
            positionX = node.GetFloat("x");
            positionY = node.GetFloat("y");
            XAttribute imageAttr = node.RequireAttribute("image");
            
            if (tickTexture != null) tickTexture.Dispose();
            StreamReader srTick = new StreamReader(Path.Combine(Game.CurrentGame.BasePath, Path.Combine(Game.CurrentGame.BasePath, imageAttr.Value)));
            tickTexture = Texture2D.FromStream(Engine.Instance.GraphicsDevice, srTick.BaseStream);

            XAttribute backAttr = node.Attribute("background");
            if (backAttr != null)
            {
                StreamReader srMeter = new StreamReader(Path.Combine(Game.CurrentGame.BasePath, Path.Combine(Game.CurrentGame.BasePath, backAttr.Value)));
                meterTexture = Texture2D.FromStream(Engine.Instance.GraphicsDevice, srMeter.BaseStream);
                bounds = new RectangleF(positionX, positionY, meterTexture.Width, meterTexture.Height);
            }

            bool horiz = false;
            XAttribute dirAttr = node.Attribute("orientation");
            if (dirAttr != null)
            {
                horiz = (dirAttr.Value == "horizontal");
            }
            horizontal = horiz;

            int x; int y;
            node.TryInteger("tickX", out x);
            node.TryInteger("tickY", out y);

            tickOffset = new Point(x, y);

            XElement soundNode = node.Element("Sound");
            if (soundNode != null) sound = Engine.Instance.SoundSystem.EffectFromXml(soundNode);
        }

        public void Reset()
        {
            value = maxvalue;
        }

        public void Draw(SpriteBatch batch)
        {
            Draw(batch, positionX, positionY);
        }

        private void Draw(SpriteBatch batch, float positionX, float positionY)
        {
            if (tickTexture != null)
            {
                int i = 0;
                int ticks = (int)Math.Ceiling(value / tickSize);
                // prevent float errors
                if (ticks > 28) ticks = 28;

                if (meterTexture != null) batch.Draw(meterTexture, new Microsoft.Xna.Framework.Vector2(positionX, positionY), Engine.Instance.OpacityColor);
                if (horizontal)
                {
                    for (int y = (int)positionX; i < ticks; i++, y += tickTexture.Width)
                    {
                        batch.Draw(tickTexture, new Microsoft.Xna.Framework.Vector2(y, positionY), Engine.Instance.OpacityColor);
                    }
                }
                else
                {
                    for (int y = 54 + (int)positionY; i < ticks; i++, y -= tickTexture.Height)
                    {
                        batch.Draw(tickTexture, new Microsoft.Xna.Framework.Vector2(positionX + tickOffset.X, y + tickOffset.Y), Engine.Instance.OpacityColor);
                    }
                }
            }
        }

        #region IHandleGameEvents Members

        public void StartHandler()
        {
            Engine.Instance.GameRender += GameRender;
            Game.CurrentGame.AddGameHandler(this);
            running = true;
        }

        public void StopHandler()
        {
            Engine.Instance.GameLogicTick -= GameTick;
            Engine.Instance.GameRender -= GameRender;
            Game.CurrentGame.RemoveGameHandler(this);
            running = false;
        }

        public void GameInputReceived(GameInputEventArgs e)
        {
            
        }

        private void GameTick(GameTickEventArgs e)
        {
            UpTick();
            if (value >= stopvalue || value >= maxvalue)
            {
                Engine.Instance.GameLogicTick -= GameTick;
                animating = false;
                if (IsPlayer) Game.CurrentGame.Unpause();
                if (sound != null) Engine.Instance.SoundSystem.StopSfx(sound);
            }
        }

        public void GameRender(GameRenderEventArgs e)
        {
            if (inGamePlay && Engine.Instance.SpritesFour) Draw(e.Layers.SpritesBatch[3], positionX, positionY);
        }

        #endregion
    }
}
