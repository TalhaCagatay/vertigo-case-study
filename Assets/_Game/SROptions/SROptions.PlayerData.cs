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
        Application.Quit();
    }
}