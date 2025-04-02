using System;
using System.Windows;
using Microsoft.Win32;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private ModelRenderer modelRenderer;
        private int width, height;

        public MainWindow()
        {
            InitializeComponent();
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 1,
                MinorVersion = 1,
                RenderContinuously = true
            };
            glControl.Start(settings);
        }

        private void GlControl_Ready()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            modelRenderer = new ModelRenderer();
        }

        private void GlControl_Render(TimeSpan delta)
        {
            width = (int)(glControl.ActualWidth * 1.25);
            height = (int)(glControl.ActualHeight * 1.25);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, width, height);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f),
                (float)width / height, 0.1f, 100f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            float zoom = 100; // Tăng khoảng cách camera
            Vector3 eye = new Vector3(0, 0, zoom);
            Vector3 central = new Vector3(2, 2, 2);
            Matrix4 modelView = Matrix4.LookAt(eye, central, Vector3.UnitY);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);

            GL.Scale(0.5f, 0.5f, 0.5f); // Giảm kích thước model xuống 50%

            GL.Translate(central.X, central.Y, central.Z);
            GL.Rotate(1, 1.0f, 0.0f, 0.0f);
            GL.Rotate(1, 0.0f, 1.0f, 0.0f);
            GL.Translate(-central.X, -central.Y, -central.Z);

            modelRenderer?.Render();
        }

        private void OpenObjButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "OBJ Files (*.obj)|*.obj|All Files (*.*)|*.*",
                Title = "Select 3D Model File"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    modelRenderer.LoadModel(dialog.FileName);
                    glControl.InvalidateVisual();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading model: {ex.Message}",
                                    "Load Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        private void ProjectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenObjButton_Click(sender, e);
        }

        private void CalcButton_Click(object sender, RoutedEventArgs e) { }
        private void FlowFieldButton_Click(object sender, RoutedEventArgs e) { }
        private void SaveButton_Click(object sender, RoutedEventArgs e) { }

        protected override void OnClosed(EventArgs e)
        {
            modelRenderer?.Dispose();
            base.OnClosed(e);
        }
    }
}