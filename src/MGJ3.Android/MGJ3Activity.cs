using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace MGJ3
{
    [Activity(Label = "MGJ3"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.FullSensor
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.UiMode | ConfigChanges.SmallestScreenSize)]
    public class MGJ3Activity : Microsoft.Xna.Framework.AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var game = new MGJ3Game();
            game.Services.AddService<MGJ3Activity>(this);
            SetContentView((View)game.Services.GetService(typeof(View)));
            game.Run();
        }
    }
}

