using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_PlayerProjectile: MonoBehaviour
{
    public int damage;
    public bool isCrit;
    public bool isMelee;
    [SerializeField] GameObject hitEffect;
    [SerializeField] Rigidbody rigid;
    [SerializeField] public Transform shooterTransform;
    [SerializeField] SphereCollider sphereCollider;


    void OnEnable() {
        if(!isMelee){
           Destroy(gameObject,2.5f);
       }
    }
    void Update() {
        transform.Rotate(Vector3.right * 180 * Time.deltaTime);
    }
     void OnCollisionEnter(Collision collision)
    {

        if((!isMelee)&&(collision.gameObject.tag == "Floor" || collision.gameObject.tag=="Default"|| collision.gameObject.tag=="Wall"))
        {
            Destroy(gameObject);
        }else if(collision.gameObject.tag=="Enemy"){
            ContactPoint contact = collision.contacts[0];
            Destroy(Instantiate(hitEffect, contact.point,Quaternion.identity),1f);
            Destroy(gameObject);

        }
    }
    void OnTriggerEnter(Collider other)
    {
        
        if(other.gameObject.tag=="Enemy"){

        }
        else if(!isMelee && (other.gameObject.tag == "Floor"|| other.gameObject.tag == "Default"))
        {
            Destroy(gameObject);
        }
    }

}
