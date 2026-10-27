public sealed class DropCommand : ICommand
{
    private readonly Gun gun;

    public DropCommand(Gun gun)
    {
        this.gun = gun;
    }

    public void Execute()
    {
        gun.Drop();
    }
}
