using UnityEngine;

public sealed class HighlightDecorator : VisualDecorator
{
    private readonly Material highlightMaterial;

    public HighlightDecorator(
        IPickupVisual wrappedVisual,
        Material highlightMaterial)
        : base(wrappedVisual)
    {
        this.highlightMaterial = highlightMaterial;
    }

    public override void Apply(Material material)
    {
        wrappedVisual.Apply(highlightMaterial);
    }
}
