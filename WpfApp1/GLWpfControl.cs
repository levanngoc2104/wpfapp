using OpenTK.Graphics.OpenGL4;
using System;

namespace WpfApp
{
    public class GLWpfControl : GLWpfCanvas
    {
        public GLWpfControl()
        {
            Ready += OnReady;   // Khi control sẵn sàng
            Render += OnRender; // Render mỗi frame
        }

        public Action<object, EventArgs> Ready { get; }
        public Action<TimeSpan> Render { get; }

     

        private void OnReady(object sender, EventArgs e)
        {
            // Cấu hình OpenGL
            GL.ClearColor(0f, 0f, 0f, 1f); // Đặt màu nền thành đen
        }

        private void OnRender(TimeSpan delta)
        {
            // Xóa màn hình trước khi vẽ
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
    }
}
