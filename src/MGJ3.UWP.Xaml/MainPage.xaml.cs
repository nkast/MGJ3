﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MGJ3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        readonly MGJ3Game _game;

        public MainPage()
        {
            this.InitializeComponent();

            // Create the game.
            string launchArguments = String.Empty;
            _game = Microsoft.Xna.Platform.XamlGame<MGJ3Game>.Create(launchArguments, Window.Current.CoreWindow, swapChainPanel);
        }
    }
}
