using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class scr_DeathCountdown : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
       
        
       
    }
    void OnEnable(){
        Debug.Log("OnEnable");
         var textmesh = this.GetComponent<TextMeshProUGUI>();
        textmesh.text ="10";
        StartCoroutine(countdown(10));
    }
    
    // Update is called once per frame
    IEnumerator countdown(int x){
        while(x >0){
            Debug.Log(x);
            yield return new WaitForSeconds(1f);
            x --;
             var textmesh = this.GetComponent<TextMeshProUGUI>();
            textmesh.text = x.ToString();
        }
    }
}
