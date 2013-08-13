using System;
using System.Windows.Forms;
using Psy.Core;
using Psy.Core.Console;
using Psy.Core.Input;
using Psy.Core.Logging;
using Psy.Core.Tasks;
using Psy.Gui;
using Psy.Gui.ColourScheme;
using Psy.Gui.Renderer;
using Psy.Windows;
using SlimMath;
using Vortex.Client.Renderer;
using Vortex.Interface;
using Vortex.PerformanceHud;
using Vortex.Renderer;
using Vortex.Renderer.BulletTracer;
using Vortex.Renderer.Camera;

namespace Vortex.Client
{
    class EngineWindow : Window
    {
        public EngineWindow(StartArguments startArguments, 
            IClientConfiguration clientConfiguration, 
            WindowAttributes windowAttributes) : base(windowAttributes)
        {
            _startArguments = startArguments;
            _clientConfiguration = clientConfiguration;
        }

        /**
         *  Debug data renderers
         */
        private PositionRenderer _positionRenderer;
        private PerformanceChartRenderer _performanceChartRenderer;
        private FrameNumberRenderer _frameNumberRenderer;
        private LatencyRenderer _latencyRenderer;

        private readonly StartArguments _startArguments;
        private readonly IClientConfiguration _clientConfiguration;
        public BulletRenderer BulletRenderer;
        public ConsoleRenderer Console;
        private ConsoleLogger _consoleLogger;
        private Client _engine;
        private long _gcAfter;
        private long _gcBefore;
        public GuiManager Gui;
        private GuiRenderer _guiRenderer;
        private TimeLine _memoryTimeLine;
        private bool _running;
        public SplashScreen SplashScreen;
        private TimeLine _updateLoopTimeLine;
        public Vortex.Renderer.View View;

        public long LastUpdateMemoryAlloc
        {
            get { return _gcBefore - _gcAfter; }
        }

        private void CreatePerformanceCharts()
        {
            _updateLoopTimeLine = new TimeLine(320, "Simulation time", 33);
            _memoryTimeLine = new TimeLine(320, "Used Memory", 300 * 1024 * 1024);

            _performanceChartRenderer = new PerformanceChartRenderer(GraphicsContext, 
                _updateLoopTimeLine, _memoryTimeLine, _engine.InboundKbpsTimeLine, 
                _engine.OutboundKbpsTimeLine, _engine.NetPerformanceBarChart);

            _latencyRenderer = new LatencyRenderer(GraphicsContext, _engine.LatencyTimeline);
        }

        private void CreateConsole()
        {
            Console = new ConsoleRenderer(GraphicsContext, StaticConsole.Console);

            var debugMode = System.Configuration.ConfigurationManager.AppSettings["Debug"] == "true";
            _consoleLogger = new ConsoleLogger { LoggerLevel = LoggerLevel.Warning };

            if (debugMode)
            {
                Logger.Write("-- Debug mode --");
            }

            Logger.Add(_consoleLogger);
            StandardCommands.Attach();
        }

        protected override void OnInitialize()
        {
            CreateConsole();

            Gui = new GuiManager(GraphicsContext.WindowSize);
            SplashScreen = new SplashScreen(GraphicsContext);

            _running = true;
            _engine = new Client(_clientConfiguration, this, _startArguments);

            View = new Vortex.Renderer.View(GraphicsContext, _engine.MaterialCache);
            DevicePostReset += () => View.SetPerspectiveMatrix();
            
            StaticTaskQueue.TaskQueue.CreateRepeatingTask("EngineWindow.Update", Update, 30);

            StaticConsole.Console.AddLine("Vortex " + EngineBase<IGameClient>.Version, new Color4(1.0f, 1.0f, 0.1f, 0.0f));
            Console.Visible = false;

            _engine.AttachModule();

            BulletRenderer = new BulletRenderer(GraphicsContext);
            _guiRenderer = new GuiRenderer(GraphicsContext, new Faceless());

            _positionRenderer = new PositionRenderer(GraphicsContext, View);
            _frameNumberRenderer = new FrameNumberRenderer(GraphicsContext, _engine);

            CreatePerformanceCharts();
        }

        public override void Dispose()
        {
            if (_engine != null) _engine.Dispose();
            if (_performanceChartRenderer != null) _performanceChartRenderer.Dispose();
            if (BulletRenderer != null) BulletRenderer.Dispose();
            if (_positionRenderer != null) _positionRenderer.Dispose();
            if (_frameNumberRenderer != null) _frameNumberRenderer.Dispose();
            if (_latencyRenderer != null) _latencyRenderer.Dispose();

            base.Dispose();
        }

        protected override void OnResourceLoad()
        {
            _running = true;
        }

        protected override void OnResourceUnload()
        {
            View.Dispose();
        }

        protected override void OnRenderBegin()
        {
            base.OnRenderBegin();
            if (_running)
                View.InitializeRenderState();
            AlphaBlending(true);
        }

        protected override void OnRender()
        {
            base.OnRender();

            if (_running)
            {
                if (_engine.World != null)
                {
                    View.Render();
                    BulletRenderer.Render(_engine.World.Bullets);
                }
            }

            SplashScreen.Render();
            
            _guiRenderer.Render(Gui);

            _performanceChartRenderer.Render();
            _positionRenderer.Render();
            _frameNumberRenderer.Render();
            _latencyRenderer.Render();
            Console.Render();
        }
        
        protected override void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            base.OnKeyPress(sender, e);
            switch (e.KeyChar)
            {
                case '`':
                    Console.Visible = !Console.Visible;
                    break;
                default:
                    if (Console.Visible)
                    {
                        StaticConsole.Console.OnKeyPress(new KeyPressEventArguments(e));
                    }
                    else
                    {
                        Gui.HandleKeyText(e.KeyChar);
                    }
                    break;
            }
        }
        

        protected override void OnKeyDown(object sender, KeyEventArgs e)
        {
            base.OnKeyDown(sender, e);

            var key = InputParser.KeyPress(e.KeyCode);

            if (Console.Visible)
            {
                StaticConsole.Console.OnKeyDown(key);
                return;
            }

            _engine.InputBinder.OnKeyDown(key);
        }

        protected override void OnKeyUp(object sender, KeyEventArgs e)
        {
            base.OnKeyUp(sender, e);
            if (Console.Visible)
                return;

            var key = InputParser.KeyPress(e.KeyCode);
            _engine.InputBinder.OnKeyUp(key);
        }

        protected override void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (Gui.HandleMouseMove(new Vector2(args.X, args.Y)))
                return;

            // translate to map coordinates.
            var viewCoords = BasicCamera.ScreenToWorldCoordinate(GraphicsContext, args.X, args.Y);
            _engine.OnMapMouseMove(viewCoords);
        }

        protected override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (Gui.HandleMouseDown(new Vector2(e.X, e.Y), MouseEventUtility.TranslateMouseButton(e)))
                return;

            var key = InputParser.MouseButton(e.Button);
            _engine.InputBinder.OnKeyDown(key);
        }

        protected override void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (Gui.HandleMouseUp(new Vector2(e.X, e.Y), MouseEventUtility.TranslateMouseButton(e)))
                return;

            var key = InputParser.MouseButton(e.Button);
            _engine.InputBinder.OnKeyUp(key);
        }

        protected override void OnMouseWheel(object sender, MouseEventArgs e)
        {
            var key = InputParser.MouseWheel(e.Delta);
            _engine.InputBinder.OnKeyDown(key);
        }

        private void Update()
        {
            _gcBefore = GC.GetTotalMemory(false);

            var ms = Psy.Core.Timer.GetTime();

            if (!_running)
                Quit();

            SplashScreen.Update();
            View.Update();

            _gcAfter = GC.GetTotalMemory(false);

            _updateLoopTimeLine.AddSample((Psy.Core.Timer.GetTime() - ms) / 1000.0);
            _memoryTimeLine.AddSample(Math.Max(0, _gcAfter));
        }
    }
}
