using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DuloGames.UI;


public class scr_GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;

    public scr_CharacterController player;

    public GameObject playerHealthbar;
    public GameObject playerAmmobar;
    public GameObject playerExpbar;
    
    public GameObject menuPanel,gamePanel,deathPanel;
    public Text maxScoreTxt;
    public Text playerCoinTxt;
    Text playerHealthTxt;
    Text playerAmmoTxt;
    [SerializeField]Text playerLevelText;

    [SerializeField]TextMeshProUGUI playerCurrentAmmoTxt;

    UIProgressBar filledHpComponent;   
    UIProgressBar filledAmmoComponent;
    UIProgressBar filledExpComponent;
    [SerializeField]Sprite emptyWeaponImage;
    [SerializeField]Image statusWeaponImage;


    float playerHpRatio;
    float playerAmmoRatio;
    float playerExpRatio;



    public Image weapon1;
    public Image weapon2;
    public Image weapon3;
    public RectTransform enemyHealthGroup;
    
    [SerializeField] Text playerBero;
    [SerializeField] Text playerDiamond;
    [SerializeField] Text playerCurrentDamage;
    [SerializeField] Text playerCurrentArmor;


    
    void Awake(){
        enemyHealthGroup.anchoredPosition = Vector3.up*1000;
        filledHpComponent = playerHealthbar.GetComponent<UIProgressBar>();
        filledAmmoComponent = playerAmmobar.GetComponent<UIProgressBar>();
        filledExpComponent = playerExpbar.GetComponent<UIProgressBar>();

        playerHpRatio=1;
        playerAmmoRatio = (((float)player.playerCurrentStamina)/player.playerMaxStamina);
        playerExpRatio = (((float)player.playerCurrentExp)/player.playerRequiredExp);

    }

  
    public void GameStart(){
        menuCam.SetActive(false);
        gameCam.SetActive(true);
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);
        player.gameObject.SetActive(true);
    }
    public void GameExit(){
        Application.Quit();
    }    
    void LateUpdate()
    {  
        //Player UI
         //Weapon UI Visiblity 만들것.
         UpdatePlayerStatus();
         if(player.isDead){
            deathPanel.SetActive(true); 
         }else{
             deathPanel.SetActive(false);
         }
    }
    void UpdatePlayerStatus(){
        if (player!=null){
            playerHpRatio=Mathf.MoveTowards(playerHpRatio,(((float)player.currenthealth)/ (float)player.attributes[4].value.ModifiedValue),0.3f*Time.deltaTime);
            playerAmmoRatio = Mathf.MoveTowards(playerAmmoRatio,(((float)player.playerCurrentStamina)/player.playerMaxStamina),0.3f*Time.deltaTime);
            playerExpRatio = Mathf.MoveTowards(playerExpRatio,(((float)player.playerCurrentExp)/player.playerRequiredExp),0.6f*Time.deltaTime);
            playerHealthTxt = playerHealthbar.GetComponentInChildren<Text>();
            playerAmmoTxt = playerAmmobar.GetComponentInChildren<Text>();
            playerHealthTxt.text = player.currenthealth.ToString("n0") + " / " + player.attributes[4].value.ModifiedValue.ToString("n0");
            playerAmmoTxt.text =  player.playerCurrentStamina.ToString("n0") + " / " + player.playerMaxStamina.ToString("n0");
            filledHpComponent.fillAmount = playerHpRatio;
            filledAmmoComponent.fillAmount = playerAmmoRatio;  
            filledExpComponent.fillAmount = playerExpRatio;  
            playerCurrentAmmoTxt.text = player.currentWeaponAmmo+"";   
            //Level
            playerLevelText.text = player.playerLevel+"";       
         }
  

        if(player.equipedWeapon == null){
            statusWeaponImage.sprite = emptyWeaponImage;
        }
        playerBero.text = player.coins.ToString("n0");
        playerDiamond.text = player.diamond.ToString("n0");
        playerCurrentDamage.text = player.damage.ToString("n0");
        playerCurrentArmor.text = player.attributes[3].value.ModifiedValue.ToString("n0");




    }
    



}
