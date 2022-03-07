using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


public enum ItemType{
    COIN,USE,ETC,Potion,
    Helmet,
    Chest,
    Melee,
    Pistol,
    Rifle,
    Pant,
    Boot,
    Necklace,
    Ring,
    WeaponScroll,
    Glove,
    Cube

}
public enum Attributes{
    Strength=0,Dexterity=1,Intellect=2,Armor=3,WeaponDamage=4,Health=5,BossDamage=6,ArmorPenetration=7,PotionPower=8,HealthRegen=9,StaminaRegen=10,CriticalRate=11,CriticalDamage=12
}
public abstract class scr_ItemObject : ScriptableObject
{
    public Sprite UIDisplay;
    public GameObject EquipedDisplay;
    public bool stackable;
    public float dropRate;
    public float successRate;
    public Recipe[] itemRecipe;
    public int combinePrice;
    public ItemType type;
    
    [TextArea(15,20)]
    public string description;

    public Item data = new Item();
    public Item CreateItem(){
        Item newItem = new Item(this);
        return newItem;
    }
    public bool IsItemQuant(scr_ItemObject x){
        return (x.type == ItemType.ETC||x.type == ItemType.Potion||x.type == ItemType.WeaponScroll||x.type == ItemType.Cube||x.type == ItemType.USE);
    }
    }

[System.Serializable]
public class Item{
    public ItemType itemType;
    public string Name;
    public int value;
    public int cooldown;
    public int shopPrice;
    public int numberOfUpgradesAvailable;
    public int numberOfScrollSuccess;
    public int Id =-1;
    public List<ItemBuff> buffs;
    public List<ItemBuff> basebuffValues;
    public Item(){
        Name ="";
        Id = -1;
    }
    public Item(scr_ItemObject item){
        Name = item.name;
        Id = item.data.Id;
        itemType = item.type;
        numberOfScrollSuccess=0;
        shopPrice =item.data.shopPrice;
        numberOfUpgradesAvailable = item.data.numberOfUpgradesAvailable;
        buffs = new List<ItemBuff>();
        for (int i = 0 ; i < item.data.buffs.Count;i++){
            buffs.Add(new ItemBuff(item.data.buffs[i].min,item.data.buffs[i].max));
            buffs[i].attribute = item.data.buffs[i].attribute;
        }
        basebuffValues= new List<ItemBuff>();
        for(int i=0;i<buffs.Count;i++){
            basebuffValues.Add(new ItemBuff(buffs[i].value,buffs[i].value));
            basebuffValues[i].attribute = item.data.buffs[i].attribute;
        }
    buffs = buffs.OrderBy(x=>x.attribute).ToList();
        
    }
    
    
}

[System.Serializable]
public class ItemBuff:IModifier{
    public Attributes attribute;
    public int value;
    public int min;
    public int max;
    public ItemBuff(int _min,int _max){
        min = _min;
        max = _max;
        GenerateValue();
    }
    public void GenerateValue(){
        this.value = UnityEngine.Random.Range(min,max);
    }
    public void AddValue(ref int baseValue){
        baseValue +=value;
    }

}
[System.Serializable]
public class Recipe{
    public scr_ItemObject recipeItem;
    public int recipeAmount;
}
