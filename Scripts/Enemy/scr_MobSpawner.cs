using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_MobSpawner : MonoBehaviour
{
    public List<GameObject> MobsSpawned;
    public GameObject Mob;
    float nextCheckTime=0f;
    void Update(){

        if(Time.time > nextCheckTime){
                nextCheckTime += 3f;
            foreach(GameObject x in MobsSpawned.ToArray()){
                if(x==null){
                    MobsSpawned.Remove(x);
                }
            }
                if (MobsSpawned.Count<12){
                Vector3 ranVec = Vector3.right * Random.Range(-15 ,15) + Vector3.forward * Random.Range(-15,15);
                var size = Random.Range(0.8f,2.3f);
                Mob.transform.localScale = new Vector3(size,size,size);
                GameObject enemyInst = Instantiate(Mob,this.transform.position + ranVec,this.transform.rotation);
                scr_Enemy enemy = enemyInst.GetComponent<scr_Enemy>();
                enemy.maxHealth = (int)(size * enemy.maxHealth);
                enemy.target = this.transform;
                MobsSpawned.Add(enemyInst);
            }
            
        }
   
      
    }

}
