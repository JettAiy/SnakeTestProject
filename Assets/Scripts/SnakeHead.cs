using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : MonoBehaviour
{
    public event System.Action<CollisionDetector> OnCollision;



    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionDetector detector = collision.gameObject.GetComponent<CollisionDetector>();

        if (detector != null) OnCollision?.Invoke(detector);
    }

}
