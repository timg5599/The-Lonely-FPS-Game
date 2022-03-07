using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_QuestHolder: MonoBehaviour
{
 [SerializeField]public scr_QuestObject[] fullQuestList;
 [SerializeField]public List<scr_QuestObject> currentQuestList;

 [SerializeField]public scr_QuestInventory playerCurrentQuest;
[SerializeField] public int questResetPrice;

 
 private void OnEnable() {
     if(currentQuestList.Count<6)Shufflelist();
 }
 public void Shufflelist(){
     currentQuestList.Clear();
     while(currentQuestList.Count <6){
        var newQuest = fullQuestList[Random.Range(0,fullQuestList.Length)];
        if(!currentQuestList.Contains(newQuest) && CheckPlayerQuest(newQuest)){
        currentQuestList.Add(newQuest);
        }
     }
 }
public bool CheckPlayerQuest(scr_QuestObject x){
    for(int i=0;i<playerCurrentQuest.GetQuestSlots.Length;i++){
        if(playerCurrentQuest.GetQuestSlots[i]!=null){
            if(playerCurrentQuest.GetQuestSlots[i].QuestObject == x){
                return false;
            }
        }
    }
    return true;
}
}
