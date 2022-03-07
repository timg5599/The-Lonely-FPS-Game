using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
public class scr_InputFieldScriptBuy : MonoBehaviour
{

    [SerializeField]public scr_ItemObject item;
    [SerializeField]TextMeshProUGUI text;
    [SerializeField]public scr_CharacterController enterPlayer;
    [SerializeField]GameObject dialogBox;
    [SerializeField]TMP_InputField InputField;
    [SerializeField]Button exitButton;
    [SerializeField]Button purchaseButton;
    [SerializeField]Text warningText;

    public bool isNumInput=true;


    int amount=-1;
    bool isDeselect;
    void OnEnable() {
        exitButton.onClick.AddListener(exit);
        purchaseButton.onClick.AddListener(purchaseItem);
        
    }
    void exit(){
        Destroy(dialogBox);
    }
    

    public void OnValueChangedEvent(string inputAmount){
    }
    public void OnEndEditEvent(string num){
       StartCoroutine(EndEditCoroutine());
    }
    public void OnSelectEvent(string str){
        if(!isNumInput){
            text.color = new Color(0,0,0,0);
            amount=1;
        }else{
            text.text ="1";
        };
    } 
    public void OnDeselectEvent(string str){
      
        isDeselect= true;        
    }
    IEnumerator EndEditCoroutine(){
        yield return new WaitForSeconds(0.1f);
        var amountString ="";
        var defaultint =0;
        for(int i=0;i<text.text.Length-1;i++){
            amountString+=text.text[i];
        }
        Debug.Log(amount);
        if(Int32.TryParse(amountString,out defaultint))amount =Int32.Parse(amountString);
        if(isDeselect){
            exit();
        }else if((amount<=0 || amount>200)&&isNumInput){
            warningText.color = Color.red; 
            warningText.text = "Please enter a number between 1 and 200";
        }else{
            purchaseItem();
        };
    }
    void purchaseItem(){
            if(amount*item.data.shopPrice > enterPlayer.coins){
                warningText.color = Color.red;  
                warningText.text = "Not Enough Bero";
                return;
            }
            if((amount<=0 || amount>200)&&isNumInput){
                
            }else{
                switch(item.type){
                    case ItemType.ETC:
                        enterPlayer.statusLogger.ReceiveChatMessage(1,"Purchased: "+amount+" "+ item.name );
                        enterPlayer.coins -= item.data.shopPrice*amount;
                        enterPlayer.sfxAudio.PlayOneShot(enterPlayer.defaultLootClips[UnityEngine.Random.Range(0, enterPlayer.defaultLootClips.Length)]);
                        enterPlayer.inventoryETC.AddItem(item.CreateItem(),amount);
                    break;

                    case ItemType.USE:
                        enterPlayer.statusLogger.ReceiveChatMessage(1,"Purchased: "+amount+" "+ item.name );
                        enterPlayer.coins -= item.data.shopPrice*amount;
                        enterPlayer.sfxAudio.PlayOneShot(enterPlayer.defaultLootClips[UnityEngine.Random.Range(0, enterPlayer.defaultLootClips.Length)]);
                        enterPlayer.inventoryUSE.AddItem(item.CreateItem(),amount);
                    break;

                    case ItemType.Potion:
                        enterPlayer.statusLogger.ReceiveChatMessage(1,"Purchased: "+amount+" "+ item.name );
                        enterPlayer.coins -= item.data.shopPrice*amount;
                        enterPlayer.sfxAudio.PlayOneShot(enterPlayer.defaultLootClips[UnityEngine.Random.Range(0, enterPlayer.defaultLootClips.Length)]);
                        enterPlayer.inventoryUSE.AddItem(item.CreateItem(),amount);
                    break;

                    case ItemType.WeaponScroll:
                        enterPlayer.statusLogger.ReceiveChatMessage(1,"Purchased: "+amount+" "+ item.name );
                        enterPlayer.coins -= item.data.shopPrice*amount;
                        enterPlayer.sfxAudio.PlayOneShot(enterPlayer.defaultLootClips[UnityEngine.Random.Range(0, enterPlayer.defaultLootClips.Length)]);
                        enterPlayer.inventoryUSE.AddItem(item.CreateItem(),amount);
                    break;
                    default:
                        enterPlayer.statusLogger.ReceiveChatMessage(1,"Purchased: "+ item.name );
                        enterPlayer.coins -= item.data.shopPrice;
                        enterPlayer.sfxAudio.PlayOneShot(enterPlayer.defaultLootClips[UnityEngine.Random.Range(0, enterPlayer.defaultLootClips.Length)]);
                        enterPlayer.inventoryEQP.AddItem(item.CreateItem(),1);
                    break;
                }
            Destroy(dialogBox);
            }
    }
}
