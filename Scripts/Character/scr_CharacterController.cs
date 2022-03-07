using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DuloGames.UI;
using static scr_Models;
using TMPro;
using EZCameraShake;
using SimpleJSON;

public class scr_CharacterController : MonoBehaviour
{
    #region - Player Controller -
    
    [SerializeField] private GameObject gameCameraHolder;
    private CharacterController characterController;
    private DefaultInput defaultInput;
    [HideInInspector]
    public Vector2 input_Movement;

    [HideInInspector]
    public Vector2 input_View;
    public Vector3 newCameraRotation;
    public Vector3 newCharacterRotation;

    [Header("References")]
    public Transform cameraHolder;
    public Transform feetTransForm;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;
    public LayerMask playerMask;
    public LayerMask groundMask;
    public LayerMask borderMask;


    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    public float playerGravity;
    public float damage;

    public Vector3 jumpingForce;
    public int jumpcount;
    private Vector3 jumpingForceVelocity;
    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerStanceSmoothing;
    public CharacterStance playerStandStance;
    public CharacterStance playerCrouchStance;
    public CharacterStance playerProneStance;
    private float stanceCheckErrorMargin = 0.01f;

    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceCapsuleCenterVelocity;
    private float stanceCapsuleHeightVelocity;

    public bool cantStand;
    public bool isSprinting;

    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;

    [Header("Weapon")]
    public scr_WeaponController currentWeapon;
    public float weaponAnimationSpeed;

    // Custom boolean for two variable
    public bool isGrounded;
    [HideInInspector]
    public bool isFalling;
    [HideInInspector]
    bool isSwaping;
    bool isDamaged;
    bool isDashing;
    public bool isDead;
    public bool isBlocking;




    [Header("Aiming In")]
    public bool isAimingIn;
    public bool isShooting;

    [Header("Audio Sources - Character")]
    public AudioClip[] playerDamagedClips= default;
    public AudioClip[] defaultWalkingClips = default;
    public AudioClip[] playerBlockClips= default;
    public AudioClip playerCriticalState= default;
    public AudioSource playerCritialStateSource= default;
    public AudioClip[] drinkingPotionClip = default;



    public AudioSource characterAudio;

    [Header("FootStep")]
    public bool useFootsteps;
    public float baseStepSpeed = 0.7f;
    public float crouchStepMultiplier = 1.5f;
    public float sprintStepMultiplier = 0.9f;
    public float footstepTime =0;
    public float GetCurrentOffset => (playerStance == PlayerStance.Crouch) ? baseStepSpeed * crouchStepMultiplier:isSprinting? baseStepSpeed*sprintStepMultiplier:baseStepSpeed;

    [Header("Looting")]
    GameObject nearObject;
    public AudioClip[] defaultLootClips = default;
    public AudioClip diamondLootAudio;

    public AudioSource sfxAudio;

    [Header("Physics Problem")]
    bool isBorder;


    #endregion
    [Header("Player Status")]
    public int coins;
    public int honorPoint;
    public int diamond;
    public int currenthealth;
    public int currentWeaponAmmo;
    public int playerCurrentStamina;
    public int playerMaxStamina;
    public float playerLevel;
    float potionCoolDown;
    public float playerCurrentExp,playerRequiredExp;
    [Header("Attributes")]
    [SerializeField] public Attribute[] attributes;
    [SerializeField] Text[] attributesText;

    [Header("Weapons")]
    public GameObject equipedWeapon;
    public scr_Weapon currentEquipedStat;
    public GameObject[] weapons;
    
    scr_ItemObject currentMeleeWeaponItemObject;
    scr_ItemObject currentPistolWeaponItemObject;
    scr_ItemObject currentMainWeaponItemObject;
    GameObject currentMeleeWeapon;
    [SerializeField]scr_Weapon meleeWeaponHitBoxStat;
    GameObject currentPistolWeapon;
    GameObject currentMainWeapon;
    [SerializeField]Image currentWeaponImage;
    [SerializeField]GameObject weaponHolder;


    [Header("Menu")]
    public bool isMenuOpen;
    public bool isShop;
    
    [SerializeField]scr_ShopNPC storeUI;
    [SerializeField]scr_QuestNPC questUI;
    [SerializeField]scr_CraftingNPC craftingUI;



    [Header("Dash")]
    private float nextActionTime = 0.0f;
    private float nextDashTime = 0.0f;
    [SerializeField] ParticleSystem forwardDashParticleSystem;
    [SerializeField] ParticleSystem backwardDashParticleSystem;
    [SerializeField] ParticleSystem rightDashParticleSystem;
    [SerializeField] ParticleSystem leftDashParticleSystem;
    [SerializeField] ParticleSystem backleftDashParticleSystem;
    [SerializeField] ParticleSystem backrightDashParticleSystem;
    [SerializeField] ParticleSystem frontleftDashParticleSystem;
    [SerializeField] ParticleSystem frontrightDashParticleSystem;
    [SerializeField] AudioClip dashAudio;
    [SerializeField] ParticleSystem criticalHealthParticleSystem;

    [Header("UI")] 
    public GameObject LevelUpUI;   
    public GameObject ScrollUI;   

    public Text LevelUpText;
    public AudioClip levelupAudio;
    public Demo_Chat statusLogger;
    [SerializeField] Image[] quickSlotCooldownFill;
    [Header("Inventory / UI")]
    public bool isInventoryOn;
    public bool isQuestOn;
    public bool isQuestBoardOn;
    public bool isCraftOn;
    bool isEquipmentOn;
    public scr_QuestInventory questInventory;
    public scr_Inventory inventoryEQP;
    public scr_Inventory inventoryUSE;
    public scr_Inventory inventoryETC;
    public scr_Inventory equipment;
    public scr_Inventory quickSlot;
    [SerializeField] GameObject EQPInvTab;
    [SerializeField] GameObject USEInvTab;
    [SerializeField] GameObject ETCInvTab;
    [SerializeField] GameObject inventoryEQPDisplay;
    [SerializeField] GameObject inventoryUSEDisplay;
    [SerializeField] GameObject inventoryETCDisplay;
    [SerializeField] GameObject inventoryGroup;
    [SerializeField] GameObject equipmentGroup;
    [SerializeField] GameObject questGroup;
    [SerializeField] GameObject craftGroup;


    [Header("Networking")]
    [SerializeField] public ServerTalker server;
    [SerializeField] public string userId = "1";
    public JSONNode inventoryLoad;


    #region - Start -

    private void Start() {
        scr_Enemy.enemyDead += CheckKillQuest;

        for (int i = 0; i < attributes.Length; i++)
        {
            attributes[i].SetParent(this);
        }
        for (int i = 0; i < equipment.GetSlots.Length; i++)
        {
            equipment.GetSlots[i].OnBeforeUpdate += OnBeforeSlotUpdate;
            equipment.GetSlots[i].OnAfterUpdate += OnAfterSlotUpdate;
        }
        //Armor
        attributes[3].value.BaseValue =1;
        //Health
        attributes[4].value.BaseValue =50;
        //HealthRegen
        attributes[9].value.BaseValue = 1;
        //StaminaRegen
        attributes[10].value.BaseValue =3;
        //CriticalRate
        attributes[11].value.BaseValue =10;
        //CriticalDamage
        attributes[12].value.BaseValue =130;
        SetValueOnCharacterScreen();
        LoadUserInfo();
        LoadInventory();

    }
#region - Inventory -
    public void OnBeforeSlotUpdate(InventorySlot _slot){
        if(_slot.ItemObject == null)
            return;
        switch(_slot.parent.inventory.type){
            case InterfaceType.Inventory:
                break;
            case InterfaceType.Equipment:
                // Debug.Log(string.Concat("Removed ", _slot.ItemObject, " on ", _slot.parent.inventory.type, ", Allowed Items: ", string.Join(", ",_slot.AllowedItems)));
                for(int i = 0; i< _slot.item.buffs.Count;i++){
                    for(int j = 0; j< attributes.Length;j++){
                        if(attributes[j].type == _slot.item.buffs[i].attribute)
                            attributes[j].value.RemoveModifier(_slot.item.buffs[i]);
                    }
                }
                if(_slot.ItemObject.type == ItemType.Melee){
                    if(equipedWeapon == weapons[0]){
                        equipedWeapon = null;
                    }
                    weapons[0] = null;
                    if(currentMeleeWeapon !=null){
                        Destroy(currentMeleeWeapon);
                    }
                    
                    currentEquipedStat = null;
                    currentMeleeWeaponItemObject = null;
                    Debug.Log("Melee Unequiped");
                }else if(_slot.ItemObject.type == ItemType.Pistol){
                    if(equipedWeapon == weapons[1]){
                        equipedWeapon = null;
                    }
                    weapons[1] = null;
                    if(currentPistolWeapon !=null){
                        Destroy(currentPistolWeapon);
                    }
                    currentEquipedStat = null;
                    currentPistolWeaponItemObject= null;

                    Debug.Log("Pistol Unequiped");
                }else if(_slot.ItemObject.type == ItemType.Rifle){
                    if(equipedWeapon == weapons[2]){
                        equipedWeapon = null;
                    }
                    weapons[2] = null;
                    if(currentMainWeapon !=null){
                        Destroy(currentMainWeapon);
                    }
                    currentEquipedStat = null;
                    currentMainWeaponItemObject= null;
                    Debug.Log("Rifle Unequiped");
                }
                
                break;
            case InterfaceType.Chest:
                break;
            default:
                break;
        }
    }
    public void OnAfterSlotUpdate(InventorySlot _slot){
        if(_slot.ItemObject == null)
            return;
        switch(_slot.parent.inventory.type){
            case InterfaceType.Inventory:
                break;
            case InterfaceType.Equipment:
                server.SaveInventory(userId, "CurrentEquipment", equipment.SaveToServer());
                // Debug.Log(string.Concat("Placed ", _slot.ItemObject, " on ", _slot.parent.inventory.type, ", Allowed Items: ", string.Join(", ",_slot.AllowedItems)));
                for (int i = 0; i< _slot.item.buffs.Count;i++){
                    for(int j = 0; j< attributes.Length;j++){
                        if(attributes[j].type == _slot.item.buffs[i].attribute)
                            attributes[j].value.AddModifier(_slot.item.buffs[i]);
                    }
                }
                if(_slot.ItemObject.type == ItemType.Melee){
                    
                    weapons[0] = _slot.ItemObject.EquipedDisplay;
                    currentMeleeWeaponItemObject = _slot.ItemObject;
                    if(equipedWeapon==null){
                        if(currentPistolWeapon !=null){
                         Destroy(currentPistolWeapon);
                        }
                        if(currentMainWeapon !=null){
                            Destroy(currentMainWeapon);
                        }
                        currentMeleeWeapon = Instantiate(weapons[0],new Vector3(weaponHolder.transform.position.x,weaponHolder.transform.position.y, weaponHolder.transform.position.z) , Quaternion.identity);
                        currentMeleeWeapon.transform.parent = weaponHolder.transform;
                        currentMeleeWeapon.transform.localRotation = Quaternion.identity;
                        equipedWeapon = weapons[0];
                        meleeWeaponHitBoxStat.damage = attributes[5].value.ModifiedValue;
                        meleeWeaponHitBoxStat.timeBetweenShooting = currentMeleeWeapon.GetComponent<scr_Weapon>().timeBetweenShooting;
                        currentWeaponImage.sprite = _slot.ItemObject.UIDisplay;
                    }
                }else if(_slot.ItemObject.type == ItemType.Pistol){
                    weapons[1] = _slot.ItemObject.EquipedDisplay;
                    currentPistolWeaponItemObject = _slot.ItemObject;

                    if(equipedWeapon==null){
                        if(currentMeleeWeapon !=null){
                         Destroy(currentPistolWeapon);
                        }
                        if(currentMainWeapon !=null){
                            Destroy(currentMainWeapon);
                        }
                        currentPistolWeapon = Instantiate(weapons[1],new Vector3(weaponHolder.transform.position.x,weaponHolder.transform.position.y, weaponHolder.transform.position.z) , Quaternion.identity);
                        currentPistolWeapon.transform.parent = weaponHolder.transform;
                        currentPistolWeapon.transform.localRotation = Quaternion.identity;
                        equipedWeapon = weapons[1];
                        currentWeaponImage.sprite = _slot.ItemObject.UIDisplay;
                    }
                }else if(_slot.ItemObject.type == ItemType.Rifle){

                    
                    if(currentMeleeWeapon !=null){
                         Destroy(currentPistolWeapon);
                        }
                    if(currentPistolWeapon !=null){
                        Destroy(currentMainWeapon);
                    }
                    weapons[2] = _slot.ItemObject.EquipedDisplay;
                    currentMainWeaponItemObject = _slot.ItemObject;
                    if(equipedWeapon==null){
                        currentMainWeapon = Instantiate(weapons[2],new Vector3(weaponHolder.transform.position.x,weaponHolder.transform.position.y, weaponHolder.transform.position.z) , Quaternion.identity);
                        currentMainWeapon.transform.parent = weaponHolder.transform;
                        currentMainWeapon.transform.localRotation = Quaternion.identity;
                        equipedWeapon = weapons[2];
                        currentWeaponImage.sprite = _slot.ItemObject.UIDisplay;
                    }
                }
                break;
            case InterfaceType.Chest:
                break;
            default:
                break;
        }
    }

#endregion
    #endregion

    #region - Awake -
    private void Awake()
    {
        playerRequiredExp = playerLevel *50;
        forwardDashParticleSystem.Stop();
        backwardDashParticleSystem.Stop();
        rightDashParticleSystem.Stop();
        leftDashParticleSystem.Stop();
        criticalHealthParticleSystem.Stop();
        defaultInput = new DefaultInput();
        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.DashMovement.performed += e =>  OnStartDash(e.control.displayName.ToString());
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();
        // defaultInput.Character.Crouch.performed += e => Crouch();
        defaultInput.Character.Sprint.performed += e => toggleSprint();
        defaultInput.Character.SprintReleased.performed += e => stopSprint();
        defaultInput.Character.Interact.performed += e => Interact();
        defaultInput.Weapon.Fire2Pressed.performed += e => AimingInPressed();
        defaultInput.Weapon.Fire2Released.performed += e => AimingInReleased();
        defaultInput.Weapon.Reload.performed += e => Reload();
        defaultInput.Weapon.SwitchWeapon1.performed += e => SwitchWeapon1();
        defaultInput.Weapon.SwitchWeapon2.performed += e => SwitchWeapon2();
        defaultInput.Weapon.SwitchWeapon3.performed += e => SwitchWeapon3();
        defaultInput.UI.MainMenu.performed += e => ToggleEsc();
        defaultInput.UI.Inventory.performed += e => ToggleInventory();
        defaultInput.UI.Equipment.performed += e => ToggleEquipment();
        defaultInput.UI.Quest.performed += e => ToggleQuest();
        defaultInput.UI.QuickSlot.performed += e => UseQuickSlot(e.control.displayName.ToString());


        defaultInput.Enable();

        //Locking Cursor at the Beginning
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;

        equipedWeapon = weapons[0];
        isDamaged = false;

        if (currentWeapon)
        {
            currentWeapon.Initialize(this);
        }

        

    }
    #endregion
    #region - Update -
    private void Update()
    {
        if(!isDead ){
        SetIsGrounded();
        SetIsFalling();
        HandleFootSteps();
        UpdatePlayerStatus();
            if(!checkUIOpen()){
                CalculateView();
                ShootingInput();
            }
        }
        saveInventory();
        saveQuest();
        loadQuest();
        CalculateDamage();
        if(checkUIOpen()){
            UnlockCursor();
        }else{
            LockCursor();
        }
    }
    bool checkUIOpen(){
        return isShop || isInventoryOn || isEquipmentOn||isQuestOn||isQuestBoardOn||isCraftOn;
    }
    #endregion
    #region - Fixed Update -
    private void FixedUpdate()
    {
        if(!isDead){
            if(!isInventoryOn){
                CalculateAimingIn();

            }
        CalculateMovement();
        CalculateJump();
        CalculateDash();
        StopByWall();
        CalculateCameraHeight();
        }
    }
    #endregion
    #region - IsFalling / IsGrounded -
    private void SetIsGrounded()
    {
        // Check at feet's position in playerSettings.isGround Radius and objects in groundMask layer.
        isGrounded = Physics.CheckSphere(feetTransForm.position, playerSettings.isGroundedRadius, groundMask);
        if(isGrounded){
            jumpcount =0;
        }
        //


    }
    private void SetIsFalling()
    {

        isFalling=(!isGrounded && characterController.velocity.magnitude >= playerSettings.isFallingSpeed);

    }
    #endregion
    #region - View/ Movement -
    public void StopByWall()
    {
        isBorder = Physics.Raycast(transform.position, transform.forward, 3.5f, borderMask);
    }

    private void CalculateDash(){
        if(isDashing){
            if(Time.time - playerSettings.dashStartTime <= 0.30f ){
                
               if(input_Movement.Equals(Vector2.zero)){
                characterController.Move(transform.forward*playerSettings.dashSpeed*Time.deltaTime);
               }else{                
                if(input_Movement.Equals(Vector2.left)){
                     characterController.Move(-transform.right*playerSettings.dashSpeed*Time.deltaTime);
                     leftDashParticleSystem.Play();
                }else if(input_Movement.Equals(Vector2.right)){
                     characterController.Move(transform.right*playerSettings.dashSpeed*Time.deltaTime);
                     rightDashParticleSystem.Play();
                }else if(input_Movement.Equals(new Vector2(0f,1f))){
                     characterController.Move(transform.forward*playerSettings.dashSpeed*Time.deltaTime);
                     forwardDashParticleSystem.Play();
                }else if(input_Movement.Equals(new Vector2(0f,-1f))){
                     characterController.Move(-transform.forward*playerSettings.dashSpeed*Time.deltaTime);
                     backwardDashParticleSystem.Play();
                }else if(input_Movement.x > 0.5f && input_Movement.y > 0.5f){
                    characterController.Move((transform.forward+transform.right)*playerSettings.dashSpeed*Time.deltaTime);
                    frontrightDashParticleSystem.Play();
                }else if(input_Movement.x < -0.5f && input_Movement.y > 0.5f){
                    characterController.Move((transform.forward-transform.right)*playerSettings.dashSpeed*Time.deltaTime);
                    frontleftDashParticleSystem.Play();
                }else if(input_Movement.x >0.5f && input_Movement.y < -0.5f){
                    characterController.Move((-transform.forward+transform.right)*playerSettings.dashSpeed*Time.deltaTime);
                    backrightDashParticleSystem.Play();
                }else if(input_Movement.x < -0.5f && input_Movement.y < -0.5f){
                    characterController.Move((-transform.forward-transform.right)*playerSettings.dashSpeed*Time.deltaTime);
                    backleftDashParticleSystem.Play();
                }
               }
            }else{
                    OnEndDash();
            }
            }


    }
    void OnStartDash(string direction){
        if(!isDashing && (Time.time > nextDashTime) && playerCurrentStamina >= 5){
            playerCurrentStamina -= 5;
            nextDashTime = Time.time + .8f;
            characterAudio.PlayOneShot(dashAudio);
            playerSettings.dashDirection = direction;
            isDashing = true;
            playerSettings.dashStartTime =Time.time;
        }
    }
    void OnEndDash(){
        isDashing = false;
        playerSettings.dashStartTime =0;
        playerSettings.dashDirection = "";
    }
    void PlayDashParticles(){

    }
    private void CalculateView()
    {

        // Update Camera X
        newCharacterRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newCharacterRotation);
        // Update Camera Y
        newCameraRotation.x += playerSettings.ViewYSensitivity * (playerSettings.ViewXInverted ? input_View.y : -input_View.y) * Time.deltaTime;
        // Restrict Rotation
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);
        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }
    private void CalculateMovement()
    {

            var verticalSpeed = playerSettings.WalkingForwardSpeed;
            var horizontalSpeed = playerSettings.WalkingStrafeSpeed;

            if (isSprinting)
            {
                verticalSpeed = playerSettings.RunningForwardSpeed;
                horizontalSpeed = playerSettings.RunningStrafeSpeed;
            }
            if (!isGrounded)
            {
                playerSettings.SpeedEffector = playerSettings.FallingSpeedEffector;
            }
            else if (playerStance == PlayerStance.Crouch)
            {
                playerSettings.SpeedEffector = playerSettings.CrouchSpeedEffector;
            }
            else if (playerStance == PlayerStance.Prone)
            {
                playerSettings.SpeedEffector = playerSettings.ProneSpeedEffector;
            }
            else
            {
                playerSettings.SpeedEffector = 1f;
            }
            //Weapon Animation speed will depend on character velocity
            weaponAnimationSpeed = characterController.velocity.magnitude / playerSettings.WalkingForwardSpeed * playerSettings.SpeedEffector;
            if (weaponAnimationSpeed > 1)
            {
                weaponAnimationSpeed = 1;
            }


            //Effectors
            verticalSpeed *= playerSettings.SpeedEffector;
            horizontalSpeed *= playerSettings.SpeedEffector;
        //Checking if Border is ahead inputMovement.y�� �յ� x�� ����
        if (isBorder && input_Movement.y > 0) {
             newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, new Vector3(horizontalSpeed * input_Movement.x * Time.deltaTime, 0, 0), ref newMovementSpeedVelocity, isGrounded ? playerSettings.MovementSmoothing : playerSettings.FallingSmoothing);
        }
        else {
                newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, new Vector3(horizontalSpeed * input_Movement.x * Time.deltaTime, 0, verticalSpeed * input_Movement.y * Time.deltaTime), ref newMovementSpeedVelocity, isGrounded ? playerSettings.MovementSmoothing : playerSettings.FallingSmoothing);
            }
        //Make movement relative to PlayerRotation
        var movementSpeed = transform.TransformDirection(newMovementSpeed);


            //Gravity Conditions for Jumping
            if (playerGravity > gravityMin)
            {
                playerGravity -= gravityAmount * Time.deltaTime;
            }

            //prevent Glitch
            if (playerGravity < -0.1f && isGrounded)
            {
                playerGravity = -0.1f;
            }

            movementSpeed.y += playerGravity;
            movementSpeed += jumpingForce * Time.deltaTime;
            characterController.Move(movementSpeed);
    }
    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
    }

    private void CalculateCameraHeight()
    {
        var currentstance = playerStandStance;

        if (playerStance == PlayerStance.Crouch)
        {
            currentstance = playerCrouchStance;
        }


        cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, currentstance.Cameraheight, ref cameraHeightVelocity, playerStanceSmoothing);
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, cameraHeight, cameraHolder.localPosition.z);

        characterController.height = Mathf.SmoothDamp(characterController.height, currentstance.StanceCollider.height, ref stanceCapsuleHeightVelocity, playerStanceSmoothing);
        characterController.center = Vector3.SmoothDamp(characterController.center, currentstance.StanceCollider.center, ref stanceCapsuleCenterVelocity, playerStanceSmoothing);

    }

    #endregion
    #region - Jumping -
    private void Jump()
    {
        //check grounded
        if (jumpcount >=1 )
        {
            return;
        }
        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                return;
            }
            playerStance = PlayerStance.Stand;
            return;
        }
        //Jump
        jumpcount ++;
        jumpingForce = Vector3.up * playerSettings.JumpingHeight;
        playerGravity = 0;
        currentWeapon.TriggerJump();


    }
    #endregion
    #region - Stance -
    private void Crouch()
    {
        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                return;
            }
            playerStance = PlayerStance.Stand;
            return;

        }

        if (StanceCheck(playerCrouchStance.StanceCollider.height))
        {
            return;
        }

        playerStance = PlayerStance.Crouch;
    }
    private void Prone()
    {
        if (playerStance == PlayerStance.Prone)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                return;
            }
            playerStance = PlayerStance.Stand;
            return;
        }

        playerStance = PlayerStance.Prone;
    }
    private bool StanceCheck(float stanceCheckHeight)
    {
        var start = new Vector3(feetTransForm.position.x, feetTransForm.position.y + characterController.radius + stanceCheckErrorMargin, feetTransForm.position.z);
        var end = new Vector3(feetTransForm.position.x, feetTransForm.position.y - characterController.radius - stanceCheckErrorMargin + stanceCheckHeight, feetTransForm.position.z);

        return Physics.CheckCapsule(start, end, characterController.radius, playerMask);
    }
    #endregion
    #region - Sprinting -
    private void toggleSprint()
    {
        if (!isGrounded)
        {
            return;
        }
        isSprinting = !isSprinting;
    }
    private void stopSprint()
    {
        isSprinting = false;
    }
    #endregion
    #region - FootSteps -
    public void JumpSound()
    {
        characterAudio.PlayOneShot(defaultWalkingClips[Random.Range(0, defaultWalkingClips.Length)]);
        footstepTime = GetCurrentOffset;
    }
    public void HandleFootSteps()
    {
        if (!isGrounded)
        {
            return;
        }
        if (input_Movement == Vector2.zero) {
            return;
        }

        footstepTime -= Time.deltaTime;
        if(footstepTime <= 0)
        {
            if (Physics.Raycast(cameraHolder.transform.position,Vector3.down,out RaycastHit hit, 3))
            {
                switch (hit.collider.tag)
                {
                    case "Ground/Wood":
                        break;
                    case "Ground/Metal":
                        break;
                    case "Ground/Sand":
                        break;
                    case "Ground/Water":
                        break;
                    default:
                        characterAudio.PlayOneShot(defaultWalkingClips[Random.Range(0,defaultWalkingClips.Length)]);
                        break;
                }
            }
            footstepTime = GetCurrentOffset;
        }

    }
    #endregion
    #region - Gizmos -
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(feetTransForm.position, playerSettings.isGroundedRadius);
    }
    #endregion
    #region - Aiming In-
    private void AimingInPressed()
    {
        if(!isShop){
        isAimingIn = true;
        if(equipedWeapon==weapons[0]){
            isBlocking = true;
        }
        }
    }
    private void AimingInReleased()
    {
        isAimingIn = false;
        if(equipedWeapon==weapons[0]){
            isBlocking = false;
        }
    }
    private void CalculateAimingIn()
    {
        if (!currentWeapon )
        {
            return;
        }
        currentWeapon.isAimingIn = isAimingIn;


    }
    #endregion
    #region - Reload / Fire -
    private void ShootingInput()
    {
        isShooting = Input.GetKey(KeyCode.Mouse0);

        if (isShooting && !isShop)
        {
            Shoot();
        }
        if(currentEquipedStat!=null){
         currentWeaponAmmo = currentEquipedStat.bulletsLeft;
        }
         if (currentWeaponAmmo == 0 &&!isSwaping){
            Reload();
            return;
         }

    }
    
    private void Reload()
    {
        if(equipedWeapon !=null)
        currentWeapon.Reload();
    }
    private void Shoot()
    {
        if(equipedWeapon !=null)
        currentWeapon.Attack();

    }
    void CalculateDamage(){
        if(equipedWeapon!= null){
            if(equipedWeapon.GetComponent<scr_Weapon>().type ==scr_Weapon.Type.Range){
               
                damage = attributes[1].value.ModifiedValue*0.5f+attributes[5].value.ModifiedValue;
                damage *= currentEquipedStat.damage ;
            }else{
                damage = attributes[0].value.ModifiedValue*0.5f+attributes[5].value.ModifiedValue;
                damage *= currentEquipedStat.damage ;
            }
        }else{
            damage =0 ;
        }
    }


    #endregion
    #region - Level - 
    public void GainExp(int x){
        statusLogger.ReceiveChatMessage(1,"You have Gained Exp: " + x);
        playerCurrentExp+= x;
        server.UpdateUser(userId, "exp", playerCurrentExp.ToString());
        if (playerCurrentExp > playerRequiredExp){
            LevelUp((int)(playerCurrentExp-playerRequiredExp));
        }

    }
    void LevelUp(int carriedExp){
        statusLogger.ReceiveChatMessage(1,"You have Leveled up: " + playerLevel);
        playerCurrentExp =carriedExp;
        playerLevel ++;
        server.UpdateUser(userId, "level", playerLevel.ToString());
        playerRequiredExp = playerLevel *50;
        characterAudio.PlayOneShot(levelupAudio);
        StartCoroutine(LevelUpUIShowAndHide());
    }
    IEnumerator LevelUpUIShowAndHide(){
        LevelUpText.text = "Level " + playerLevel;
        LevelUpUI.GetComponent<RectTransform>().anchoredPosition = Vector3.down * 150;
        LevelUpUI.GetComponentInChildren<UIParticleSystem>().Play();
        yield return new WaitForSeconds(5f);
         LevelUpUI.GetComponent<RectTransform>().anchoredPosition = Vector3.up*1000;
    }
    public IEnumerator ScrollUIShowAndHide(){
        ScrollUI.GetComponent<RectTransform>().anchoredPosition = Vector3.down * 150;
        ScrollUI.GetComponentInChildren<UIParticleSystem>().Play();
        yield return new WaitForSeconds(2f);
         ScrollUI.GetComponent<RectTransform>().anchoredPosition = Vector3.up*1000;
    }

    #endregion
    #region - Loot -
    public void GainCoin(int coin_val)
    {
        coins += coin_val;
        server.UpdateUser(userId, "bero", coins.ToString());

    }
    public void UseCoin(int coin_val)
    {
        coins -= coin_val;
        server.UpdateUser(userId, "bero", coins.ToString());
    }
    public void GainDiamond(int value)
    {
        diamond += value;
        server.UpdateUser(userId, "diamond", diamond.ToString());
    }
    public void UseDiamond(int value)
    {
        diamond -= value;
        server.UpdateUser(userId, "diamond", diamond.ToString());
    }
    public void GainHonor(int value)
    {
        honorPoint += value;
        server.UpdateUser(userId, "honorPoint", honorPoint.ToString());

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Coin")
        {
            scr_ItemController item = other.GetComponent<scr_ItemController>();
            var coin_val = Random.Range(item.value,item.value*2);
            statusLogger.ReceiveChatMessage(1,"Gained: " + coin_val + " Bero");
            GainCoin(coin_val);
            sfxAudio.PlayOneShot(defaultLootClips[Random.Range(0, defaultLootClips.Length)]);
            Destroy(other.gameObject);
            return;
        }
        else if(other.tag == "Item/Diamond"){
            scr_ItemController item = other.GetComponent<scr_ItemController>();
            statusLogger.ReceiveChatMessage(1,"Gained: " + item._name);
            GainDiamond(1);
            sfxAudio.PlayOneShot(diamondLootAudio);
            Destroy(other.gameObject);
            return;
        }

        var grounditem = other.GetComponent<GroundItem>();
        if(grounditem){
            playerAddItemToInv(grounditem.item.CreateItem());
            Destroy(other.gameObject);
            
        }else if (other.tag == "EnemyProjectile")
        {
            scr_EnemyHitbox enemyBullet = other.GetComponent<scr_EnemyHitbox>();
            if (!isDamaged && !isBlocking)
            {
                if(currenthealth - enemyBullet.damage <= 0){
                    currenthealth = 0;
                }else{
                    currenthealth -= enemyBullet.damage;
                }
                Vector3 knockBack =  transform.position- other.transform.position;
                if (other.GetComponent<Rigidbody>() != null)
                {
                    Destroy(other.gameObject);
                }
                StartCoroutine(OnDamage(knockBack));
            }else if(!isDamaged && isBlocking){
                CameraShaker.Instance.ShakeOnce(2f,2f,0.1f,1f);
                characterAudio.PlayOneShot(playerBlockClips[Random.Range(0, playerBlockClips.Length )]);
                Debug.Log("Blocked");
                  if(currenthealth - enemyBullet.damage/2 <= 0){
                    currenthealth = 0;
                }else{
                    currenthealth -= enemyBullet.damage/2;
                }
                //Playblock sound
            }
        }
    }
    private void OnCollisionEnter(Collision other) {

    }
    private void OnCollisionStay(Collision other) {
    if (other.gameObject.tag == "Enemy")
        {
            scr_Enemy enemyBullet = other.gameObject.GetComponent<scr_Enemy>();
            if (!isDamaged && !isBlocking)
            {
                if(currenthealth - enemyBullet.collisionDamage <= 0){
                    currenthealth = 0;
                }else{
                    currenthealth -= enemyBullet.collisionDamage;
                }
                Vector3 knockBack =  transform.position- other.transform.position;
                StartCoroutine(OnDamage(knockBack));
            }else if(!isDamaged && isBlocking){
                CameraShaker.Instance.ShakeOnce(2f,2f,0.1f,1f);
                characterAudio.PlayOneShot(playerBlockClips[Random.Range(0, playerBlockClips.Length )]);
                Debug.Log("Blocked");
                  if(currenthealth - enemyBullet.collisionDamage/2 <= 0){
                    currenthealth = 0;
                }else{
                    currenthealth -= enemyBullet.collisionDamage/2;
                }
                //Playblock sound
            }
        }
    }
    public void playerAddItemToInv(Item _item){
        switch(_item.itemType){
                case ItemType.ETC:
                    sfxAudio.PlayOneShot(defaultLootClips[Random.Range(0, defaultLootClips.Length)]);
                    if(inventoryETC.AddItem(_item,1))
                    statusLogger.ReceiveChatMessage(1,"Gained: " + _item.Name);
                    server.SaveInventory(userId, "ETCInventory", inventoryETC.SaveToServer());
                break;
                case ItemType.USE:
                    sfxAudio.PlayOneShot(defaultLootClips[Random.Range(0, defaultLootClips.Length)]);
                    if(inventoryUSE.AddItem(_item,1))
                    statusLogger.ReceiveChatMessage(1,"Gained: " + _item.Name);
                    server.SaveInventory(userId, "USEInventory", inventoryUSE.SaveToServer());
                break;
                case ItemType.Potion:
                    sfxAudio.PlayOneShot(defaultLootClips[Random.Range(0, defaultLootClips.Length)]);
                    if(inventoryUSE.AddItem(_item,1))
                    statusLogger.ReceiveChatMessage(1,"Gained: " + _item.Name);
                    server.SaveInventory(userId, "USEInventory", inventoryUSE.SaveToServer());
                break;
                case ItemType.WeaponScroll:
                    sfxAudio.PlayOneShot(defaultLootClips[Random.Range(0, defaultLootClips.Length)]);
                    if(inventoryUSE.AddItem(_item,1))
                    statusLogger.ReceiveChatMessage(1,"Gained: " + _item.Name);
                    server.SaveInventory(userId, "USEInventory", inventoryUSE.SaveToServer());
                break;
                default:
                    sfxAudio.PlayOneShot(defaultLootClips[Random.Range(0, defaultLootClips.Length)]);
                    if(inventoryEQP.AddItem(_item,1))
                    statusLogger.ReceiveChatMessage(1,"Gained: " + _item.Name);
                    server.SaveInventory(userId, "EQPInventory", inventoryEQP.SaveToServer());

                break;
            }
    }
    #endregion 
    #region - Monster Interaction / Quest -
    void CheckKillQuest(scr_Enemy enemy){
        StartCoroutine(OnCheckQuest(enemy.enemyName));
        
    } 
    IEnumerator OnCheckQuest(string enemyName){
        for(int i=0;i<questInventory.GetQuestSlots.Length;i++){
            var questSlot = questInventory.GetQuestSlots[i];
             if(questSlot.quest.Id >=0){
                if(questSlot.quest.monsterName.Equals(enemyName) && questSlot.QuestObject.monsterkillneed>questInventory.GetQuestSlots[i].quest.monsterKillCount){
                    questSlot.quest.monsterKillCount++;
                }else if(questSlot.quest.monsterKillCount == questSlot.QuestObject.monsterkillneed){
                    questSlot.quest.questCompleteStatus = true;
                    statusLogger.ReceiveChatMessage(1,"<Color='#107896'>Quest Complete: " + questSlot.quest.questName +"</Color>");
                }
            }
        }
        yield return null;
    }
    IEnumerator OnDamage(Vector3 knockBack)
    {
        CameraShaker.Instance.ShakeOnce(4f,4f,0.1f,1f);
        isDamaged = true;
        characterAudio.PlayOneShot(playerDamagedClips[Random.Range(0, playerDamagedClips.Length)]);
        knockBack = knockBack.normalized;
        knockBack += Vector3.up;
        transform.position += knockBack;
        yield return new WaitForSeconds(0.45f);
       isDamaged = false;
    }
    #endregion
    #region - Switching Weapon -
    private void SwitchWeapon1()
    {
        if(weapons[0]!=null && equipedWeapon != weapons[0] && isSwaping == false && !isAimingIn) {
            currentWeapon.weaponAnimator.SetTrigger("Swap");
            equipedWeapon = weapons[0];
            equipedWeapon.SetActive(true);
            if(currentPistolWeapon !=null){
                Destroy(currentPistolWeapon);
            }
            if(currentMainWeapon !=null){
                 Destroy(currentMainWeapon);
            }
            currentWeaponImage.sprite = currentMeleeWeaponItemObject.UIDisplay;
            currentMeleeWeapon = Instantiate(weapons[0],new Vector3(weaponHolder.transform.position.x,weaponHolder.transform.position.y, weaponHolder.transform.position.z) , Quaternion.identity);
            currentMeleeWeapon.transform.parent = weaponHolder.transform;
            currentMeleeWeapon.transform.localRotation = Quaternion.identity;
            meleeWeaponHitBoxStat.damage = attributes[5].value.ModifiedValue;
            meleeWeaponHitBoxStat.timeBetweenShooting = currentMeleeWeapon.GetComponent<scr_Weapon>().timeBetweenShooting;
            currentEquipedStat = equipedWeapon.GetComponent<scr_Weapon>();
            isSwaping = true;
            Invoke("SwapOut", 0.3f);
            currentWeapon.weaponAnimator.Play("Walking");
        }
    }
    private void SwitchWeapon2()
    {
        if (weapons[1]!=null && equipedWeapon != weapons[1] && isSwaping == false && !isAimingIn)
        {
            currentWeapon.weaponAnimator.SetTrigger("Swap");
            equipedWeapon = weapons[1];
            equipedWeapon.SetActive(true);

            if(currentMeleeWeapon !=null){
                Destroy(currentMeleeWeapon);
            }
            if(currentMainWeapon !=null){
                 Destroy(currentMainWeapon);
            }
            currentWeaponImage.sprite = currentPistolWeaponItemObject.UIDisplay;
            currentPistolWeapon = Instantiate(weapons[1],new Vector3(weaponHolder.transform.position.x,weaponHolder.transform.position.y, weaponHolder.transform.position.z) , Quaternion.identity);
            currentPistolWeapon.transform.parent = weaponHolder.transform;
            currentPistolWeapon.transform.localRotation = Quaternion.identity;
            equipedWeapon = weapons[1];
            currentEquipedStat = equipedWeapon.GetComponent<scr_Weapon>();
            isSwaping = true;
            Invoke("SwapOut", 0.3f);
            currentWeapon.weaponAnimator.Play("Walking");

        }
    }
    private void SwitchWeapon3()
    {
        if (weapons[2]!=null && equipedWeapon != weapons[2] && isSwaping == false && !isAimingIn)
        {
            currentWeapon.weaponAnimator.SetTrigger("Swap");
            equipedWeapon = weapons[2];
            equipedWeapon.SetActive(true);
            if(currentMeleeWeapon !=null){
                Destroy(currentMeleeWeapon);
            }
            if(currentPistolWeapon !=null){
                 Destroy(currentPistolWeapon);
            }
            currentWeaponImage.sprite = currentMainWeaponItemObject.UIDisplay;
            currentMainWeapon = Instantiate(weapons[2],new Vector3(weaponHolder.transform.position.x,weaponHolder.transform.position.y, weaponHolder.transform.position.z) , Quaternion.identity);
            currentMainWeapon.transform.parent = weaponHolder.transform;
            currentMainWeapon.transform.localRotation = Quaternion.identity;
            equipedWeapon = weapons[2];
            currentEquipedStat = equipedWeapon.GetComponent<scr_Weapon>();
            isSwaping = true;
            Invoke("SwapOut", 0.3f);
            currentWeapon.weaponAnimator.Play("Walking");
        }
    }
    void SwapOut()
    {
        isSwaping = false;
    }

    #endregion
    #region - UI -

public void LockCursor(){
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}
public void UnlockCursor(){
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}

public void ToggleInventory(){
    if(!isInventoryOn){
        inventoryGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero ;
        isInventoryOn = true;
    }else{
        inventoryGroup.GetComponent<RectTransform>().anchoredPosition += Vector2.up * 1500;
        isInventoryOn = false;
    }
}
void ToggleEquipment(){
    if(!isEquipmentOn){
        equipmentGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(-925,375);
        isEquipmentOn = true;
    }else{
        equipmentGroup.GetComponent<RectTransform>().anchoredPosition += Vector2.up * 1500;
        isEquipmentOn = false;
    }
}
void ToggleQuest(){
    if(!isQuestOn){
        questGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);
        isQuestOn = true;
    }else{
        questGroup.GetComponent<RectTransform>().anchoredPosition += Vector2.down * 1000;
        isQuestOn = false;
    }
}
void ToggleCraft(){
    if(!isCraftOn){
        craftGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);
        isCraftOn = true;
    }else{
        craftGroup.GetComponent<RectTransform>().anchoredPosition += Vector2.left * 2000;
        isCraftOn = false;
        craftingUI.Exit();
    }
}
void ToggleEsc(){
    if(isInventoryOn){
        ToggleInventory();
        return;
    }else if(isShop){
        storeUI.Exit();
        return;
    }else if(isEquipmentOn){
        ToggleEquipment();
        return;
    }else if(isQuestOn){
        ToggleQuest();
        return;
    }else if(isQuestBoardOn){
        questUI.Exit();
        return;
    }else if(isCraftOn){
        ToggleCraft();
        return;
    }
    else if(isMenuOpen){
        UnlockCursor();
        
    }else{
        LockCursor();
    }
   
    isMenuOpen = !isMenuOpen;
}
    #endregion
    #region - Interact -
    
void Interact(){
    if(nearObject!=null){
        if (nearObject.tag =="Shop"){
            scr_ItemHolder interactZone = nearObject.GetComponent<scr_ItemHolder>();
            storeUI.itemList = interactZone.itemlist;
            storeUI.instantiateItemList();
            storeUI.Enter(this);
        }else if (nearObject.tag == "QuestNPC"){
            scr_QuestHolder interactZone = nearObject.GetComponent<scr_QuestHolder>();
            questUI.questListOnBoard = interactZone.currentQuestList;
            questUI.questHolder = interactZone;
            questUI.Enter(this);
        }else if (nearObject.tag == "CraftingNPC"){
           ToggleCraft();
           Debug.Log("Enter Craft");

            
        
        }
}
}
void OnTriggerStay(Collider other){
    if(other.tag == "Shop" || other.tag =="QuestNPC"||other.tag=="CraftingNPC"){
        nearObject = other.gameObject;
    }
}


    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Coin")
        {
            nearObject = null;
        }else if (other.tag == "Shop"){
            storeUI.Exit();
            nearObject = null;
        }else if(other.tag == "QuestNPC"){
            questUI.Exit();
            nearObject = null;

        }else if(other.tag == "CraftNPC"){
            if(isCraftOn){
                ToggleCraft();
            }
            nearObject = null;

        }
    }
    #endregion
    #region - Update Player Status -
    
    public void usePotion(InventorySlot potionInSlot){
        if(potionInSlot.ItemObject != null){
            if(Time.time > potionCoolDown){
                
                potionCoolDown = Time.time + 3f;
                if(currenthealth+ potionInSlot.ItemObject.data.value < attributes[4].value.ModifiedValue){
                    currenthealth += potionInSlot.ItemObject.data.value;
                }else{
                    currenthealth = attributes[4].value.ModifiedValue;
                }
                characterAudio.PlayOneShot(drinkingPotionClip[Random.Range(0, drinkingPotionClip.Length)]);
                potionInSlot.amount -= 1;
                if(potionInSlot.amount <1){
                    potionInSlot.RemoveItem();
                    potionInSlot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text ="";
                    return;
                }
                potionInSlot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text = potionInSlot.amount ==1? "" : potionInSlot.amount.ToString("n0");
            }else{
                statusLogger.ReceiveChatMessage(1,"Potion is on CoolDown");   
            }
        }
    }
    void CheckCoolDownPotion(){
        if(potionCoolDown-Time.time>0){
            var potionCoolDownRatio = (potionCoolDown-Time.time)/3;
            for(int i=0;i<quickSlot.GetSlots.Length;i++){
                if(quickSlot.GetSlots[i].ItemObject!=null){
                    if(quickSlot.GetSlots[i].ItemObject.type == ItemType.Potion){
                        quickSlotCooldownFill[i].fillAmount = potionCoolDownRatio;
                    }
                }else{
                    quickSlotCooldownFill[i].fillAmount = 0;
                }
            }
        }
    }
    void UpdatePlayerStatus(){
        WeaponUpdate();
        Regenerate();
        CheckPlayerStatus();
        CheckCoolDownPotion();
        OnToggleInventory(true);
        }
    void WeaponUpdate(){
        if(equipedWeapon!=null){
            currentEquipedStat = equipedWeapon.GetComponent<scr_Weapon>();
        }
    }
    void Regenerate(){
        if(Time.time > nextActionTime){
            nextActionTime += 3f;
            if(playerCurrentStamina+5<playerMaxStamina){
                playerCurrentStamina +=  attributes[10].value.ModifiedValue;
            }else{
                playerCurrentStamina = playerMaxStamina;
            }
            if(currenthealth <(int)attributes[4].value.ModifiedValue){
                currenthealth +=  attributes[9].value.ModifiedValue;
            }else{
                currenthealth =(int)attributes[4].value.ModifiedValue;
            }
        }
    }
    
    
    void CheckPlayerStatus()
    {
        if(((float)currenthealth/(int)attributes[4].value.ModifiedValue)<0.4f){
            if(playerCritialStateSource.isPlaying == false){
                playerCritialStateSource.PlayOneShot(playerCriticalState);
            }
            criticalHealthParticleSystem.Play();
        }else{
             if(playerCritialStateSource.isPlaying == true){
                playerCritialStateSource.Stop();
            }
            criticalHealthParticleSystem.Stop();

        }
        if(currenthealth <=0){
            onDeath();
        }
    }


    #endregion
    #region  -Death-
    public void onDeath(){
        gameCameraHolder.GetComponent<Animator>().SetTrigger("IsDead");
        isDead = true; 
        Invoke("onRevive",10);
    }
    void onRevive(){
        currenthealth = 50;
        gameCameraHolder.GetComponent<Animator>().SetTrigger("IsAlive");
        isDead =false;
        //Revive Location
        this.transform.localPosition = new Vector3(141,30,-147);
        this.transform.localRotation = Quaternion.identity;

    }
    #endregion
    #region - Inventory -

    void UseQuickSlot(string x){
        Debug.Log(x);
        switch(x){
            case "F1":
                usePotion(quickSlot.GetSlots[0]);
                break;
            case "F2":
                usePotion(quickSlot.GetSlots[1]);
                break;
            case "F3":
                usePotion(quickSlot.GetSlots[2]);
                break;
            case "F4":
                usePotion(quickSlot.GetSlots[3]);
                break;
            case "F5":
                usePotion(quickSlot.GetSlots[4]);
                break;
            case "F6":
                usePotion(quickSlot.GetSlots[5]);
                break;
            case "F7":
                usePotion(quickSlot.GetSlots[6]);
                break;
            case "F8":
                usePotion(quickSlot.GetSlots[7]);
                break;
        }
    }
    public void OnToggleInventory(bool state)
    {
        var boolEQP = EQPInvTab.GetComponent<UITab>().isOn;
        var boolUSE = USEInvTab.GetComponent<UITab>().isOn;
        var boolETC = ETCInvTab.GetComponent<UITab>().isOn;

        if(boolEQP){
            inventoryEQPDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200,0);
            inventoryUSEDisplay.GetComponent<RectTransform>().anchoredPosition += Vector2.up*1000;
            inventoryETCDisplay.GetComponent<RectTransform>().anchoredPosition += Vector2.up*1000;

        }else if(boolUSE){

            inventoryEQPDisplay.GetComponent<RectTransform>().anchoredPosition += Vector2.up*1000;
            inventoryUSEDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200,0);
            inventoryETCDisplay.GetComponent<RectTransform>().anchoredPosition += Vector2.up*1000;
        }else if(boolETC){

            inventoryEQPDisplay.GetComponent<RectTransform>().anchoredPosition += Vector2.up*1000;
            inventoryUSEDisplay.GetComponent<RectTransform>().anchoredPosition += Vector2.up*1000;
            inventoryETCDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200,0);
        }
    }

    private void OnApplicationQuit(){
        inventoryEQP.Clear();
        inventoryUSE.Clear();
        inventoryETC.Clear();
        equipment.Clear();
        quickSlot.Clear();
        questInventory.Clear();
    }
    private void saveInventory(){
        if(Input.GetKeyDown(KeyCode.Delete)){
            server.SaveInventory(userId, "QuickSlot", quickSlot.SaveToServer());
        }
    }
    private void LoadInventory(){
        server.LoadInventory(userId, "EQPInventory");
        server.LoadInventory(userId, "USEInventory");
        server.LoadInventory(userId, "ETCInventory");
        server.LoadInventory(userId, "CurrentEquipment");
        server.LoadInventory(userId, "QuickSlot");
    }
    private void saveQuest(){
        if(Input.GetKeyDown(KeyCode.Insert)){
            Debug.Log("SavedQuest");
            questInventory.SaveQuest();
        }
    }
    private void loadQuest(){
        if(Input.GetKeyDown(KeyCode.Home)){
            Debug.Log("LoadedQuest");
            questInventory.LoadQuest();
        }
    }

    #endregion
    #region - Attribute - 
    public void AttributeModified(Attribute attribute){
    //    Debug.Log(string.Concat(attribute.type, " was updated! Value is now " , attribute.value.ModifiedValue ));
        SetValueOnCharacterScreen();
    }
    public void SetValueOnCharacterScreen(){
        for(int i =0; i < attributes.Length; i++){
            if(attributes[i].type == Attributes.CriticalRate ||attributes[i].type == Attributes.CriticalDamage){
            attributesText[i].text = attributes[i].value.ModifiedValue+"%";
            }else{
            attributesText[i].text = attributes[i].value.ModifiedValue+"";
            }
        }

    }
    #endregion


    #region - Networking -
    public void getUserId()
    {
        //Set User Id with Steam related Value;
    }
    public void LoadUserInfo()
    {
        server.GetUserByID(userId);


    }
    #endregion
}

[System.Serializable]
public class Attribute{
    [System.NonSerialized]
    public scr_CharacterController parent;
    public Attributes type;
    public scr_ModifiableInt value;
    public void SetParent(scr_CharacterController _parent)
    {
        parent = _parent;
        value = new scr_ModifiableInt(AttributeModified);
    }
    public void AttributeModified(){
        parent.AttributeModified(this);
    }

}