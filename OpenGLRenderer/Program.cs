using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

class Program
{
    static void Main()
    {
        var nativeSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800, 600),
            Title = "OpenTK Rotating Triangle",
        };

        using (var window = new Game(nativeSettings))
        {
            window.Run();
        }
    }
}

class Game : GameWindow
{
    private readonly float[] _vertices =
    {
        // positions         // colors
         0.0f,  0.5f, 0.0f,  1f, 0f, 0f, // top, red
         0.5f, -0.5f, 0.0f,  0f, 1f, 0f, // right, green
        -0.5f, -0.5f, 0.0f,  0f, 0f, 1f, // left, blue
    };

    private int _vertexBuffer;
    private int _vertexArray;
    private int _shaderProgram;
    private float _rotation;

    public Game(NativeWindowSettings settings) : base(GameWindowSettings.Default, settings) { }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        // Create VAO
        _vertexArray = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArray);

        // Create VBO
        _vertexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        // Vertex position
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // Vertex color
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // Compile shaders
        var vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec3 aColor;
            uniform float rotation;
            out vec3 vertexColor;
            void main()
            {
                float cosR = cos(rotation);
                float sinR = sin(rotation);
                mat3 rot = mat3(
                    cosR, sinR, 0,
                   -sinR, cosR, 0,
                     0,    0,   1
                );
                gl_Position = vec4(rot * aPosition, 1.0);
                vertexColor = aColor;
            }
        ";

        var fragmentShaderSource = @"
            #version 330 core
            in vec3 vertexColor;
            out vec4 FragColor;
            void main()
            {
                FragColor = vec4(vertexColor, 1.0);
            }
        ";

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);
        CheckShader(vertexShader);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);
        CheckShader(fragmentShader);

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);
        GL.ValidateProgram(_shaderProgram);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        _rotation += (float)args.Time; // rotate over time

        GL.UseProgram(_shaderProgram);
        int rotationLocation = GL.GetUniformLocation(_shaderProgram, "rotation");
        GL.Uniform1(rotationLocation, _rotation);

        GL.BindVertexArray(_vertexArray);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        SwapBuffers();
    }

    private void CheckShader(int shader)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string info = GL.GetShaderInfoLog(shader);
            System.Console.WriteLine(info);
        }
    }
}
