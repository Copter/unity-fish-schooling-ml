using UnityEngine;

public struct NeighborFish {
    public NeighborFish(FishSFAgent fishComponent) {
        FishComponent = fishComponent;
    }
    public FishSFAgent FishComponent { get; }
    public Vector2 GetRelativePos(Transform transform) {
        return transform.InverseTransformPointUnscaled(FishComponent.transform.position);
    }
    public Vector2 Velocity(Transform transform) {
        return transform.InverseTransformVector(FishComponent.rb.velocity);
    }
}

