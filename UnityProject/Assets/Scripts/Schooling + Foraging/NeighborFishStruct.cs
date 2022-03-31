using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NeighborFish {
    public NeighborFish(float posX, float posY, float velocityX, float velocitY, Transform fishTransform){
        PosX = posX;
        PosY = posY;
        VelocityX = velocityX;
        VelocitY = velocitY;
        FishTransform = fishTransform;
    }
    public float PosX{get;}
    public float PosY{get;}
    public Transform FishTransform{get;}
    public Vector2 GetPos(){
        return new Vector2(PosX,PosY);
    }
    public float VelocityX {get;}
    public float VelocitY {get;}
}

