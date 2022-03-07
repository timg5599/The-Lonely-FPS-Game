using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using System;

public abstract class scr_UserInterface : MonoBehaviour
{
public scr_Inventory inventory;
public scr_CharacterController player;
public float potionCoolDown;
[SerializeField] GameObject ItemDisplayerPrefeb;
GameObject ItemDisplayer;
[SerializeField] GameObject[] EquipmentSlot;
[SerializeField] scr_StaticInterface EquipmentSlots;
[SerializeField] AudioClip[] UIsoundClips;
[SerializeField] ServerTalker server;


    int tap = 0 ;
float clicktime = 0;
float clickdelay = 0.6f;



 public Dictionary<GameObject, InventorySlot> slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

    public void Start()
    {
        for (int i = 0; i < inventory.GetSlots.Length; i++)
        {
            inventory.GetSlots[i].parent = this;
            inventory.GetSlots[i].OnAfterUpdate += OnSlotUpdate;

        }

        CreateSlots();
        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });
    }

    
    public abstract void CreateSlots();
    public void OnSlotUpdate(InventorySlot _slot){
        if(_slot.item.Id >=0){
                //Separate Item to Graphic from other to make it less heavy
                //Set Item Sprite into Inventory 
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().sprite = _slot.ItemObject.UIDisplay;
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().color =new Color(1,1,1,1);
                _slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text = _slot.amount ==1? "" : _slot.amount.ToString("n0");
            }else{
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().sprite =null;
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().color =new Color(1,1,1,0);
                _slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text ="";
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
        MouseData.slotHoveredOver = obj;
        try {
            if (slotsOnInterface[MouseData.slotHoveredOver] == null)
            {
            }
            else
            {
                ItemDisplayer = Instantiate(ItemDisplayerPrefeb, this.transform);
                ItemDisplayer.transform.SetAsLastSibling();
                ItemDisplayer.GetComponent<ItemDisplayer>().item = slotsOnInterface[obj];
            }
        }
        catch (Exception e)
        {
            
        }
          
    }
    public void OnExit(GameObject obj){
        if(ItemDisplayer!= null){
            Destroy(ItemDisplayer);
        }
        MouseData.slotHoveredOver = null;
    }
    public void OnEnterInterface(GameObject obj){
        MouseData.interfaceMouseIsOver = obj.GetComponent<scr_UserInterface>();
        Debug.Log(MouseData.interfaceMouseIsOver);

    }
    public void OnExitInterface(GameObject obj){
        MouseData.interfaceMouseIsOver = null;
    }


    public void OnDragStart(GameObject obj){
        if(slotsOnInterface[obj].item.Id <=-1){
            return;
        }

        MouseData.tempItemBeingDragged = CreateTempItem(obj);
    }   
    public GameObject CreateTempItem(GameObject obj){
        GameObject tempItem=null;
        if(slotsOnInterface[obj].item.Id>=0){
        tempItem = new GameObject();
        var rt = tempItem.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50,50);
        tempItem.transform.SetParent(transform.parent);
        var img = tempItem.AddComponent<Image>();
        img.sprite = slotsOnInterface[obj].ItemObject.UIDisplay;
        img.raycastTarget =false;
        }

        return tempItem;
    }
    public void OnDragEnd(GameObject obj){
        
      player.sfxAudio.PlayOneShot(UIsoundClips[0]);
      Destroy(MouseData.tempItemBeingDragged);
      Debug.Log(MouseData.shopMouseIsOver);
        //Sell Item
        if(MouseData.shopMouseIsOver != null){
                MouseData.shopMouseIsOver.createSellDialogBox(slotsOnInterface[obj]);
                return;
        }  
        if (MouseData.interfaceMouseIsOver == null)
        {
            // slotsOnInterface[obj].RemoveItem();
            return;
        }
        if (MouseData.slotHoveredOver)
        {
            InventorySlot mouseHoverSlotData = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver];
                //Upgrade Scrolls
            if(slotsOnInterface[obj].ItemObject.type == ItemType.WeaponScroll && (mouseHoverSlotData.ItemObject.type!=ItemType.Melee||mouseHoverSlotData.ItemObject.type!=ItemType.Pistol||mouseHoverSlotData.ItemObject.type!=ItemType.Rifle)){
                if(mouseHoverSlotData.ItemObject.type ==ItemType.Melee||mouseHoverSlotData.ItemObject.type ==ItemType.Rifle||mouseHoverSlotData.ItemObject.type ==ItemType.Pistol){
                        if(mouseHoverSlotData.item.numberOfUpgradesAvailable >0){
                        slotsOnInterface[obj].amount --;
                        mouseHoverSlotData.item.numberOfUpgradesAvailable--;
                        if(UnityEngine.Random.Range(0f,1f) < slotsOnInterface[obj].ItemObject.successRate){
                            Debug.Log(1);
                            //Success sound
                            StartCoroutine(player.ScrollUIShowAndHide());
                            player.sfxAudio.PlayOneShot(UIsoundClips[2]);
                            mouseHoverSlotData.item.numberOfScrollSuccess++;
                            var scroll = slotsOnInterface[obj].item;
                            var item =  mouseHoverSlotData.item;
                            foreach(ItemBuff buffScroll in scroll.buffs){
                                //check if Scroll Buff is in Item
                                foreach(ItemBuff buffItem in item.buffs){
                                    if(buffItem.attribute == buffScroll.attribute){
                                    buffItem.value += buffScroll.value;
                                }
                            mouseHoverSlotData.item.buffs = mouseHoverSlotData.item.buffs.OrderBy(x=>x.attribute).ToList();
                            }
                        }
                        }else{
                            Debug.Log("fail scroll");
                            player.sfxAudio.PlayOneShot(UIsoundClips[1]);
                        }
                        player.server.SaveInventory(player.userId, "CurrentEquipment", player.equipment.SaveToServer());
                        player.server.SaveInventory(player.userId, "USEInventory", player.inventoryUSE.SaveToServer());
                    }
                }
            }
            //Stack on Drop
            else if(slotsOnInterface[obj].ItemObject.type ==ItemType.Potion){
                if(slotsOnInterface[obj] == mouseHoverSlotData){
                    Debug.Log("SameSlot");
                }else{
                    if(mouseHoverSlotData.ItemObject == null || mouseHoverSlotData.ItemObject.name != slotsOnInterface[obj].ItemObject.name){
                    inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
                }
                else if(mouseHoverSlotData.ItemObject.name == slotsOnInterface[obj].ItemObject.name){
                    if(mouseHoverSlotData.amount + slotsOnInterface[obj].amount <=200){
                        mouseHoverSlotData.AddAmount(slotsOnInterface[obj].amount);
                        slotsOnInterface[obj].RemoveItem();
                    }else{
                        var originalamount = slotsOnInterface[obj].amount;
                        var leftoveramount = slotsOnInterface[obj].amount-(200-mouseHoverSlotData.amount);
                        mouseHoverSlotData.AddAmount(200-mouseHoverSlotData.amount);
                        slotsOnInterface[obj].UpdateSlot(slotsOnInterface[obj].item,leftoveramount);
                    }
                }
                }

            }
            else{
                //Debug.Log("slotOnInterface[obj] : "+ slotsOnInterface[obj] +" mouseHoverSlotData : " + mouseHoverSlotData );
                inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
                }
                //Update Item

            slotsOnInterface[obj].UpdateSlot(slotsOnInterface[obj].item,slotsOnInterface[obj].amount);
            mouseHoverSlotData.UpdateSlot(mouseHoverSlotData.item,mouseHoverSlotData.amount);
            checkAmountZero(slotsOnInterface[obj]);
            checkAmountZero(mouseHoverSlotData);


        }
    }

    string checkItemType(InventorySlot x)
    {
        if (x.ItemObject.type == ItemType.ETC)
        {
            return "ETC";
        }
        else if (x.ItemObject.type == ItemType.USE || x.ItemObject.type == ItemType.WeaponScroll || x.ItemObject.type == ItemType.Potion || x.ItemObject.type == ItemType.Cube) {
            return "USE";
        }
        else
        {
            return "EQP";
        }
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
        tap++;
       if (tap == 1) clicktime = Time.time;
        Debug.Log(obj.tag);
        if (tap > 1 && Time.time - clicktime < clickdelay)
        {
            tap = 0;
            clicktime = 0;
            var itemClicked = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver].ItemObject.type;
            if(ItemDisplayer !=null){
                Destroy(ItemDisplayer);
            }
            if(MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver].ItemObject!=null){
                switch(itemClicked){
                    case ItemType.Melee:
                        InventorySlot meleeEquipmentSlot = EquipmentSlots.slotsOnInterface[EquipmentSlot[2]];
                        inventory.SwapItems(slotsOnInterface[obj], meleeEquipmentSlot);
                        break;
                    case ItemType.Pistol:
                        InventorySlot pistolEquipmentSlot = EquipmentSlots.slotsOnInterface[EquipmentSlot[3]];
                        inventory.SwapItems(slotsOnInterface[obj], pistolEquipmentSlot);
                        break;
                    case ItemType.Rifle:
                        InventorySlot mainEquipmentSlot = EquipmentSlots.slotsOnInterface[EquipmentSlot[4]];
                        inventory.SwapItems(slotsOnInterface[obj], mainEquipmentSlot);
                        break;
                    case ItemType.Potion:
                        player.usePotion(slotsOnInterface[obj]);
                        break;
                    default:
                        break;
                }
            }
        }
        else if (tap > 2 || Time.time - clicktime > 1) tap = 0;
    }



}

public static class MouseData{

    public static scr_UserInterface interfaceMouseIsOver;
    public static GameObject tempItemBeingDragged;
    public static GameObject slotHoveredOver;
    public static scr_ShopNPC shopMouseIsOver;

    public static scr_UserInterfaceQuest interfaceMouseIsOverQuest;
    public static GameObject tempItemBeingDraggedQuest;
    public static GameObject slotHoveredOverQuest;

}
public static class ExtensionMethod{
    public static void UpdateSlotDisplay(this Dictionary<GameObject,InventorySlot> _slotsOnInterface ){
        foreach(KeyValuePair<GameObject,InventorySlot> x in _slotsOnInterface){
            Debug.Log(x.Value.item.Name);
        }
        foreach(KeyValuePair<GameObject,InventorySlot> _slot in _slotsOnInterface){
            Debug.Log("UpdatingSlotDisplay");
            if(_slot.Value.item.Id >=0){
                //Separate Item to Graphic from other to make it less heavy
                //Set Item Sprite into Inventory 
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = _slot.Value.ItemObject.UIDisplay;
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color =new Color(1,1,1,1);
                _slot.Key.GetComponentInChildren<TextMeshProUGUI>().text = _slot.Value.amount ==1? "" : _slot.Value.amount.ToString("n0");
            }else{
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite =null;
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color =new Color(1,1,1,0);
                _slot.Key.GetComponentInChildren<TextMeshProUGUI>().text ="";
            }
        }
    }
}