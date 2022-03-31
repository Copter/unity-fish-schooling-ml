using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NeighborFish {
    public NeighborFish(float posX, float posY, float velocityX, float velocitY){
        PosX = posX;
        PosY = posY;
        VelocityX = velocityX;
        VelocitY = velocitY;
    }
    public float PosX{get;}
    public float PosY{get;}
    public Vector2 GetPos(){
        return new Vector2(PosX,PosY);
    }
    public float VelocityX {get;}
    public float VelocitY {get;}
}

