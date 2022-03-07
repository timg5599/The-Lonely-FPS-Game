using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Reflection;

public class scr_QuestNPC : MonoBehaviour
{
    // Start is called before the first frame update

    public List<scr_QuestObject> questListOnBoard;
    scr_CharacterController enterPlayer;
    [SerializeField]RectTransform uiGroup;
    [SerializeField]GameObject questInfo;
    [SerializeField]GameObject[] questHolderPrefabs;
    [SerializeField]Text questInfoTitle;
    [SerializeField]Text questInfoDialog;
    [SerializeField]Text questInfoBero;
    [SerializeField]Text questInfoEXP;
    [SerializeField]Text questInfoHonorPoint;
    [SerializeField]Image[] possibleQuestRewards;
    [SerializeField]AudioClip clickSound;
    [SerializeField]AudioClip AcceptQuest;
    [SerializeField] Text resetGoldText;

    public scr_QuestHolder questHolder;
    int tap = 0 ;
    float clicktime = 0;
    float clickdelay = 0.5f;
    GameObject currentQuestSlot;

    public void Enter(scr_CharacterController player){
        Debug.Log("Enter QuestNPC");
        enterPlayer = player;
        //화면정중앙에 배치
        enterPlayer.isQuestBoardOn=true;
        uiGroup.anchoredPosition = Vector3.zero;
        resetGoldText.text = questHolder.questResetPrice.ToString("n0");
        UpdateQuestBoardView();
        
    }
    
    public void Exit(){
        Debug.Log("Exit QuestNPC");
        if(enterPlayer!=null){
        enterPlayer.isQuestBoardOn=false;    
        uiGroup.anchoredPosition = Vector3.right*4000;
        }
    }
    public void UpdateQuestBoardView(){
        for(int i=0;i<questListOnBoard.Count;i++){
            var x = questListOnBoard[i];
            Quest newQuest = new Quest();
            newQuest = questListOnBoard[i].CreateQuest();
            switch(x.type){
                case QuestType.Normal:
                    questHolderPrefabs[i].GetComponentInChildren<Image>().color = new Color(.55f,.56f,.55f, 0.62f);
                break;
                case QuestType.Hard:
                    questHolderPrefabs[i].GetComponentInChildren<Image>().color = new Color(.87f, .87f,.43f, 0.62f);
                break;
                case QuestType.Epic:
                    questHolderPrefabs[i].GetComponentInChildren<Image>().color = new Color(.48f,.22f,.83f, 0.62f);
                break;
                case QuestType.Legendary:
                    questHolderPrefabs[i].GetComponentInChildren<Image>().color = new Color(.4f,.73f,.29f, 0.62f);
                break;
            }
            questHolderPrefabs[i].GetComponentInChildren<Text>().text = "["+questListOnBoard[i].type +"]"+"\n\n\n"+ newQuest.questName;
            questHolderPrefabs[i].GetComponent<scr_QuestBoardPrefab>().questOnSlot = questListOnBoard[i];
            var y = questHolderPrefabs[i];
            questHolderPrefabs[i].GetComponent<Button>().onClick.RemoveAllListeners();

            questHolderPrefabs[i].GetComponent<Button>().onClick.AddListener(delegate{UpdateInfoBoard(y);});
        }

    }
    public void UpdateInfoBoard(GameObject _questslot){
        Debug.Log("UpdateInfoBoard");
        var _quest = _questslot.GetComponent<scr_QuestBoardPrefab>().questOnSlot;
        enterPlayer.sfxAudio.PlayOneShot(clickSound);
        currentQuestSlot = _questslot;
        Quest newQuest = new Quest();
        newQuest = _quest.CreateQuest();
        //Tap Once
        questInfo.SetActive(true);
        questInfoTitle.text = newQuest.questName;
        questInfoDialog.text = _quest.objective;
        questInfoBero.text = newQuest.beroReward +"";
        questInfoEXP.text = newQuest.expReward +"";
        questInfoHonorPoint.text = newQuest.HonorPoint +"";
        tap++;
        Debug.Log("Singletap" + tap);
       if (tap == 1) clicktime = Time.time;
        for(int i=0;i<_quest.rewards.Length;i++){
            possibleQuestRewards[i].sprite = _quest.rewards[i].UIDisplay;
        }
        if (tap > 1 && Time.time - clicktime < clickdelay )
        {
            tap = 0;
            clicktime = 0;
            Debug.Log("DoubleClick" + tap);
            if(RecieveQuest(_quest)){
                _questslot.SetActive(false);
                questInfo.SetActive(false);
                currentQuestSlot=null;
            };
        
        }else if (tap >= 2 || Time.time - clicktime > 1) tap = 0;
    }
    public bool RecieveQuest(scr_QuestObject _quest){
        if(enterPlayer.questInventory.AddQuest(_quest.CreateQuest())){
            enterPlayer.sfxAudio.PlayOneShot(AcceptQuest);
            return true;
        }else{
            return false;
        }
    }
    public void OnClickAccept(){
        if(RecieveQuest(currentQuestSlot.GetComponent<scr_QuestBoardPrefab>().questOnSlot)){
            currentQuestSlot.SetActive(false);
            questInfo.SetActive(false);
            currentQuestSlot=null;
        }

    }
    public void OnResetQuest(){
        for(int i=0;i<6;i++){
            questHolderPrefabs[i].SetActive(true);
        }
        enterPlayer.coins-=questHolder.questResetPrice;
        questHolder.Shufflelist();
        questListOnBoard = questHolder.currentQuestList;
        UpdateQuestBoardView();
    }
    public static int GetListenerNumber(UnityEventBase unityEvent)
{
    var field = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly );
    var invokeCallList = field.GetValue(unityEvent);
    var property = invokeCallList.GetType().GetProperty("Count");
    return (int)property.GetValue(invokeCallList);
}
}
