using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public Gun gun;
    public Transform gunHolder;

    private bool isHoldingShoot;
    private ICommand shootCommand;
    private ICommand reloadCommand;
    private ICommand dropCommand;

    private void Awake()
    {
        ConfigureCommands();
    }

    private void ConfigureCommands()
    {
        if (gun == null)
        {
            shootCommand = null;
            reloadCommand = null;
            dropCommand = null;
            return;
        }

        shootCommand = new ShootCommand(gun);
        reloadCommand = new ReloadCommand(gun);
        dropCommand = new DropCommand(gun);
    }

    public void SetGun(Gun newGun)
    {
        gun = newGun;
        ConfigureCommands();
    }

    void OnShoot()
    {
        isHoldingShoot = true;
    }

    void OnShootRelease()
    {
        isHoldingShoot = false;
    }
    void OnReload()
    {
        reloadCommand?.Execute();
    }

    void Update()
    {
        if (isHoldingShoot)
        {
            shootCommand?.Execute();
        }
    }

    public void OnDrop()
    {
        if (dropCommand == null)
        {
            return;
        }

        dropCommand.Execute();
        SetGun(null);
    }
}
