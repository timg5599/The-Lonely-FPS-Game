using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GroundItem : MonoBehaviour,ISerializationCallbackReceiver
{
    // Start is called before the first frame update
    public scr_ItemObject item;
    [SerializeField] bool isNew=true;
    
    Rigidbody rigid;
    SphereCollider sphereCollider;
    BoxCollider lootCollider;
    public void OnAfterDeserialize(){

    }

    public void OnBeforeSerialize(){
        //땅에 보이는거 렌더링
    #if UNITY_EDITOR

    #endif
    }
    void Awake(){
        if(isNew){
            item.CreateItem();
        }
         rigid = GetComponent<Rigidbody>();
         sphereCollider = GetComponent<SphereCollider>();
         lootCollider = GetComponent<BoxCollider>();
        Instantiate(item.EquipedDisplay,this.transform);
        this.transform.rotation = Quaternion.identity;
        rigid.velocity = new Vector3(Random.Range(-1.0f, 1.0f),Random.Range(0.0f, 1.0f),Random.Range(-1.0f, 1.0f)) * 5 ;

    }
    private void Update()
    {
        transform.Rotate(Vector3.up *50* Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor"||collision.gameObject.tag == "Default")
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
