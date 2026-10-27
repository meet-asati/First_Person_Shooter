using UnityEngine;
using UnityEngine.InputSystem;

public class PickUp : MonoBehaviour
{
    public Material highlightMaterial;
    private IPickupVisual pickupVisual;

    public GameObject weaponPrefab;
    public float lookRange = 3f;

    private bool isLookedAt = false;
    private Camera playerCam;
    private PlayerShooting player;

    void Start()
    {
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        IPickupVisual rendererVisual = new RendererVisual(meshRenderers);
        pickupVisual = new HighlightDecorator(rendererVisual, highlightMaterial);

        player = FindObjectOfType<PlayerShooting>();
        playerCam = player.GetComponentInChildren<Camera>();
    }

    void Update()
    {
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        if(Physics.Raycast(ray, out RaycastHit hit, lookRange))
        {
            if(hit.collider.GetComponentInParent<PickUp>() == this)
            {
                if(!isLookedAt) 
                    SetLookedAt(true);

                return;
            }
        }

        if(isLookedAt) 
            SetLookedAt(false);
    }

    void SetLookedAt(bool lookedAt)
    {
        isLookedAt = lookedAt;

        if(lookedAt)
        {
            pickupVisual.Apply(highlightMaterial);
        }
        else
        {
            pickupVisual.Restore();
        }
    }

    public void OnPickup()
    {
        if(!isLookedAt) return;

        player.OnDrop();

        GameObject newWeapon = Instantiate(weaponPrefab, player.gunHolder);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        player.SetGun(newWeapon.GetComponent<Gun>());

        Destroy(gameObject);
    }
}
