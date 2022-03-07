using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_EnemyHitbox: MonoBehaviour
{
    public int damage;
    public bool isMelee;
    void OnEnable() {
        if(!isMelee){
           Destroy(gameObject,5);
       }
    }
    void Update() {
        
    }
     void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with " + collision.gameObject.tag );
        if((!isMelee)&&(collision.gameObject.tag == "Floor" || collision.gameObject.tag=="Default"|| collision.gameObject.tag=="Wall"))
        {
            Destroy(gameObject);
        }
    }
    void OnTriggerEnter(Collider other)
    {
       
        if(!isMelee && (other.gameObject.tag == "Floor"|| other.gameObject.tag == "Default"))
        {
            Destroy(gameObject);
        }
    }
}
