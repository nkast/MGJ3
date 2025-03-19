using System;

namespace MGJ3
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Windows.ApplicationModel.Core.IFrameworkViewSource viewProviderFactory = new Microsoft.Xna.Platform.GameFrameworkViewSource<MGJ3Game>();
            Windows.ApplicationModel.Core.CoreApplication.Run(viewProviderFactory);
        }
    }
}
