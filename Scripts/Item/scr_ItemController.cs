using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DuloGames.UI;

[CreateAssetMenu(fileName = "New Default Objects",menuName ="Inventory System/Items/Default")]
public class scr_ItemController : MonoBehaviour
{
    public int ID;
    public enum Type { 
    Coin, Equip, Use, Etc
    };
    public string _name;
    public UIItemQuality Quality;
    public UIEquipmentType EquipType;
    public Type type;
    public int value;
    [TextArea(15,20)]
    public Sprite icon;
    Rigidbody rigid;
    SphereCollider sphereCollider;
    BoxCollider lootCollider;

    
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        lootCollider = GetComponent<BoxCollider>();
    }
    private void Update()
    {
        transform.Rotate(Vector3.up *50* Time.deltaTime);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor"|| collision.gameObject.tag == "Default")
        {
            rigid.isKinematic = true;
            sphereCollider.enabled = false;
            lootCollider.enabled = true;
        }
    }
    void OnTriggerEnter(Collider other){        
        sphereCollider.enabled = true;
    }

}
