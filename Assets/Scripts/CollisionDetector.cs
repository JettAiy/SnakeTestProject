using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{

    public enum CollisionType
    {
        TAIL,
        OBSTACLE,
        FOOD
    }

    public CollisionType collisionType;

    [HideInInspector]
    public float time;

    public int points;
}
