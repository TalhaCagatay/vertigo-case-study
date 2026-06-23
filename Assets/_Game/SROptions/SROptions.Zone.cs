using System.ComponentModel;
using _Game;
using Vertigo.CardWheel.Controller;

public partial class SROptions
{
    [Category("Zone")]
    public void AdvanceZone()
    {
        var cardWheelController = ControllerInstaller.Container.Resolve<CardWheelController>();
        cardWheelController.AdvanceZone();
    }
}