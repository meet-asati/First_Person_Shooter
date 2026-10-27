using UnityEngine;

public sealed class RendererVisual : IPickupVisual
{
    private readonly MeshRenderer[] renderers;
    private readonly Material[] originalMaterials;

    public RendererVisual(MeshRenderer[] renderers)
    {
        this.renderers = renderers;
        originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
        }
    }

    public void Apply(Material material)
    {
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.material = material;
        }
    }

    public void Restore()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = originalMaterials[i];
        }
    }
}
