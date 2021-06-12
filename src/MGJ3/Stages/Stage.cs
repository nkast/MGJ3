﻿using System;
using System.IO;
using MGJ3.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using tainicom.Aether.Core.Serialization;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Cameras;
using tainicom.Aether.Elementary.Photons;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.Maths;
using tainicom.Aether.MonoGame;
using tainicom.Aether.Physics2D.Components;

namespace MGJ3.Stages
{
    abstract class Stage
    {
        string _stageFilename;
        ContentManager _content;
        AetherContextMG _context;
        public AetherEngine Engine { get { return engine; } }
        public Player Player1 { get { return (Player)engine["Player1"]; } }
        public Physics2dPlane PhysicsPlane0 { get { return (Physics2dPlane)engine["Physics2dPlane1"]; } }

        protected AetherEngine engine;
        protected Stream stream;


        public Stage(Game game, string stageFilename)
        {
            this._stageFilename = stageFilename;
            _content = new ContentManager(game.Services, "Content");
            _context = new AetherContextMG(game.GraphicsDevice, _content);
        }
        
        bool isTableLoaded = false;
        internal bool LoadStage()
        {
            //create engine
            if (engine == null)
            {
                engine = new AetherEngine(_context);
                return false;
            }
                        
            //create stream
            if (stream == null)
            {
                stream = TitleContainer.OpenStream(_stageFilename);
                if (stream == null) throw new Exception("Stage not found");
                return false;
            }

            if (!isTableLoaded)
            {
                IAetherReader aetherReader = null;
                aetherReader = new AetherXMLReader(engine, stream);
                aetherReader.TypeResolver = new DefaultTypeResolver();
                aetherReader.Read("EatherEngine", engine);
                aetherReader.Close();
                stream.Close();

                //initialize
                IAether[] initpart = new IAether[engine.Values.Count];
                engine.Values.CopyTo(initpart, 0);
                foreach (IAether particle in initpart)
                {
                    IInitializable initializable = particle as IInitializable;
                    if (initializable != null) initializable.Initialize(engine);
                }

                ICamera camera = engine["DefaultCamera"] as ICamera;

                isTableLoaded = true;
            }
            
            return true;
        }

        internal void UpdateCamera()
        {
            ICamera camera = engine["DefaultCamera"] as ICamera;

            IPhotonWalker viewProj = Engine.PhotonsMgr.DefaultWalker;
            viewProj.Projection = camera.Projection;
            viewProj.View = camera.View;



        }
    }
}
