using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharCollider : MonoBehaviour
{

    public bool collided = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Killzone")
        {
            collided = true;
        }
    }
}
