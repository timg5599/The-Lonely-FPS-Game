using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class scr_StaticInterfaceQuest : scr_UserInterfaceQuest
{
  public GameObject[] slots;

    public override void CreateSlots()
    {
        slotsOnInterface = new Dictionary<GameObject, QuestSlot>();
        for (int i = 0; i < inventory.GetQuestSlots.Length; i++)
        {
            var obj = slots[i];
            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
            AddEvent(obj, EventTriggerType.PointerClick, delegate { OnPointerClick(obj); });
            inventory.GetQuestSlots[i].slotDisplay = obj;
            slotsOnInterface.Add(obj, inventory.GetQuestSlots[i]);
        }
    }
}
