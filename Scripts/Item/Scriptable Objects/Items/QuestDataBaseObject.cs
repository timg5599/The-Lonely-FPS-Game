using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "New Item Database",menuName ="Inventory System/Quest/Database")]
public  class QuestDataBaseObject : ScriptableObject,ISerializationCallbackReceiver
{
    public  scr_QuestObject[] QuestObjects;

    [ContextMenu("Update ID's")]
    public  void UpdateID()
    {
        for (int i = 0; i < QuestObjects.Length; i++)
        {
            if (QuestObjects[i].data.Id != i)
                QuestObjects[i].data.Id = i;
        }
    }
    public  void OnAfterDeserialize()
    {
        UpdateID();
    }

    public  void OnBeforeSerialize()
    {
    }
}