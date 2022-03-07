using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_FloatingText : MonoBehaviour
{
    // Start is called before the first frame update
    public float DestroyTime =3f; 
    public Vector3 offset = new Vector3(0,3,0);
    public Vector3 RandomizedIntensity = new Vector3(1.5f,1f,0);
    void Start(){
        Destroy(gameObject,DestroyTime);
        transform.localPosition+= offset;
        transform.localPosition += new Vector3(Random.Range(-RandomizedIntensity.x,RandomizedIntensity.x),
        Random.Range(-RandomizedIntensity.y,RandomizedIntensity.y),
        Random.Range(-RandomizedIntensity.z,RandomizedIntensity.z));
    }
    void Update(){

    }
}
