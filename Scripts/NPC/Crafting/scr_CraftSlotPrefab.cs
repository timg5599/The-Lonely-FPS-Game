using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scr_CraftSlotPrefab : MonoBehaviour
{
    public scr_ItemObject slotItem;
    public Image itemIcon;
    public Text itemName;



private void OnEnable() {
    itemIcon.sprite = slotItem.UIDisplay;
    itemName.text = slotItem.name;
}   
}
