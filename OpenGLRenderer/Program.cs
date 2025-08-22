using OpenGLRenderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

class Game : GameWindow
{
    public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title }) { }

    public Shader? shader = null;
    protected override void OnLoad()
    {
        base.OnLoad();

        shader = new Shader("shader.vert", "shader.frag");

        GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);

        //Code goes here
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        //Code goes here.

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        shader!.Dispose();
    }

    static void Main(string[] args)
    {
        using Game game = new(800, 600, "LearnOpenTK");

        game.Run();
    }
}