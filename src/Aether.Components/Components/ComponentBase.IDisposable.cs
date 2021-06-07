using System;

namespace tainicom.ProtonType.ContentLib.Components
{
    public partial class ComponentBase : IDisposable
    {
        private bool _isDisposed = false;
        public bool IsDisposed { get { return _isDisposed; } }


        ~ComponentBase()
        {
            Dispose(false);
        }
                
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                //Dispose unmanaged resources here
            }
            //Dispose managed resources here

            _isDisposed = true;
        }
    }
}
