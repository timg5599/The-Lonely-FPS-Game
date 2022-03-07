using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_TriggerDeathPlane : MonoBehaviour
{
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other){
        if (other.tag == "Player"){
            var player = other.GetComponent<scr_CharacterController>();
            player.onDeath();
        }else{
            Destroy(other);
        }
    }
}
