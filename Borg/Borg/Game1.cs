using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Borg
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch sb;

        Texture2D turretT;
        Texture2D[] tubesT = new Texture2D[4];
        Texture2D torpedoT;
        Texture2D enemytorpedoT;
        Texture2D borgT;
        Texture2D explosionT;
        Rectangle turret;
        Rectangle[] tubes = new Rectangle[4];
        Rectangle torpedo;
        Rectangle enemytorpedo;
        Rectangle borg;
        Rectangle explosion;
        Rectangle phaser;

        Color[] colors = {Color.Blue, Color.Green, Color.Orange, Color.Red};

        Texture2D white;
        Rectangle chargemask;
        Rectangle chargebar;
        Rectangle explosivemask;
        Rectangle[] explosivebar = new Rectangle[9];
        Rectangle propulsivemask;
        Rectangle[] propulsivebar = new Rectangle[9];

        int selected;
        int torpedoindex;
        int torpedodistance;
        double factor;
        int borgindex;
        int phasindex;
        int phascolor;
        bool fired;
        bool phasfired;
        bool enemyfired;
        bool canspawn;
        int power;
        int phaspower;
        int energy;
        int propulsion;
        int timer = 0;
        int borgtimer = 0;
        int phastimer = 0;

        String display = "";
        SpriteFont sf;

        SoundEffect enemytorpedoS;
        SoundEffect torpedoS;
        SoundEffect phaserS;

        Random random;

        KeyboardState oldkb = Keyboard.GetState();
        MouseState oldmouse = Mouse.GetState();
        GamePadState oldpad = GamePad.GetState(PlayerIndex.One);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true;
            
            // TODO: Add your initialization logic here
            turret = new Rectangle(350, 200, 50, 50);
            torpedo = new Rectangle(-200, -200, 100, 100);
            enemytorpedo = new Rectangle(-200, -200, 40, 40);
            for (int i = 0; i < tubes.Length; i++)
                tubes[i] = new Rectangle((int)(350 + Math.Cos(i * Math.PI * 0.5) * 50), (int)(200 - Math.Sin(i * Math.PI * 0.5) * 50), 25 + ((i+1) % 2) * 25, 25 + (i % 2) * 25);
            borg = new Rectangle(-110, -110, 100, 100);
            explosion = new Rectangle(-200, -200, 100, 100);
            phaser = new Rectangle(-110, -110, 100, 100);

            chargemask = new Rectangle(10, 10, 120, 30);
            explosivemask = new Rectangle(10, 50, 119, 30);
            propulsivemask = new Rectangle(10, 90, 119, 30);
            chargebar = new Rectangle(20, 20, 100, 10);
            for (int e = 0; e < explosivebar.Length; e++)
            {
                explosivebar[e] = new Rectangle(20 + (10*e), 60, 9, 10);
            }
            for (int p = 0; p < propulsivebar.Length; p++)
            {
                propulsivebar[p] = new Rectangle(20 + (10 * p), 100, 9, 10);
            }

            selected = 1;
            torpedoindex = 1;
            torpedodistance = 100;
            factor = 40;
            borgindex = 1;
            phasindex = 1;
            phascolor = 1;
            power = 1;
            phaspower = 1;
            propulsion = 1;
            energy = 100;

            fired = false;
            phasfired = false;
            enemyfired = false;
            canspawn = true;

            random = new Random();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            sb = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            for (int i = 0; i < tubesT.Length; i++)
                tubesT[i] = this.Content.Load<Texture2D>("tube" + i);
            turretT = this.Content.Load<Texture2D>("turret");
            torpedoT = this.Content.Load<Texture2D>("torpedo");
            enemytorpedoT = this.Content.Load<Texture2D>("enemytorpedo");
            borgT = this.Content.Load<Texture2D>("borg_cube");
            explosionT = this.Content.Load<Texture2D>("download");

            white = this.Content.Load<Texture2D>("white");

            sf = this.Content.Load<SpriteFont>("SpriteFont1");

            enemytorpedoS = this.Content.Load<SoundEffect>("ETORP");
            torpedoS = this.Content.Load<SoundEffect>("TORP");
            phaserS = this.Content.Load<SoundEffect>("PHAS");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            GamePadState pad = GamePad.GetState(PlayerIndex.One);
            
            // Allows the game to exit
            if (pad.Buttons.Back == ButtonState.Pressed || kb.IsKeyDown(Keys.Escape))
                this.Exit();

            /*
             * ENERGY REGEN BEHAVIOR
             */
            timer = (timer + 1) % 180;
            if (timer == 0)
                energy = Math.Min(100, energy+3);
            
            /*
             * TORPEDO BEHAVIOR
             */ 
            if (fired)
            {
                int eX = torpedo.X;
                int eY = torpedo.Y;

                switch (torpedoindex)
                {
                    case 0:
                        torpedo.X += 5;
                        if (torpedoindex == borgindex && torpedo.X >= borg.X)
                        {
                            borg.X = -110;
                            torpedo.X = -110;
                        }
                        break;
                    case 1:
                        torpedo.Y -= 5;
                        if (torpedoindex == borgindex && torpedo.Y <= borg.Y)
                        {
                            borg.Y = -110;
                            torpedo.Y = -110;
                        }
                        break;
                    case 2:
                        torpedo.X -= 5;
                        if (torpedoindex == borgindex && torpedo.X <= borg.X)
                        {
                            borg.X = -110;
                            torpedo.X = -110;
                        }
                        break;
                    case 3:
                        torpedo.Y += 5;
                        if (torpedoindex == borgindex && torpedo.Y >= borg.Y)
                        {
                            borg.Y = -110;
                            torpedo.Y = -110;
                        }
                        break;
                }

                if (torpedo.X > tubes[0].X + torpedodistance || torpedo.X < tubes[2].X - torpedodistance || torpedo.Y > tubes[3].Y + torpedodistance || torpedo.Y < tubes[1].Y - torpedodistance)
                {
                    fired = false;
                    explosion = new Rectangle(eX, eY, torpedo.Width, torpedo.Height);
                    torpedo.X = -110;
                    torpedo.Y = -110;
                }
            }

            /*
             * ENEMY TORPEDO BEHAVIOR
             */
            if (!canspawn)
            {
                switch (borgindex)
                {
                    case 0:
                        enemytorpedo.X -= 5;
                        if (enemytorpedo.X <= turret.X)
                        {
                            enemytorpedo.X = -110;
                        }
                        break;
                    case 1:
                        enemytorpedo.Y += 5;
                        if (enemytorpedo.Y >= turret.Y)
                        {
                            enemytorpedo.Y = -110;
                        }
                        break;
                    case 2:
                        enemytorpedo.X += 5;
                        if (enemytorpedo.X >= turret.X)
                        {
                            enemytorpedo.X = -110;
                        }
                        break;
                    case 3:
                        enemytorpedo.Y -= 5;
                        if (enemytorpedo.Y <= turret.Y)
                        {
                            enemytorpedo.Y = -110; 
                        }
                        break;
                }

                if (enemytorpedo.X <= -110 || enemytorpedo.Y <= -110)
                {
                    canspawn = true;
                }
            }

            /*
             * PHASER BEHAVIOR
             */
            if (phasfired)
            {
                phastimer++;

                if (phasindex == borgindex)
                {
                    borg.X = -110;
                    borg.Y = -110;
                }

                if (phastimer >= 60)
                {
                    phasfired = false;
                    phaser = new Rectangle(-110, -110, 100, 100);
                }
            }

            /*
             * BORG BEHAVIOR
             */
            if (borg.X <= -110 || borg.Y <= -110)
            {
                if (canspawn && random.Next(50) < 1)
                {
                    borgtimer = 0;
                    borgindex = random.Next(4);
                    switch (borgindex)
                    {
                        case 0:
                            borg.X = random.Next(200) + tubes[borgindex].X;
                            borg.Y = tubes[borgindex].Y;
                            break;
                        case 1:
                            borg.X = tubes[borgindex].X;
                            borg.Y = random.Next(tubes[borgindex].Y);
                            break;
                        case 2:
                            borg.X = random.Next(tubes[borgindex].X);
                            borg.Y = tubes[borgindex].Y;
                            break;
                        case 3:
                            borg.X = tubes[borgindex].X;
                            borg.Y = random.Next(200) + tubes[borgindex].Y;
                            break;
                    }
                }
            }
            else
            {
                if (!enemyfired && random.Next(50) < 1)
                {
                    enemyfired = true;
                    canspawn = false;
                    enemytorpedo.X = borg.X;
                    enemytorpedo.Y = borg.Y;
                    enemytorpedoS.Play();
                }

                borgtimer++;
                if (enemyfired && borgtimer >= 120 && random.Next(25) < 1)
                {
                    borg.X = -110;
                    borg.Y = -110;
                    enemyfired = false;
                    borgtimer = 0;
                }
            }

            /*
             * ACCEPTING INPUT
             */
            /*if (kb.IsKeyDown(Keys.Right) && oldkb.IsKeyUp(Keys.Right))
                selected = 0;
            else if (kb.IsKeyDown(Keys.Up) && oldkb.IsKeyUp(Keys.Up))
                selected = 1;
            else if (kb.IsKeyDown(Keys.Left) && oldkb.IsKeyUp(Keys.Left))
                selected = 2;
            else if (kb.IsKeyDown(Keys.Down) && oldkb.IsKeyUp(Keys.Down))
                selected = 3;*/
            /*bool line1 = mouse.Y < GraphicsDevice.Viewport.Height * mouse.X / GraphicsDevice.Viewport.Width;
            bool line2 = mouse.Y < (-1 * GraphicsDevice.Viewport.Height * mouse.X / GraphicsDevice.Viewport.Width) + GraphicsDevice.Viewport.Height;
            if (line1 && !line2)
                selected = 0;
            else if (line1 && line2)
                selected = 1;
            else if (!line1 && line2)
                selected = 2;
            else
                selected = 3;*/
            if (pad.DPad.Right == ButtonState.Pressed && oldpad.DPad.Right == ButtonState.Released)
            {
                selected = 0;
                factor = (GraphicsDevice.Viewport.Width - tubes[0].X) / 9.0;
            }
            else if (pad.DPad.Up == ButtonState.Pressed && oldpad.DPad.Up == ButtonState.Released)
            {
                selected = 1;
                factor = (tubes[1].Y) / 9.0;
            }
            else if (pad.DPad.Left == ButtonState.Pressed && oldpad.DPad.Left == ButtonState.Released)
            {
                selected = 2;
                factor = (tubes[2].X) / 9.0;
            }
            else if (pad.DPad.Down == ButtonState.Pressed && oldpad.DPad.Down == ButtonState.Released)
            {
                selected = 3;
                factor = (GraphicsDevice.Viewport.Height - tubes[3].Y) / 9.0;
            }

            // is there a better way to do this, i don't know
            /*if (kb.IsKeyDown(Keys.NumPad0) && oldkb.IsKeyUp(Keys.NumPad0))
                power = 0;
            if (kb.IsKeyDown(Keys.NumPad1) && oldkb.IsKeyUp(Keys.NumPad1))
                power = 1;
            if (kb.IsKeyDown(Keys.NumPad2) && oldkb.IsKeyUp(Keys.NumPad2))
                power = 2;
            if (kb.IsKeyDown(Keys.NumPad3) && oldkb.IsKeyUp(Keys.NumPad3))
                power = 3;
            if (kb.IsKeyDown(Keys.NumPad4) && oldkb.IsKeyUp(Keys.NumPad4))
                power = 4;
            if (kb.IsKeyDown(Keys.NumPad5) && oldkb.IsKeyUp(Keys.NumPad5))
                power = 5;
            if (kb.IsKeyDown(Keys.NumPad6) && oldkb.IsKeyUp(Keys.NumPad6))
                power = 6;
            if (kb.IsKeyDown(Keys.NumPad7) && oldkb.IsKeyUp(Keys.NumPad7))
                power = 7;
            if (kb.IsKeyDown(Keys.NumPad8) && oldkb.IsKeyUp(Keys.NumPad8))
                power = 8;
            if (kb.IsKeyDown(Keys.NumPad9) && oldkb.IsKeyUp(Keys.NumPad9))
                power = 9;

            if (kb.IsKeyDown(Keys.D1) && oldkb.IsKeyUp(Keys.D1))
                propulsion = 1;
            if (kb.IsKeyDown(Keys.D2) && oldkb.IsKeyUp(Keys.D2))
                propulsion = 2;
            if (kb.IsKeyDown(Keys.D3) && oldkb.IsKeyUp(Keys.D3))
                propulsion = 3;
            if (kb.IsKeyDown(Keys.D4) && oldkb.IsKeyUp(Keys.D4))
                propulsion = 4;
            if (kb.IsKeyDown(Keys.D5) && oldkb.IsKeyUp(Keys.D5))
                propulsion = 5;
            if (kb.IsKeyDown(Keys.D6) && oldkb.IsKeyUp(Keys.D6))
                propulsion = 6;
            if (kb.IsKeyDown(Keys.D7) && oldkb.IsKeyUp(Keys.D7))
                propulsion = 7;
            if (kb.IsKeyDown(Keys.D8) && oldkb.IsKeyUp(Keys.D8))
                propulsion = 8;
            if (kb.IsKeyDown(Keys.D9) && oldkb.IsKeyUp(Keys.D9))
                propulsion = 9;*/
            if (pad.ThumbSticks.Left.X >= 0)
                power = (int)(pad.ThumbSticks.Left.X * 9);
            if (pad.ThumbSticks.Left.Y >= 0)
                propulsion = (int)(pad.ThumbSticks.Left.Y * 8) + 1;

            if (pad.Buttons.X == ButtonState.Pressed && oldpad.Buttons.X == ButtonState.Released)
                phaspower = 1;
            if (pad.Buttons.A == ButtonState.Pressed && oldpad.Buttons.A == ButtonState.Released)
                phaspower = 2;
            if (pad.Buttons.Y == ButtonState.Pressed && oldpad.Buttons.Y == ButtonState.Released)
                phaspower = 3;
            if (pad.Buttons.B == ButtonState.Pressed && oldpad.Buttons.B == ButtonState.Released)
                phaspower = 4;

            /*
             * FIRING BEHAVIOR
             */
            //if (mouse.LeftButton == ButtonState.Pressed && oldmouse.LeftButton == ButtonState.Released && !fired && energy >= power)
            if (pad.Triggers.Left >= 0.5 && oldpad.Triggers.Left < 0.5 && !fired && energy >= power)
            {
                fired = true;
                torpedoindex = selected;
                energy -= power + propulsion;
                torpedodistance = (int)(propulsion * factor);
                torpedo = new Rectangle(tubes[selected].X, tubes[selected].Y, 10 + (power*10), 10 + (power*10));
                torpedoS.Play();
            }
            if (pad.Triggers.Right >= 0.5 && oldpad.Triggers.Right < 0.5 && !phasfired && energy >= phaspower*5)
            {
                phasfired = true;
                phasindex = selected;
                phascolor = phaspower;
                switch (selected)
                {
                    case 0:
                        phaser = new Rectangle(tubes[selected].X, tubes[selected].Y, GraphicsDevice.Viewport.Width - tubes[selected].X, 25);
                        break;
                    case 1:
                        phaser = new Rectangle(tubes[selected].X, 0, 25, tubes[selected].Y);
                        break;
                    case 2:
                        phaser = new Rectangle(0, tubes[selected].Y, tubes[selected].X, 25);
                        break;
                    case 3:
                        phaser = new Rectangle(tubes[selected].X, tubes[selected].Y, 25, GraphicsDevice.Viewport.Height - tubes[selected].Y);
                        break;
                }
                energy -= phaspower*5;
                phastimer = 0;
                phaserS.Play();
            }

            //display = "" + energy;    
            chargebar = new Rectangle(20, 20, energy, 10);

            oldkb = kb;
            oldmouse = mouse;
            oldpad = pad;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            sb.Begin();
            sb.Draw(explosionT, explosion, Color.White);
            sb.Draw(turretT, turret, Color.White);
            for (int i = 0; i < tubes.Length; i++)
            {
                if (i == selected && !fired && energy >= power)
                    sb.Draw(tubesT[i], tubes[i], Color.LightGreen);
                else if (fired || energy < power)
                    sb.Draw(tubesT[i], tubes[i], Color.Red);
                else
                    sb.Draw(tubesT[i], tubes[i], Color.White);
            }
            sb.Draw(borgT, borg, Color.White);
            sb.Draw(white, phaser, colors[phascolor-1]);
            sb.Draw(torpedoT, torpedo, Color.White);
            sb.Draw(enemytorpedoT, enemytorpedo, Color.White);
            //sb.DrawString(sf, display, new Vector2(0, 0), Color.White);

            sb.Draw(white, chargemask, Color.White);
            sb.Draw(white, explosivemask, Color.White);
            sb.Draw(white, propulsivemask, Color.White);

            sb.Draw(white, chargebar, Color.LightBlue);
            for (int e = 0; e < power; e++)
                sb.Draw(white, explosivebar[e], Color.LightGreen);
            for (int p = 0; p < propulsion; p++)
                sb.Draw(white, propulsivebar[p], Color.Orange);

            sb.End();

            base.Draw(gameTime);
        }
    }
}
