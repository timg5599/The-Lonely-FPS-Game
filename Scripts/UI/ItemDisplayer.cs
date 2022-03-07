using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ItemDisplayer : MonoBehaviour
{
    public InventorySlot item;
    [SerializeField]Image Icon;
    [SerializeField]Text itemName;
    [SerializeField]Text attributeArea;
    [SerializeField]GameObject[] itemGlares;
    int baseValue;
    int totalUpgradestat;

    // Update is called once per frame

    void Start(){
        Debug.Log(item);
        Debug.Log(item.ItemObject);
        Debug.Log(item.item);

        if (item.ItemObject != null) {
        Icon.sprite = item.ItemObject.UIDisplay;

        //for all EQP Items
        if(item.ItemObject.type != ItemType.WeaponScroll && item.ItemObject.type != ItemType.Potion &&item.ItemObject.type != ItemType.ETC){
            
            foreach(ItemBuff x in item.item.buffs){
                baseValue=0;
                foreach(ItemBuff y in item.item.basebuffValues){
                    if(y.attribute==x.attribute){
                        baseValue = y.value;
                    }
                }
                //display upgrade buffs
                if(x.value > baseValue){
                    totalUpgradestat += x.value-baseValue;
                    attributeArea.text += x.attribute + ": + " + x.value + " ("+baseValue +" <color='#B1FC59'>+ " +(x.value-baseValue)+"</color>)"+"\n";  
                }
                else if (x.value>0){
                    attributeArea.text += x.attribute + ": + " + x.value + "\n";  
                }
            }
        attributeArea.text += "\nNumber of Upgrades Available: " + item.item.numberOfUpgradesAvailable;
            
        }
        itemName.text = getItemName(item.ItemObject.name,totalUpgradestat);


        attributeArea.text += item.ItemObject.description;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }
    string getItemName(string name, int numstatdiff){
        var _return = "";
        if (numstatdiff==0) {
            _return += name;
        }
        else if(numstatdiff<5){
            _return += " <color='#4DA4EA'>"+name+ " (+"+  item.item.numberOfScrollSuccess +")</color>";
            itemGlares[0].SetActive(true);
            }
        else if (numstatdiff <15){
             _return += " <color='#6706A5'>"+name+ " (+"+  item.item.numberOfScrollSuccess +")</color>";
            itemGlares[1].SetActive(true);

        }
        else if (numstatdiff <25){
            _return += " <color='#ED2939'>"+name+ " (+"+  item.item.numberOfScrollSuccess +")</color>";
            itemGlares[2].SetActive(true);

        }
        else{
             _return += " <color='#B1FC59'>"+name+ " (+"+  item.item.numberOfScrollSuccess +")</color>";
            itemGlares[3].SetActive(true);
             
        }
        
       
        return _return;
    }
}
