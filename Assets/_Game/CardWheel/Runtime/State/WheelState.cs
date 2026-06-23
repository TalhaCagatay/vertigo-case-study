namespace Vertigo.CardWheel.State
{
    // for this simple demo, we don't need a more complex state machine and transition logic
    public enum WheelState
    {
        Idle,
        Spinning,
        Result,
        GameOver
    }
}
