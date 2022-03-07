using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Knife.Effects;

using static scr_Models;

public class scr_WeaponController : MonoBehaviour
{

    private scr_CharacterController characterController;
    [Header("References")]
    public Animator weaponAnimator;
    [SerializeField] Camera playercamera;

    [Header("Settings")]
    public WeaponSettingsModel settings;
    bool isInitialized;
    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;
    
    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

    Vector3 newWeaponMovementRotation;
    Vector3 newWeaponMovementRotationVelocity;

    Vector3 targetWeaponMovementRotation;
    Vector3 targetWeaponMovementRotationVelocity;

    private bool isGroundedTrigger;
    public float fallingDelay;

    [Header("Weapon Idle Sway")]
    public Transform weaponSwayObject;
    public float swayAmountA=1;
    public float swayAmountB=2;
    public float swayScale = 600;
    public float swayLerpSpeed = 14;

    float swayTime;
    Vector3 swayPosition;
    [HideInInspector]
    public bool isAimingIn;

    [Header("Sights")]
    public Transform sightTarget;
    public float sightOffset;
    public float aimingInTime;
    private Vector3 weaponSwayPosition;
    private Vector3 weaponSwayPositionVelocity;

    //[Header("Weapon Stats")]
    //public int damage;
    //public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    //public int magazineSize, bulletPerTap;
    //public bool allowButtonHold;
    //int bulletsLeft, bulletsShot;

    ////bools
    public bool shooting, readyToShoot;
    public bool reloading = false;

    //Reference
    public GameObject Sight;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;
    public Camera weaponCamera;
    [SerializeField]GameObject bullet;

    //Graphics
    [SerializeField]ParticleGroupEmitter[] shotEmitters;
    public GameObject hitEffect;

    public BoxCollider meleeCollider;

    [Header("Audio Sources - Weapon")]
    public AudioClip[] reloadClip;
    public AudioSource weaponAudio;


    public scr_Weapon equipedWeaponStats;
    [Header("Recoil")]
    private scr_Recoil Recoil_Script;
    Vector3 originalRotation;


    [Header("Enemy Health Indicator")]
    public RectTransform enemyHealthGroup;
    public RectTransform enemyHealthBar;
    public Text enemyHealthBarText;
    public Text enemyLevelText;



    public Text enemyName;
    scr_Enemy enemy;

//Event
    public delegate void EnemyDamaged();
    
    public delegate void EnemyDead();
    public static event EnemyDamaged enemyDamaged;
    public static event EnemyDamaged enemyDead;
    #region - Start -
    private void Start()
    {
        Recoil_Script = transform.Find("WeaponSway").GetComponent<scr_Recoil>();
        newWeaponRotation = transform.localRotation.eulerAngles;
        //bullets
        scr_Enemy.enemyDamaged += HealthBarDisplay;
        scr_Enemy.enemyDead += enemyKilled;
    }
    #endregion
    #region - Awake -
    private void Awake()
    {     
        
        readyToShoot = true;
    }
    #endregion
    #region - Update -
    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        CalculateWeaponRotation();
        SetWeaponAnimations();
        CalculateWeaponSway();
        CalculateAimingIn();
        UpdateWeapon();
        

    }
    #endregion
    #region - Weapon Sway / Weapon Rotation -

    public void Initialize(scr_CharacterController CharacterController)
    {
        characterController = CharacterController;
        isInitialized = true;
    }

    public void SetFov(float fov)
        {
            weaponCamera.fieldOfView = fov;
            // WeaponCamera.fieldOfView = fov * WeaponFovMultiplier;
            // Debug.Log("Resetting Fov");
        }
    private void CalculateAimingIn()
    {
        var targetPosition = transform.position;
        // if (characterController.isSprinting)
        // {
        //     return;
        // }
        if (isAimingIn)
        {
            weaponAnimator.SetBool("IsAiming", true);
            settings.MovementSwayX = 1f;
            settings.MovementSwayY = 1f;
            targetPosition = characterController.cameraHolder.transform.position + (weaponSwayObject.transform.position - sightTarget.position) + (characterController.cameraHolder.transform.forward * sightOffset);
            SetFov(Mathf.Lerp(weaponCamera.fieldOfView,
                        0.85f * 60f, 8* Time.deltaTime));
        }
        else
        {
            weaponAnimator.SetBool("IsAiming", false);
            settings.MovementSwayX = 3;
            settings.MovementSwayY = 3;
            SetFov(Mathf.Lerp(weaponCamera.fieldOfView,
                       60,8 * Time.deltaTime));
        }
        weaponSwayPosition = weaponSwayObject.transform.position;
        weaponSwayPosition = Vector3.SmoothDamp(weaponSwayPosition, targetPosition, ref weaponSwayPositionVelocity, aimingInTime);
        weaponSwayObject.transform.position = weaponSwayPosition;
    }
    public void TriggerJump()
    {
        isGroundedTrigger = false;
        weaponAnimator.SetTrigger("Jump");
    }


    private void CalculateWeaponRotation()
    {

        //Weapon Rotation
        targetWeaponRotation.y += settings.SwayAmount * (settings.SwayXInverted ? -characterController.input_View.x : characterController.input_View.x) * Time.deltaTime;
        targetWeaponRotation.x += settings.SwayAmount * (settings.SwayYInverted ? characterController.input_View.y : -characterController.input_View.y) * Time.deltaTime;
        //Clamp Weapon Rotation 
        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -settings.SwayClampX, settings.SwayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -settings.SwayClampY, settings.SwayClampY);
        targetWeaponRotation.z = targetWeaponRotation.y;
        //Damping Weapon Rotations
        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, settings.SwayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, settings.SwaySmoothing);

        targetWeaponMovementRotation.z = settings.MovementSwayX * (settings.MovementSwayXInverted ? -characterController.input_Movement.x : characterController.input_Movement.x);
        targetWeaponMovementRotation.x = settings.MovementSwayY * (settings.MovementSwayYInverted ? -characterController.input_Movement.y : characterController.input_Movement.y);

        targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, settings.SwayResetSmoothing);
        newWeaponMovementRotation = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, settings.SwaySmoothing);

        transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }

    private void SetWeaponAnimations()
    {
        if (isGroundedTrigger)
        {
            fallingDelay = 0;
        }
        else
        {
            fallingDelay += Time.deltaTime;
        }
        if (characterController.isGrounded && !isGroundedTrigger && fallingDelay > 0.1f)
        {
            weaponAnimator.SetTrigger("Land");
            characterController.JumpSound();
            isGroundedTrigger = true;
        }
        if (!characterController.isGrounded && isGroundedTrigger)
        {
            weaponAnimator.SetTrigger("Falling");
            isGroundedTrigger = false;
        }
        weaponAnimator.SetBool("IsSprinting", characterController.isSprinting);
        weaponAnimator.SetFloat("WeaponAnimationSpeed", characterController.weaponAnimationSpeed);

    }
    private void CalculateWeaponSway()
    {
        var targetPosition = LissajousCurve(swayTime, swayAmountA, swayAmountB) / swayScale;
        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime * swayLerpSpeed);
        swayTime += Time.deltaTime;

        if (swayTime > 6.3f)
        {
            swayTime = 0;
        }
        //weaponSwayObject.localPosition = swayPosition;
    }
    //breathing
    private Vector3 LissajousCurve(float Time, float A, float B)
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }

    #endregion
    #region - Fire / Reload -
    public void UpdateWeapon()
    {
       if(characterController.equipedWeapon != null)
          equipedWeaponStats = characterController.equipedWeapon.GetComponent<scr_Weapon>();
    }
    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.1f);
        meleeCollider.enabled = true;
        yield return new WaitForSeconds(0.20f);
        meleeCollider.enabled = false;

    }
    public void Attack()
    {
        //MeleeAttack
        if(equipedWeaponStats!=null){
            if (equipedWeaponStats.type == scr_Weapon.Type.Melee)
            {
                if (readyToShoot)
                {
                    readyToShoot = false;
                    weaponAnimator.SetTrigger("DoSwing");
                    weaponAudio.PlayOneShot(equipedWeaponStats.weaponSoundClip[Random.Range(0, equipedWeaponStats.weaponSoundClip.Length)]);
                    StopCoroutine("Swing");
                    StartCoroutine("Swing");
                    Invoke("ResetShot", equipedWeaponStats.timeBetweenShooting);
                    if (equipedWeaponStats.bulletsShot > 0 && equipedWeaponStats.bulletsLeft > 0)
                        Invoke("Shoot", equipedWeaponStats.timeBetweenShots);
                    weaponAnimator.SetTrigger("DoneSwing");

                }
                
            }
            else { 
                equipedWeaponStats.bulletsShot = 1;
                
                if (readyToShoot && !reloading && equipedWeaponStats.bulletsLeft > 0)
                {
                    readyToShoot = false;
                    //Calculate Direction with Spread
                    var isCrit = IsCrit(characterController.attributes[11].value.ModifiedValue/100f);

                    var damage = CalculateDamage(characterController.damage,isCrit,0,characterController.attributes[7].value.ModifiedValue);

                    Vector3 direction = Sight.transform.forward + new Vector3(0 , 0 , 0);
                    GameObject instantBullet = Instantiate(bullet,attackPoint.position,Quaternion.identity);
                    Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
                    
                    instantBullet.GetComponent<scr_PlayerProjectile>().damage = damage;
                    instantBullet.GetComponent<scr_PlayerProjectile>().isCrit = isCrit;
                    
                    instantBullet.GetComponent<scr_PlayerProjectile>().shooterTransform = this.transform;

                    bulletRigid.velocity = attackPoint.forward * 100;
                    
                    //RayCast
                    if (Physics.Raycast(Sight.transform.position, direction, out rayHit, 100f))
                    {
                        // Debug.Log(rayHit.collider.name);
                        if (rayHit.collider.CompareTag("Enemy"))
                        {
                            enemy = rayHit.collider.GetComponent<scr_Enemy>();
                            enemy.target = this.transform;
                            

                            //Damage Enemy
                            if(enemy !=null){
                            //add try catch
                                Vector3 knockBack = rayHit.collider.transform.position- transform.position;
                            }
                        }
                    }
                    //Recoil
                    Recoil_Script.RecoilFire(equipedWeaponStats.recoilX,equipedWeaponStats.recoilY, equipedWeaponStats.recoilZ);
                    //Graphics and Sounds
                    weaponAudio.PlayOneShot(equipedWeaponStats.weaponSoundClip[Random.Range(0, equipedWeaponStats.weaponSoundClip.Length)]);
                  
                    
                    if (shotEmitters != null)
                        {
                            foreach (var e in shotEmitters)
                                e.Emit(1);
                        }
                    equipedWeaponStats.bulletsLeft--;
                    equipedWeaponStats.bulletsShot--;
                    Invoke("ResetShot", equipedWeaponStats.timeBetweenShooting);
                    if (equipedWeaponStats.bulletsShot > 0 && equipedWeaponStats.bulletsLeft > 0)
                        Invoke("Attack", equipedWeaponStats.timeBetweenShots);
                    }
            }
        }
    }
    int CalculateDamage(float pureDamage,bool crit,int armor,int armorpen){
        var damage = (pureDamage * Random.Range(0.77f,1.13f));
        var armorpencalculate = (armor-armorpen >=0)? armor-armorpen : 0;
        damage -= armorpencalculate; 
        if(crit){
            damage *=characterController.attributes[12].value.ModifiedValue/100f;
        }

        return (int)damage;
    }
    bool IsCrit(float critChance){
        
        return (critChance>Random.Range(0f,1f));
    }
    public void enemyKilled(scr_Enemy enemy){
        enemyHealthGroup.anchoredPosition = Vector3.up *1000;
        characterController.GainExp(enemy.exp);

    }
    public void HealthBarDisplay(int x,int y,string z,int level){
        enemyHealthGroup.anchoredPosition = new Vector3(0,-100,0);
        enemyHealthBar.localScale = new Vector3((float)x/y,1,1);
        enemyHealthBarText.text = ((int)(((float)x/y)*100))+ "%";
        enemyName.text = z;
        enemyLevelText.text = level+"";
    }
    private void ResetShot()
    {
        readyToShoot = true;
    }
    public void Reload()
    {   
        
        if (equipedWeaponStats!=null && equipedWeaponStats.bulletsLeft < equipedWeaponStats.magazineSize && !reloading) { 
            reloading = true;
            weaponAnimator.SetTrigger("Reload");
            weaponAudio.PlayOneShot(reloadClip[Random.Range(0, reloadClip.Length)]);
            StartCoroutine(reloadFinished(equipedWeaponStats.reloadTime));
        }
    }

    IEnumerator reloadFinished(float reloadTime){
        yield return new WaitForSeconds(reloadTime);
            equipedWeaponStats.bulletsLeft = equipedWeaponStats.magazineSize;
            reloading = false;
    }
    #endregion
    #region - Recoil -

    #endregion
}
