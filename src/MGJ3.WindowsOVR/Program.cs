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
        [STAThread]
        static void Main()
        {
            Microsoft.Xna.Platform.XR.XRFactory.RegisterXRFactory(new Microsoft.Xna.Platform.XR.LibOVR.ConcreteXRFactory());
            using (var game = new MGJ3Game())
                game.Run();
        }
    }
}
