using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public abstract class scr_UserInterfaceQuest : MonoBehaviour
{
public scr_QuestInventory inventory;
public scr_CharacterController player;
[SerializeField] AudioClip[] UIsoundClips;
[SerializeField] Text questTitle;
[SerializeField] Text questObjective;
[SerializeField] Text questProgress;
[SerializeField] Text rewardBero;

[SerializeField] Text rewardEXP;

[SerializeField] Text rewardHonorpoint;
[SerializeField] Image[] possibleQuestRewards;
[SerializeField] Button questForfeitButton;
[SerializeField] Button questCompleteButton;
QuestSlot currentQuestOnUI;
[SerializeField] GameObject questInfoDetail;


int tap = 0 ;
float clicktime = 0;
float clickdelay = 0.6f;


 public Dictionary<GameObject, QuestSlot> slotsOnInterface = new Dictionary<GameObject, QuestSlot>();

    public void Start()
    {
        for (int i = 0; i < inventory.GetQuestSlots.Length; i++)
        {
            inventory.GetQuestSlots[i].parent = this;
            inventory.GetQuestSlots[i].OnAfterUpdate += OnSlotUpdate;

        }

        CreateSlots();
        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });

        questForfeitButton.onClick.AddListener(ForfeitCurrentQuest);
        questCompleteButton.onClick.AddListener(CompleteCurrentQuest);
        scr_Enemy.enemyDeadNoParam += UpdateQuestInfo;

    }

    
    public abstract void CreateSlots();
    public void OnSlotUpdate(QuestSlot _slot){
        if(_slot.quest.Id >=0){
                //Separate Item to Graphic from other to make it less heavy
                //Set Item Sprite into Inventory 
                Debug.Log("here");
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Text>().text = _slot.quest.questName;
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Text>().color =Color.white;
            }else{
                Debug.Log("here2");
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Text>().text =null;
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Text>().color =new Color(1,1,1,0);
            }
    }
    

    


    public void AddEvent(GameObject obj,EventTriggerType type,UnityAction<BaseEventData> action){
        //Event trigger for button overlay on each Inventory Slots
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }
    public void OnEnter(GameObject obj){
        MouseData.slotHoveredOverQuest = obj;            
        Debug.Log(obj.name);
    }
    public void OnExit(GameObject obj){
        MouseData.slotHoveredOverQuest = null;
    }
    public void OnEnterInterface(GameObject obj){
        MouseData.interfaceMouseIsOverQuest = obj.GetComponent<scr_UserInterfaceQuest>();

    }
    public void OnExitInterface(GameObject obj){
        MouseData.interfaceMouseIsOverQuest = null;
    }


    public void OnDragStart(GameObject obj){
        if(slotsOnInterface[obj].quest.Id <=-1){
            return;
        }

    }   
    public GameObject CreateTempItem(GameObject obj){
        GameObject tempItem=null;
        if(slotsOnInterface[obj].quest.Id>=0){
        tempItem = new GameObject();
        var rt = tempItem.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50,50);
        tempItem.transform.SetParent(transform.parent);
        var img = tempItem.AddComponent<Image>();
        img.sprite = slotsOnInterface[obj].QuestObject.UIdisplay;
        img.raycastTarget =false;
        }

        return tempItem;
    }
    public void OnDragEnd(GameObject obj){
        

    }
    void checkAmountZero(InventorySlot x){
        if(x.amount<=0){
            x.RemoveItem();
        }
    }
    public void OnDrag(GameObject obj){
        Debug.Log(obj +"");
        if(MouseData.tempItemBeingDragged != null){
            MouseData.tempItemBeingDragged.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }
    public void OnPointerClick(GameObject obj){
        player.sfxAudio.PlayOneShot(UIsoundClips[0]);
        QuestSlot mouseHoverSlotData = MouseData.interfaceMouseIsOverQuest.slotsOnInterface[MouseData.slotHoveredOverQuest];
        if(mouseHoverSlotData.quest.Id>=0){
            questInfoDetail.SetActive(true);
            currentQuestOnUI = mouseHoverSlotData;
            questTitle.text = mouseHoverSlotData.quest.questName;
            questObjective.text = mouseHoverSlotData.QuestObject.objective;
            questProgress.text = mouseHoverSlotData.quest.monsterName + " : " + mouseHoverSlotData.quest.monsterKillCount + " / " + mouseHoverSlotData.QuestObject.monsterkillneed;
            //Create Reward UI
            for(int i=0;i<mouseHoverSlotData.QuestObject.rewards.Length;i++){
                possibleQuestRewards[i].sprite = mouseHoverSlotData.QuestObject.rewards[i].UIDisplay;
            }
            rewardBero.text = mouseHoverSlotData.quest.beroReward.ToString("n0");
            rewardEXP.text = mouseHoverSlotData.quest.expReward.ToString("n0");
            rewardHonorpoint.text = mouseHoverSlotData.quest.HonorPoint.ToString("n0");
        }else{

        }
    }
    void ForfeitCurrentQuest(){
        currentQuestOnUI.RemoveQuest();
        questInfoDetail.SetActive(false);
        currentQuestOnUI=null;
    }
    void CompleteCurrentQuest(){
        if(currentQuestOnUI.quest.questCompleteStatus){
            player.playerAddItemToInv(currentQuestOnUI.quest.reward);
            player.GainExp(currentQuestOnUI.quest.expReward);
            player.coins += currentQuestOnUI.quest.beroReward;
            player.honorPoint+= currentQuestOnUI.quest.HonorPoint;
            currentQuestOnUI.RemoveQuest();
            questInfoDetail.SetActive(false);
            currentQuestOnUI =null;
        }else{
            player.statusLogger.ReceiveChatMessage(1,"<Color='#f67280'>Quest is not Completed.</Color>");
        }
    }
    void UpdateQuestInfo(){
        Debug.Log("called");
        if(currentQuestOnUI !=null){
            questProgress.text = currentQuestOnUI.quest.monsterName + " : " + currentQuestOnUI.quest.monsterKillCount + " / " + currentQuestOnUI.QuestObject.monsterkillneed;
        }
    }
}


public static class ExtensionMethod2{
    public static void UpdateSlotDisplay2(this Dictionary<GameObject,QuestSlot> _slotsOnInterface ){
        foreach(KeyValuePair<GameObject,QuestSlot> x in _slotsOnInterface){
            Debug.Log(x.Value.quest.Name);
        }
        foreach(KeyValuePair<GameObject,QuestSlot> _slot in _slotsOnInterface){
            Debug.Log("UpdatingSlotDisplay");
            if(_slot.Value.quest.Id >=0){
                //Separate Item to Graphic from other to make it less heavy
                //Set Item Sprite into Inventory 
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Text>().text = _slot.Value.quest.questName;
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Text>().color =new Color(1,1,1,1);
            }else{
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Text>().text =null;
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Text>().color =new Color(1,1,1,0);
            }
        }
    }
}