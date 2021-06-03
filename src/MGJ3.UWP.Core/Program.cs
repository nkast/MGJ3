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
            var factory = new MonoGame.Framework.GameFrameworkViewSource<MGJ3Game>();
            Windows.ApplicationModel.Core.CoreApplication.Run(factory);
        }
    }
}
