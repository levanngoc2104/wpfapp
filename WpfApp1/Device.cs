namespace WpfApp
{
    internal class Device
    {
        private Direct3D direct3D;
        private int v;
        private object hardware;
        private nint zero;
        private object hardwareVertexProcessing;
        private PresentParameters presentParameters;

        public Device(Direct3D direct3D, int v, object hardware, nint zero, object hardwareVertexProcessing, PresentParameters presentParameters)
        {
            this.direct3D = direct3D;
            this.v = v;
            this.hardware = hardware;
            this.zero = zero;
            this.hardwareVertexProcessing = hardwareVertexProcessing;
            this.presentParameters = presentParameters;
        }

        internal System.IDisposable CreateRenderTarget(int actualWidth, int actualHeight, object a8R8G8B8, object none, int v1, bool v2)
        {
            throw new System.NotImplementedException();
        }
    }
}