using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
public class scr_ShopNPC : MonoBehaviour
{
    public RectTransform uiGroup;
    public Animator NPCAnimator;
    [SerializeField]scr_CharacterController enterPlayer; 
    
    [SerializeField]public scr_ItemObject[] itemList;
    [SerializeField]GameObject storeSlotPrefab;
    [SerializeField]GameObject buyDialogPrefab;
    [SerializeField]GameObject sellDialogPrefab;


    [SerializeField]Transform canvasParent;
    [SerializeField]Text playerGold;
    GameObject isDialogOpen;

    void OnEnable() {
     instantiateItemList();   
   
    }

    public void mouseOverShop(){
        MouseData.shopMouseIsOver = this;
        Debug.Log(MouseData.shopMouseIsOver);
    }   
    public void mouseExitShop(){
        MouseData.shopMouseIsOver = null;
        Debug.Log(MouseData.shopMouseIsOver);

    }    
 
    public void Enter(scr_CharacterController player){
        Debug.Log("Shopping");
        enterPlayer = player;
        if(!enterPlayer.isInventoryOn) enterPlayer.ToggleInventory();
        //화면정중앙에 배치
        enterPlayer.isShop = true;
        uiGroup.anchoredPosition = Vector3.zero + Vector3.left*650;
        enterPlayer.UnlockCursor();
    }
    
    public void Exit(){
        Debug.Log("Exit Shop");
        if(enterPlayer!=null){
        enterPlayer.isShop = false;
        if(enterPlayer.isInventoryOn) enterPlayer.ToggleInventory();
        uiGroup.anchoredPosition = Vector3.down*1000;
        enterPlayer.LockCursor();
        }
    }
    private void OnTriggerEnter(Collider other) {
        Debug.Log(other.name);
    }
    void Update() {
        if(enterPlayer!=null && playerGold!=null)playerGold.text = enterPlayer.coins.ToString("n0");
    }
   public void instantiateItemList(){
         foreach (Transform child in this.transform) {
            GameObject.Destroy(child.gameObject);
        }
        for(int i=0;i<itemList.Length;i++){
            var x = itemList[i];
            var itemslot = Instantiate(storeSlotPrefab,Vector3.zero,Quaternion.identity);
            itemslot.transform.parent = this.transform;
            itemslot.GetComponent<scr_StoreSlotPrefab>().ItemIcon.sprite = itemList[i].UIDisplay;
            itemslot.GetComponent<scr_StoreSlotPrefab>().ItemName.text = itemList[i].name;
            itemslot.GetComponent<scr_StoreSlotPrefab>().ItemPrice.text = itemList[i].data.shopPrice+"";
            itemslot.GetComponent<Button>().onClick.AddListener(()=>purchaseAsk(x));
            }

    }
    void purchaseAsk(scr_ItemObject item){
        if(isDialogOpen == null){
        createBuyDialogBox(item);
        }else{
            Destroy(isDialogOpen);
            createBuyDialogBox(item);
        }
       
    }
    void createBuyDialogBox(scr_ItemObject item){
        if(item.type == ItemType.WeaponScroll ||item.type == ItemType.USE ||item.type ==  ItemType.ETC ||item.type ==  ItemType.Potion){            
            isDialogOpen = Instantiate(buyDialogPrefab);
            isDialogOpen.transform.parent = canvasParent;
            isDialogOpen.transform.localPosition = new Vector3(500,0,0);
            isDialogOpen.GetComponentInChildren<scr_InputFieldScriptBuy>().enterPlayer = this.enterPlayer;
            isDialogOpen.GetComponentInChildren<scr_InputFieldScriptBuy>().item = item;
            var texts = isDialogOpen.GetComponentsInChildren<Text>();
            texts[2].text =  "How many '"+"<color=White>"+item.name+"</color>" +"' do you need?";
            isDialogOpen.GetComponentInChildren<TMP_InputField>().Select();
        }else{
            isDialogOpen = Instantiate(buyDialogPrefab);
            isDialogOpen.transform.parent = canvasParent;
            isDialogOpen.transform.localPosition = new Vector3(500,0,0);
            isDialogOpen.GetComponentInChildren<scr_InputFieldScriptBuy>().enterPlayer = this.enterPlayer;
            isDialogOpen.GetComponentInChildren<scr_InputFieldScriptBuy>().item = item;
            isDialogOpen.GetComponentInChildren<scr_InputFieldScriptBuy>().isNumInput = false;
            var texts = isDialogOpen.GetComponentsInChildren<Text>();
            texts[2].text =  "Do you want to Purchase '"+"<color=White>"+item.name+"</color>" +"'?";
            var inputfieldImage =isDialogOpen.GetComponentsInChildren<Image>();
            Destroy(inputfieldImage[14]);
            isDialogOpen.GetComponentInChildren<TMP_InputField>().Select();

        }
    }

    public void createSellDialogBox(InventorySlot inventorySlot){
        if(inventorySlot.ItemObject.type == ItemType.WeaponScroll ||inventorySlot.ItemObject.type == ItemType.USE ||inventorySlot.ItemObject.type ==  ItemType.ETC ||inventorySlot.ItemObject.type ==  ItemType.Potion){            
            isDialogOpen = Instantiate(sellDialogPrefab);
            isDialogOpen.transform.parent = canvasParent;
            isDialogOpen.transform.localPosition = new Vector3(500,0,0);
            isDialogOpen.GetComponentInChildren<scr_InputFieldScriptSell>().enterPlayer = this.enterPlayer;
            isDialogOpen.GetComponentInChildren<scr_InputFieldScriptSell>().inventorySlot = inventorySlot;
            var texts = isDialogOpen.GetComponentsInChildren<Text>();
            texts[2].text =  "How many '"+"<color=White>"+inventorySlot.ItemObject.name+"</color>" +"' do you want to Sell? \n("+(int)inventorySlot.ItemObject.data.shopPrice/2+" Bero each)";
            isDialogOpen.GetComponentInChildren<TMP_InputField>().Select();
        }else{
            isDialogOpen = Instantiate(sellDialogPrefab);
            isDialogOpen.transform.parent = canvasParent;
            isDialogOpen.transform.localPosition = new Vector3(500,0,0);
            isDialogOpen.GetComponentInChildren<scr_InputFieldScriptSell>().enterPlayer = this.enterPlayer;
            isDialogOpen.GetComponentInChildren<scr_InputFieldScriptSell>().inventorySlot = inventorySlot;
            isDialogOpen.GetComponentInChildren<scr_InputFieldScriptSell>().isNumInput = false;
            var texts = isDialogOpen.GetComponentsInChildren<Text>();
            texts[2].text =  "Do you want to Sell '"+"<color=White>"+inventorySlot.ItemObject.name+"</color>" +"'?\n("+(int)inventorySlot.ItemObject.data.shopPrice/2+")";
            var inputfieldImage =isDialogOpen.GetComponentsInChildren<Image>();
            Destroy(inputfieldImage[14]);
            isDialogOpen.GetComponentInChildren<TMP_InputField>().Select();
        }
    }
}
