using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public enum QuestType{
    Normal,Hard,Epic,Legendary

}

public abstract class scr_QuestObject : ScriptableObject
{

    public QuestType type;
    
    [TextArea(15,20)]
    public string objective;
    public Sprite UIdisplay;
    public int monsterkillneed;
    public scr_Enemy monsterInfo;
    public scr_ItemObject[] rewards; 
    public Quest data = new Quest();
    public Quest CreateQuest(){
        Quest newQuest = new Quest(this);
        return newQuest;
    }
    }

[System.Serializable]
public class Quest{
    public string Name;
    public string questName;
    public int value;    
    public int Id =-1;
    public string monsterName;
    public int monsterKillCount;
    public bool questCompleteStatus;
     public Item reward; 
    public int beroReward;
    public int expReward;
    public int HonorPoint;
    public int questTypeMultiplier;

    public Quest(){
        Name ="";
        Id = -1;
    }
    public Quest(scr_QuestObject quest){
        questTypeMultiplier = QuestRewardMultiplier(quest.type);
        beroReward = quest.monsterInfo.enemyLevel * 100 * questTypeMultiplier;
        expReward = quest.monsterInfo.enemyLevel * 20 * questTypeMultiplier;
        HonorPoint = quest.monsterInfo.enemyLevel * questTypeMultiplier;
        Name = quest.name;
        questName = quest.monsterInfo.enemyName + " Elimination";
        Id = quest.data.Id;
        monsterName = quest.monsterInfo.enemyName;
        monsterKillCount = 0;
        questCompleteStatus= false;
        var assignReward = Random.Range(0f,1f);
        if(assignReward <0.05f){
            reward = quest.rewards[0].CreateItem();
        }else if (assignReward <0.15f){
            reward = quest.rewards[1].CreateItem();

        }else{
            reward = quest.rewards[Random.Range(3,5)].CreateItem();
        }
    }
    
    int QuestRewardMultiplier(QuestType x){
        if(x==QuestType.Normal){
            return 2;
        }else if(x==QuestType.Hard){
            return 4;
        }else if(x==QuestType.Epic){
            return 6;
        }else{
            return 8;
        }
    }
}


