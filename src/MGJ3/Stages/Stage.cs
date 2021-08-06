using System;
using System.IO;
using MGJ3.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using tainicom.Aether.Core.Serialization;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Cameras;
using tainicom.Aether.Elementary.Visual;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.Maths;
using tainicom.Aether.MonoGame;
using tainicom.Aether.Physics2D.Components;
using nkast.Aether.Physics2D.Diagnostics;


namespace MGJ3.Stages
{
    abstract class Stage
    {
        string _stageFilename;
        ContentManager _content;
        AetherContextMG _context;
        public AetherEngine Engine { get { return engine; } }
        public StageBounds StageBounds { get { return (StageBounds)engine["StageBounds1"]; } }
        public StageFinish StageFinish1 { get { return (StageFinish)engine["StageFinish1"]; } }
        public Player Player1 { get { return (Player)engine["Player1"]; } }
        public Physics2dPlane PhysicsPlane0 { get { return (Physics2dPlane)engine["Physics2dPlane0"]; } }

        protected AetherEngine engine;
        protected Stream stream;

        private DebugView _debugView;


        public Stage(Game game, string stageFilename)
        {
            this._stageFilename = stageFilename;
            _content = new ContentManager(game.Services, "Content");
            _context = new AetherContextMG(game.Services, _content);
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
                #if UWP
                var assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(IntroPage)).Assembly;
                stream = assembly.GetManifestResourceStream("Chervil.Stages.HardHatZone.aebin");
                #else
                stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("MGJ3.Stages."+_stageFilename);
			    #endif
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

                _debugView = new DebugView(PhysicsPlane0.World);
                _debugView.LoadContent(_context.Device, _content);
                _debugView.Flags = _debugView.Flags | DebugViewFlags.PerformanceGraph;

                isTableLoaded = true;
            }
            
            return true;
        }

        internal void UpdateCamera()
        {
            ICamera camera = engine["DefaultCamera"] as ICamera;

            IVisualWalker viewProj = Engine.VisualMgr.DefaultWalker;
            viewProj.Projection = camera.Projection;
            viewProj.View = camera.View;



        }

        internal void DebugDraw()
        {
            ICamera camera = engine["DefaultCamera"] as ICamera;

            _debugView.RenderDebugData(camera.Projection, camera.View, Matrix.Identity);
        }

    }
}
