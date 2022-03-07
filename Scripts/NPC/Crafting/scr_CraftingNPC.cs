using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
public class scr_CraftingNPC : MonoBehaviour
{
    public RectTransform uiGroup;
    [SerializeField]scr_CharacterController enterPlayer; 
    
    [SerializeField]Text playerGold;
    [SerializeField]ItemDataBaseObject craftableItemList;
    [SerializeField]Button EQPButton;
    [SerializeField]Button USEButton;
    [SerializeField]Button ETCButton;
    [SerializeField]GameObject[] craftCatagories;
    [SerializeField]GameObject craftSlotPrefab;
    [SerializeField]GameObject craftSlotPrefabList;
    [SerializeField]scr_CraftInfo craftInfoUI;
    [SerializeField]GameObject craftInfoUIGameObject;



    void OnEnable() {

        EQPButton.onClick.RemoveAllListeners();
        USEButton.onClick.RemoveAllListeners();
        ETCButton.onClick.RemoveAllListeners();
        EQPButton.onClick.AddListener(EQPClicked);
        USEButton.onClick.AddListener(USEClicked);
        ETCButton.onClick.AddListener(ETCClicked);
    }
    
    public void Enter(scr_CharacterController player){
        Debug.Log("Crafting");
        enterPlayer = player;
        if(!enterPlayer.isInventoryOn) enterPlayer.ToggleInventory();
        //화면정중앙에 배치
        enterPlayer.isCraftOn = true;
        uiGroup.anchoredPosition = Vector3.zero + Vector3.left*650;
        enterPlayer.UnlockCursor();
    }
    
    public void Exit(){
        Debug.Log("Exit Crafting");
        if(enterPlayer!=null){
        enterPlayer.isCraftOn = false;
        if(enterPlayer.isInventoryOn) enterPlayer.ToggleInventory();
        uiGroup.anchoredPosition = Vector3.down*1000;
        enterPlayer.LockCursor();
        craftInfoUIGameObject.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other) {
        Debug.Log(other.name);
    }
    void Update() {
        if(enterPlayer!=null && playerGold!=null)playerGold.text = enterPlayer.coins.ToString("n0");
    }
    void EQPClicked(){
        Debug.Log("EQPClicked");
        craftCatagories[0].SetActive(true);
        craftCatagories[1].SetActive(false);
        craftCatagories[2].SetActive(false);
    }
    void USEClicked(){
        Debug.Log("USEClicked");
        craftCatagories[0].SetActive(false);
        craftCatagories[1].SetActive(true);
        craftCatagories[2].SetActive(false);
    }
    void ETCClicked(){
        Debug.Log("ETCClicked");
        craftCatagories[0].SetActive(false);
        craftCatagories[1].SetActive(false);
        craftCatagories[2].SetActive(true);
    }

    public void UpdateItemList(ItemType[] x){
        foreach (Transform child in craftSlotPrefabList.transform) {
         GameObject.Destroy(child.gameObject);
        }
        var itemTypeList = new List<ItemType>();
        foreach(ItemType y in x){
            itemTypeList.Add(y);
        }
        for(int i=0;i<craftableItemList.ItemObjects.Length;i++){
            if(itemTypeList.Contains(craftableItemList.ItemObjects[i].type)&&(craftableItemList.ItemObjects[i].itemRecipe.Length !=0)){
                var prefab = craftSlotPrefab.GetComponent<scr_CraftSlotPrefab>();
                prefab.slotItem = craftableItemList.ItemObjects[i];
                var prefabInstance = Instantiate(prefab);
                prefabInstance.transform.parent = craftSlotPrefabList.transform;
                var _itemObject = craftableItemList.ItemObjects[i];
                prefabInstance.GetComponent<Button>().onClick.AddListener(delegate{UpdateInfo(_itemObject);});             
            }
        }
    }
    void UpdateInfo(scr_ItemObject _item){
        Debug.Log("Update Item Craft Info"+ _item.name);
        craftInfoUI.itemSelected = _item;
        craftInfoUI.UpdateOnce();
        craftInfoUIGameObject.SetActive(true);
    }

}
