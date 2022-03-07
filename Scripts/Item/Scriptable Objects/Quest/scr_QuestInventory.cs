using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor;
using System.Runtime.Serialization;


[CreateAssetMenu(fileName ="New QuestInventory",menuName = "Quest System/QuestInventory")]
public class scr_QuestInventory :ScriptableObject
{
    public string savePath;
    public QuestDataBaseObject database;
    public QuestInventory questContainer;
    public QuestSlot[] GetQuestSlots{get{return questContainer.questSlots;}}
 
    public bool AddQuest(Quest _quest){
        // Find first Empty InventorySlot
        if (EmptySlotCount <= 0){
            return false;
        }
        QuestSlot slot = FindQuestOnInventory(_quest);
        if(slot==null){
            SetEmptySlot(_quest);
        return true;
        }else{
            return false;
        }


        }
    public int EmptySlotCount
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < GetQuestSlots.Length; i++)
            {
                if (GetQuestSlots[i].quest.Id <= -1)
                {
                    counter++;
                }
            }
            return counter;
        }
    }
    public QuestSlot FindQuestOnInventory(Quest _quest)
    {
        for (int i = 0; i < GetQuestSlots.Length; i++)
        {
            if(GetQuestSlots[i].quest.Id == _quest.Id)
            {
                return GetQuestSlots[i];
            }
        }
        return null;
    }
    public QuestSlot SetEmptySlot(Quest _quest){
        //Used to Find first Empty Slot
           for(int i =0; i< GetQuestSlots.Length;i++){
            if(GetQuestSlots[i].quest.Id<=-1){
                GetQuestSlots[i].UpdateSlot(_quest);
                return GetQuestSlots[i];
            }
        }
        //Set up function when full inventory
        return null;
    }
    public void SwapItems(QuestSlot item1, QuestSlot item2){
        if(item2.CanPlaceInSlot(item1.QuestObject) && item1.CanPlaceInSlot(item2.QuestObject)){
            QuestSlot temp = new QuestSlot(item2.quest);
            item2.UpdateSlot(item1.quest);
            item1.UpdateSlot(temp.quest);
        }
    }

    public void RemoveItem(Quest _quest){
        for(int i = 0 ; i < GetQuestSlots.Length;i++){
            if(GetQuestSlots[i].quest==_quest){
                GetQuestSlots[i].UpdateSlot(null);
            }
        }
    }
    [ContextMenu("SaveQuest")]
    public void SaveQuest()
    {
    //    string saveData = JsonUtility.ToJson(this, true);
    //     BinaryFormatter bf = new BinaryFormatter();
    //     FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
    //     bf.Serialize(file, saveData);
    //     file.Close();
        IFormatter formatter2 = new BinaryFormatter();
        Stream newstream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create, FileAccess.Write);
        formatter2.Serialize(newstream, questContainer);
        newstream.Close();
    }
    [ContextMenu("LoadQuest")]
    public void LoadQuest()
    {
    //   if(File.Exists(string.Concat(Application.persistentDataPath, savePath))){
    //         BinaryFormatter bf = new BinaryFormatter();
    //         FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open);
    //         JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), this);
    //         file.Close();
    //     }
            IFormatter formatter = new BinaryFormatter();
            Stream newstream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
            QuestInventory newContainer = (QuestInventory)formatter.Deserialize(newstream);
            for(int i = 0 ; i < GetQuestSlots.Length; i++){
                questContainer.questSlots[i].UpdateSlot(newContainer.questSlots[i].quest);
            }
            newstream.Close();

    }

    [ContextMenu("Clear")]
    public void Clear(){
        questContainer.Clear();
    }


    
}


[System.Serializable]
public class QuestInventory{
    public QuestSlot[] questSlots = new QuestSlot[10];
    public void Clear(){
        for(int i = 0 ; i<questSlots.Length;i++){
            questSlots[i].RemoveQuest();
        }
    }

}

public delegate void QuestSlotUpdated(QuestSlot _slot);
[System.Serializable]
public class QuestSlot{
    public QuestType[] AllowedQuest = new QuestType[0];
    [System.NonSerialized]
    public scr_UserInterfaceQuest parent;
    [System.NonSerialized]
    public GameObject slotDisplay;
    [System.NonSerialized]
    public QuestSlotUpdated OnAfterUpdate;
    [System.NonSerialized]
    public QuestSlotUpdated OnBeforeUpdate;
    public Quest quest = new Quest();
    public scr_QuestObject QuestObject
    {
        get{
            if(quest.Id >=0){
                return parent.inventory.database.QuestObjects[quest.Id];
            }
            return null;
        }
    }
    public bool CanPlaceInSlot(scr_QuestObject _itemObject){
        if(AllowedQuest.Length <=0 || _itemObject == null||_itemObject.data.Id< 0){
            return true;
        }
        for(int i = 0; i < AllowedQuest.Length;i++){
            if(_itemObject.type == AllowedQuest[i]){
                return true;
            }
        }
        return false;

    }
    public QuestSlot(){
        UpdateSlot(new Quest());

    }
    public QuestSlot(Quest _quest){
        UpdateSlot(_quest);
    }   
  
    public void UpdateSlot(Quest _quest){

        if(OnBeforeUpdate!=null)
            OnBeforeUpdate.Invoke(this);
        quest = _quest;
        if(OnAfterUpdate!= null)
            OnAfterUpdate.Invoke(this);
    
    } 

    public void RemoveQuest(){
        UpdateSlot(new Quest());

    }

}
