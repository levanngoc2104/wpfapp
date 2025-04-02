using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace WpfApp
{
    public class ModelRenderer : IDisposable
    {
        private ObjModel model;
        private int vertexBufferId; // ID của Vertex Buffer Object (VBO) cho đỉnh
        private int indexBufferId;  // ID của Index Buffer Object (IBO) cho chỉ số
        private int vertexCount;    // Số lượng đỉnh
        private int indexCount;     // Số lượng chỉ số

        public ModelRenderer()
        {
            vertexBufferId = 0;
            indexBufferId = 0;
        }

        public void LoadModel(string filePath)
        {
            try
            {
                model = new ObjModel();
                model.Load(filePath);
                SetBuffers(); // Thiết lập buffer ngay sau khi tải model
                Console.WriteLine($"Model loaded: {filePath}, Vertices: {model.Vertices.Count}, Indices: {model.Indices.Count}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load model: {ex.Message}");
            }
        }

        private void SetBuffers()
        {
            if (model == null || model.Vertices.Count == 0 || model.Indices.Count == 0)
            {
                Console.WriteLine("No model data to buffer");
                return;
            }

            // Tạo và bind Vertex Buffer
            if (vertexBufferId == 0)
                GL.GenBuffers(1, out vertexBufferId);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          model.Vertices.Count * sizeof(float),
                          model.Vertices.ToArray(),
                          BufferUsageHint.StaticDraw);
            vertexCount = model.Vertices.Count / 3;

            // Tạo và bind Index Buffer
            if (indexBufferId == 0)
                GL.GenBuffers(1, out indexBufferId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                          model.Indices.Count * sizeof(int),
                          model.Indices.ToArray(),
                          BufferUsageHint.StaticDraw);
            indexCount = model.Indices.Count;

            // Unbind buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            var error = GL.GetError();
            if (error != ErrorCode.NoError)
                Console.WriteLine($"OpenGL Error in SetBuffers: {error}");
        }

        public void Render()
        {
            if (vertexBufferId == 0 || indexBufferId == 0 || indexCount == 0)
            {
                Console.WriteLine("No buffers to render");
                return;
            }

            // Bật các tính năng cần thiết
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            // Thiết lập ánh sáng
            GL.Light(LightName.Light0, LightParameter.Position, new Vector4(2.0f, 5.0f, 3.0f, 1.0f));
            GL.Light(LightName.Light0, LightParameter.Ambient, new Color4(0.2f, 0.2f, 0.2f, 1.0f));
            GL.Light(LightName.Light0, LightParameter.Diffuse, new Color4(1.0f, 1.0f, 1.0f, 1.0f));

            // Thiết lập trạng thái vertex array
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);

            // Bind index buffer và vẽ
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.Color3(1.0f, 0.5f, 0.2f); // Màu cam
            GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Tắt trạng thái
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            // Kiểm tra lỗi OpenGL
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
                Console.WriteLine($"OpenGL Error in Render: {error}");

            // Tắt các tính năng không cần thiết
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);
        }

        public void Dispose()
        {
            if (vertexBufferId != 0)
                GL.DeleteBuffers(1, ref vertexBufferId);
            if (indexBufferId != 0)
                GL.DeleteBuffers(1, ref indexBufferId);
        }
    }

    public class ObjModel
    {
        public List<float> Vertices { get; set; } = new List<float>();
        public List<int> Indices { get; set; } = new List<int>();

        public void Load(string filePath)
        {
            var tempVertices = new List<Vector3>();
            Vertices.Clear();
            Indices.Clear();

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
                                float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                                float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                                float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture)));
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
                throw new Exception($"Error loading OBJ file: {ex.Message}");
            }
        }
    }
}