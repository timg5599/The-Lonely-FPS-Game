using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scr_CraftInfoPrefab : MonoBehaviour
{
    public Recipe recipeItem;
    public Image itemIcon;
    public Text itemName;
    public Text recipeAmount;
    public int playerAmount;


void OnAwake(){
}
private void OnEnable() {
    var slotItem = recipeItem.recipeItem;
    itemIcon.sprite = slotItem.UIDisplay;
    itemName.text = slotItem.name;
    UpdateAmountNeed(1);
}

public void UpdateAmountNeed(int playerCreatingAmount){
    var needAmount =recipeItem.recipeAmount * playerCreatingAmount;

    

    if(needAmount>playerAmount){
        recipeAmount.text = "<Color='#B80F0A'>" + playerAmount.ToString("n0")+ "</Color> / " +(needAmount).ToString("n0");
    }else{
        recipeAmount.text = playerAmount.ToString("n0")+ " / " +(needAmount).ToString("n0");
    }

}
}
