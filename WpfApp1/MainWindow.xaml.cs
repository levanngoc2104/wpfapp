using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private int vbo, vao, ebo;
        private ObjModel model;
        private Matrix4 projection, view, modelMatrix;
        private int shaderProgram;
        private Vector3 lightPos = new Vector3(2.0f, 5.0f, 3.0f);

        public MainWindow()
        {
            InitializeComponent();
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 3,
                RenderContinuously = true
            };
            glControl.Start(settings);
        }

        private void GlControl_Ready()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            shaderProgram = CreateShaderProgram();

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f),
                (float)glControl.ActualWidth / (float)glControl.ActualHeight, 0.1f, 100.0f);
            view = Matrix4.LookAt(new Vector3(0, 2, 5), Vector3.Zero, Vector3.UnitY);
            modelMatrix = Matrix4.Identity;
        }

        private void CalcButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FlowFieldButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProjectButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenObjButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "OBJ Files (*.obj)|*.obj|All Files (*.*)|*.*" };
            if (dialog.ShowDialog() == true)
            {
                model = new ObjModel();
                model.Load(dialog.FileName);
                SetupBuffers();
                glControl.InvalidateVisual();
            }
        }

        private void GlControl_Render(TimeSpan delta)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (model != null)
            {
                GL.UseProgram(shaderProgram);
                GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);
                GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), false, ref view);
                GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref modelMatrix);
                GL.Uniform3(GL.GetUniformLocation(shaderProgram, "lightPos"), lightPos);
                GL.BindVertexArray(vao);
                GL.DrawElements(PrimitiveType.Triangles, model.Indices.Count, DrawElementsType.UnsignedInt, 0);
                GL.BindVertexArray(0);
            }
        }

        private void SetupBuffers()
        {
            if (model == null || model.Vertices.Count == 0 || model.Indices.Count == 0)
            {
                throw new Exception("Dữ liệu model không hợp lệ hoặc rỗng");
            }

            // Giải phóng buffer cũ nếu tồn tại
            if (vao != 0) GL.DeleteVertexArray(vao);
            if (vbo != 0) GL.DeleteBuffer(vbo);
            if (ebo != 0) GL.DeleteBuffer(ebo);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, model.Vertices.Count * sizeof(float), model.Vertices.ToArray(), BufferUsageHint.StaticDraw);

            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, model.Indices.Count * sizeof(int), model.Indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);

            int error = (int)GL.GetError();
            if (error != 0)
            {
                throw new Exception($"OpenGL Error in SetupBuffers: {error}");
            }
        }

        private int CreateShaderProgram()
        {
            string vertexShaderSource = @"#version 330 core
                layout(location = 0) in vec3 aPosition;
                out vec3 FragPos;
                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;
                void main() {
                    FragPos = vec3(model * vec4(aPosition, 1.0));
                    gl_Position = projection * view * model * vec4(aPosition, 1.0);
                }";

            string fragmentShaderSource = @"#version 330 core
                in vec3 FragPos;
                out vec4 FragColor;
                uniform vec3 lightPos;
                void main() {
                    vec3 lightColor = vec3(1.0, 1.0, 1.0);
                    vec3 objectColor = vec3(1.0, 0.5, 0.2);
                    float ambientStrength = 0.1;
                    vec3 ambient = ambientStrength * lightColor;
                    vec3 result = (ambient) * objectColor;
                    FragColor = vec4(result, 1.0);
                }";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0) MessageBox.Show($"Vertex Shader Error: {GL.GetShaderInfoLog(vertexShader)}");

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0) MessageBox.Show($"Fragment Shader Error: {GL.GetShaderInfoLog(fragmentShader)}");

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out success);
            if (success == 0) MessageBox.Show($"Shader Program Link Error: {GL.GetProgramInfoLog(program)}");

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            return program;
        }
    }

    public class ObjModel
    {
        public List<float> Vertices { get; set; } = new List<float>();
        public List<int> Indices { get; set; } = new List<int>();

        public void Load(string filePath)
        {
            var tempVertices = new List<Vector3>();
            Vertices.Clear(); // Xóa dữ liệu cũ
            Indices.Clear();  // Xóa dữ liệu cũ

            try
            {
                foreach (var line in File.ReadLines(filePath))
                {
                    if (line.StartsWith("v "))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 4)
                        {
                            tempVertices.Add(new Vector3(
                                float.Parse(parts[1]),
                                float.Parse(parts[2]),
                                float.Parse(parts[3])));
                        }
                    }
                    else if (line.StartsWith("f "))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        var faceIndices = new List<int>();
                        for (int i = 1; i < parts.Length; i++)
                        {
                            faceIndices.Add(int.Parse(parts[i].Split('/')[0]) - 1);
                        }
                        // Tam giác hóa: chuyển đa giác thành tam giác
                        for (int i = 1; i < faceIndices.Count - 1; i++)
                        {
                            Indices.Add(faceIndices[0]);
                            Indices.Add(faceIndices[i]);
                            Indices.Add(faceIndices[i + 1]);
                        }
                    }
                }
                foreach (var vertex in tempVertices)
                {
                    Vertices.AddRange(new float[] { vertex.X, vertex.Y, vertex.Z });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải file OBJ: {ex.Message}");
            }
        }
    }
}
