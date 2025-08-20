using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

class Node
{
    public Vector2 Position;
    public string Label;
    public Node(Vector2 pos, string label) { Position = pos; Label = label; }
    public bool Contains(Vector2 point)
    {
        var size = new Vector2(100, 50);
        return point.X >= Position.X && point.X <= Position.X + size.X &&
               point.Y >= Position.Y && point.Y <= Position.Y + size.Y;
    }
}

class Link
{
    public Node From;
    public Node To;
    public Link(Node from, Node to) { From = from; To = to; }
}

class Program : GameWindow
{
    private List<Node> _nodes = new List<Node>();
    private List<Link> _links = new List<Link>();
    private Node _draggingNode = null;
    private Node _linkStart = null;
    private Vector2 _lastMouse;

    private int _shaderProgram;
    private int _vao;
    private int _vbo;

    private float[] _quadVertices = {
        0f, 0f,
        1f, 0f,
        1f, 1f,
        0f, 1f
    };

    public Program() : base(GameWindowSettings.Default, new NativeWindowSettings
    {
        ClientSize = new Vector2i(1280, 720),
        Title = "Node Editor OpenGL4"
    })
    { }

    protected override void OnLoad()
    {
        GL.ClearColor(0.15f, 0.15f, 0.18f, 1.0f);

        _nodes.Add(new Node(new Vector2(200, 200), "A"));
        _nodes.Add(new Node(new Vector2(400, 300), "B"));

        // Create VAO and VBO for a unit quad
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _quadVertices.Length * sizeof(float), _quadVertices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

        GL.BindVertexArray(0);

        // Simple shader
        string vertexShaderSource = @"#version 330 core
        layout(location = 0) in vec2 aPos;
        uniform vec2 uPos;
        uniform vec2 uSize;
        uniform vec2 uResolution;
        void main(){
            vec2 pos = (aPos * uSize + uPos);
            vec2 ndc = (pos / uResolution) * 2.0 - 1.0;
            gl_Position = vec4(ndc.x, ndc.y, 0.0, 1.0);
        }";

        string fragmentShaderSource = @"#version 330 core
            out vec4 FragColor;
            uniform vec3 uColor;
            void main(){
                FragColor = vec4(uColor, 1.0);
            }";

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        var mouse = MousePosition;
        var mouseScreen = new Vector2(mouse.X, Size.Y - mouse.Y);

        if (MouseState.IsButtonPressed(MouseButton.Left))
        {
            foreach (var node in _nodes)
            {
                if (node.Contains(mouseScreen))
                {
                    _draggingNode = node;
                    break;
                }
            }
        }

        if (MouseState.IsButtonDown(MouseButton.Left) && _draggingNode != null)
        {
            var delta = mouseScreen - _lastMouse;
            _draggingNode.Position += delta;
        }

        if (MouseState.IsButtonReleased(MouseButton.Left))
        {
            _draggingNode = null;
        }

        if (MouseState.IsButtonPressed(MouseButton.Right))
        {
            foreach (var node in _nodes)
            {
                if (node.Contains(mouseScreen))
                {
                    _linkStart = node;
                    break;
                }
            }
        }

        if (MouseState.IsButtonReleased(MouseButton.Right) && _linkStart != null)
        {
            foreach (var node in _nodes)
            {
                if (node.Contains(mouseScreen) && node != _linkStart)
                {
                    _links.Add(new Link(_linkStart, node));
                    break;
                }
            }
            _linkStart = null;
        }

        if (KeyboardState.IsKeyPressed(Keys.N))
        {
            _nodes.Add(new Node(mouseScreen, $"Node{_nodes.Count + 1}"));
        }

        _lastMouse = mouseScreen;
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vao);

        int uPos = GL.GetUniformLocation(_shaderProgram, "uPos");
        int uSize = GL.GetUniformLocation(_shaderProgram, "uSize");
        int uRes = GL.GetUniformLocation(_shaderProgram, "uResolution");
        int uColor = GL.GetUniformLocation(_shaderProgram, "uColor");

        GL.Uniform2(uRes, (float)Size.X, (float)Size.Y);

        foreach (var node in _nodes)
        {
            GL.Uniform2(uPos, node.Position.X, node.Position.Y);
            GL.Uniform2(uSize, 100f, 50f);
            GL.Uniform3(uColor, 0.2f, 0.6f, 0.9f);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        GL.BindVertexArray(0);
        GL.UseProgram(0);

        SwapBuffers();
    }

    static void Main(string[] args)
    {
        using var win = new Program();
        win.Run();
    }
}