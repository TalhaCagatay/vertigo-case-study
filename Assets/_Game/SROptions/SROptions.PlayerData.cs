using System.ComponentModel;
using _Game;
using UnityEngine;
using Vertigo.Player;

public partial class SROptions
{
    [Category("Data")]
    public async void DeleteAllData()
    {
        var playerController = ControllerInstaller.Container.Resolve<PlayerController>();
        await playerController.DeletePlayerData();
        RestartAndroid();
    }

    // Source - https://stackoverflow.com/a/70151431
    // Posted by Tamaya
    // Retrieved 2026-06-22, License - CC BY-SA 4.0
    private static void RestartAndroid()
    {
        if (Application.isEditor) return;

        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            const int kIntent_FLAG_ACTIVITY_CLEAR_TASK = 0x00008000;
            const int kIntent_FLAG_ACTIVITY_NEW_TASK   = 0x10000000;

            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var pm              = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            var intent          = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);

            intent.Call<AndroidJavaObject>("setFlags", kIntent_FLAG_ACTIVITY_NEW_TASK | kIntent_FLAG_ACTIVITY_CLEAR_TASK);
            currentActivity.Call("startActivity", intent);
            currentActivity.Call("finish");
            var process = new AndroidJavaClass("android.os.Process");
            int pid     = process.CallStatic<int>("myPid");
            process.CallStatic("killProcess", pid);
        }
    }

}