using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace BreadMayhem.Toaster.Rendering;


public class Panel
{
    public static Panel instance;
    // window settings
    private (uint, uint) _windowSize;
    private (uint, uint) _renderSize;

    // caps
    private readonly uint _fpsCap;
    private readonly float _dtCap;

    // init time handlers
    private readonly Queue<float> _frameLog;
    private readonly Clock _clock = new Clock();
    
    // dt and runtime
    private float _logSum;
    
    // declare the window
    public readonly RenderWindow Screen;
    public Color ClearColour;
    
    // properties
    public float Runtime { get; private set; }
    public float DeltaTime { get; private set; }
    public float Fps => this._fpsCap / this._logSum;

    public Panel((uint, uint) renderSize, (uint, uint) windowSize, Color clearColour, string caption="test_window", Styles flags=Styles.None, uint fpsCap=60, float dtCap=0.2f)
    {
        // set the window parameters
        _renderSize = renderSize;
        _windowSize = windowSize;
        
        // set the dt/frame caps
        _fpsCap = fpsCap;
        _dtCap = dtCap;
        
        // init time variables
        Runtime = 0;
        DeltaTime = _dtCap;
        
        // populate frame log
        var invFpsCap = 1f / _fpsCap;
        _frameLog = new Queue<float>((int)_fpsCap);
        for (var i = 0; i < _fpsCap; ++i) _frameLog.Enqueue(invFpsCap);
        _logSum = invFpsCap * _fpsCap;
        
        // initialize the screen
        Screen = new RenderWindow(
            new VideoMode(windowSize.Item1, windowSize.Item2), 
            caption, flags | Styles.Titlebar | Styles.Close);
        
        // initialize the render view
        Screen.SetView(
            new View(new FloatRect(0, 0, renderSize.Item1, renderSize.Item2))
            );
        
        // set screen params
        Screen.SetFramerateLimit(_fpsCap);
        Screen.Closed += (sender, e) => { ((RenderWindow)sender).Close(); };
        ClearColour = clearColour;
        
        // restart the clock
        _clock.Restart();

        instance = this;
    }

    public void Update()
    {
        // render the screen
        Screen.Display();
        
        // calculate the delta time using clock
        DeltaTime = float.Min(_clock.ElapsedTime.AsSeconds(), _dtCap);

        // increment the runtime by the elapsed time
        Runtime += DeltaTime;
        
        // update the frame log
        _logSum -= _frameLog.Dequeue();
        _frameLog.Enqueue(DeltaTime);
        _logSum += DeltaTime;
        
        // restart the clock
        _clock.Restart(); 
        
        // clear the screen
        Screen.Clear(ClearColour);
        Screen.SetTitle($"{Fps}");
    }
}