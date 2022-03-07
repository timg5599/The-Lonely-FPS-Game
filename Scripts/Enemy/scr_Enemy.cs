using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;



public class scr_Enemy : MonoBehaviour
{
    public enum Type { Norm,Dash,Range}
    public Type enemyType;
    // Start is called before the first frame update
    [Header("EnemyStats")]
    public string enemyName;
    public int enemyLevel;
    public int maxHealth;
    public int currentHealth;
    public int exp;
    public int armor;

    public int collisionDamage;
    
    public BoxCollider meleeArea;
    public Transform target;

    public bool isChasing;
    public bool isPlayerDetected;
    public bool isAttacking;
    NavMeshAgent nav; 
    Rigidbody rigidbody;
    BoxCollider boxCollider;

    MeshRenderer[] meshes;
    Animator enemyAnimator;
    public GameObject projectile;
    public float nextTargetLocationCheckTime=0f;
    public delegate void EnemyDamaged (int x, int y, string z,int level);
    public static event EnemyDamaged enemyDamaged;
    public delegate void EnemyDead (scr_Enemy x);
    public static event EnemyDead enemyDead;
    public delegate void EnemyDeadNoParam();
    public static event EnemyDeadNoParam enemyDeadNoParam;

    public GameObject FloatingTextPrefab;
    public GameObject FloatingTextCritPrefab;
    public GameObject FloatingTextHeadCritPrefab;

    public GameObject[] currency;
    [SerializeField]GameObject[] groundItemPrefabs;
    [SerializeField]scr_ItemObject[] dropList;
    [SerializeField]AudioClip[] enemyDamagedSound;
    [SerializeField]AudioSource enemyAudio;


    
    


#region - Start -
    void Start(){
        nav = GetComponent<NavMeshAgent>();
        nav.enabled = false;

    }
#endregion
    #region - Awake-
    void Awake()
    {
        exp = enemyLevel *3;
        rigidbody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshes = GetComponentsInChildren<MeshRenderer>();
        enemyAnimator = GetComponentInChildren<Animator>();
        Invoke("EnableNav",0.025f);
        ChaseStart();
    }
    #endregion
    #region - Update -
    void Update()
    {
        if (nav.enabled) { 
            if (!isPlayerDetected){
                if(Time.time > nextTargetLocationCheckTime){
                    nav.SetDestination(RandomNavmeshLocation(50f));
                    nav.isStopped = !isChasing;   
                    nextTargetLocationCheckTime +=Random.Range(4,8);
                    ChaseStart();
                    }
                    if(nav.velocity ==Vector3.zero){
                        enemyAnimator.SetBool("IsIdle", true);
                    }else{
                        enemyAnimator.SetBool("IsIdle", false);
                        
                    }
            }else{
                nav.SetDestination(target.position);
                nav.isStopped = !isChasing;   
                enemyAnimator.SetBool("IsIdle", false);               
                
            }
            
        }
    }
    #endregion
    #region - Fixed Update-
    void FixedUpdate()
    {
        TargetingPlayer();
        FreezeVelocity();
    }
    #endregion
    #region - AI Movement -
    public void EnableNav(){
        nav.enabled = true;
    }
    public void ChaseStart()
    {
        isChasing = true;
        enemyAnimator.SetBool("IsWalking", true);
    }

    void FreezeVelocity()
    {
        if (isChasing) { 
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }
     public Vector3 RandomNavmeshLocation(float radius) {
         Vector3 randomDirection = Random.insideUnitSphere * radius;
         randomDirection += transform.position;
         NavMeshHit hit;
         Vector3 finalPosition = Vector3.zero;
         if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
             finalPosition = hit.position;            
         }
         return finalPosition;
     }
    public void setTargetDestination(Vector3 position)
    {
        target.position = position;
    }

    void TargetingPlayer()
    {
        float targetRadius = 0f;
        float targetRange = 0f;
        switch (enemyType)
        {
            case Type.Norm:
                targetRadius = 1.5f;
                targetRange = 3.5f;
                break;
            case Type.Dash:
                targetRadius = 2f;
                targetRange = 12f;
                break;
            case Type.Range:
                targetRadius = 0.7f;
                targetRange = 60f;
                break;
        }
        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));
        if (rayHits.Length > 0 && !isAttacking)
        {
            isPlayerDetected = true;
            target = rayHits[0].collider.transform; 
            StartCoroutine(AttckPlayer());
        }

    }

    #endregion
    #region - Attacked - 
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enemy Trigger Player Projectile");
        if(other.tag == "Melee")
        {   
            target= other.gameObject.transform;
            scr_Weapon weapon = other.GetComponent<scr_Weapon>();
                if(currentHealth - (int)weapon.damage <0){
                    currentHealth = 0;
                }else{
                    currentHealth -= (int)weapon.damage;
                }
            ShowDamageTaken((int)weapon.damage,false,target.transform,false);

            ChaseStart();
            Vector3 knockBack = transform.position - other.transform.position;
            StartCoroutine(OnDamage(knockBack,"Melee"));
            isPlayerDetected=true;
            enemyDamaged(this.currentHealth,this.maxHealth,this.enemyName,this.enemyLevel);
        }
    }
    private void OnCollisionEnter(Collision other) {
       
        if(other.gameObject.tag == "PlayerProjectile"){
            var coltype =other.contacts[0].thisCollider.GetType();
            if(typeof(SphereCollider)== coltype){
                Debug.Log("HeadShot" + coltype);
                scr_PlayerProjectile instant = other.gameObject.GetComponent<scr_PlayerProjectile>();
                var calculateDamage = (int)((instant.damage-armor<0)? 0 : (instant.damage-armor)*1.3);
                if(currentHealth - calculateDamage <0){
                        currentHealth = 0;
                    }else{
                        currentHealth -= calculateDamage;
                    }
                ShowDamageTaken(calculateDamage,false,target.transform,true);
                ChaseStart();
                isPlayerDetected=true;
                enemyDamaged(this.currentHealth,this.maxHealth,this.enemyName,this.enemyLevel);
                StartCoroutine(OnDamage(Vector3.zero, null));
            }else{
                scr_PlayerProjectile instant = other.gameObject.GetComponent<scr_PlayerProjectile>();
                var calculateDamage = (instant.damage-armor<0)? 0 : instant.damage-armor;
                if(currentHealth - calculateDamage <0){
                        currentHealth = 0;
                    }else{
                        currentHealth -= calculateDamage;
                    }
                ShowDamageTaken(calculateDamage,instant.isCrit,target.transform,false);
                ChaseStart();
                isPlayerDetected=true;
                enemyDamaged(this.currentHealth,this.maxHealth,this.enemyName,this.enemyLevel);
                StartCoroutine(OnDamage(Vector3.zero, null));
            }
        }
    }
    
    public void ShowDamageTaken(int x, bool crit, Transform position,bool isHead){
        if(x == 0 ){
            var damageText = Instantiate(FloatingTextPrefab,transform.position+new Vector3(0,3.8f,0),this.target.transform.rotation,transform);
            var textmesh = damageText.GetComponentInChildren<TextMeshPro>();
            textmesh.text = "Miss";
            Destroy(damageText,1.3f);
        }else{
        if(isHead){
            Debug.Log("Headshot");
            var damageText = Instantiate(FloatingTextHeadCritPrefab,transform.position+new Vector3(0,3.8f,0),this.target.transform.rotation,transform);
            var textmesh = damageText.GetComponentInChildren<TextMeshPro>();
            textmesh.text = x.ToString();
            Destroy(damageText,1.3f);

        }
        else if(!crit){
            var damageText = Instantiate(FloatingTextPrefab,transform.position+new Vector3(0,3.8f,0),this.target.transform.rotation,transform);
            var textmesh = damageText.GetComponentInChildren<TextMeshPro>();
            textmesh.text = x.ToString();
            Destroy(damageText,1.3f);
        }else{
            var damageText = Instantiate(FloatingTextCritPrefab,transform.position+new Vector3(0,3.8f,0),this.target.transform.rotation,transform);
            var textmesh = damageText.GetComponentInChildren<TextMeshPro>();
            textmesh.text = x.ToString();
            Destroy(damageText,1.3f);


        }
        
        }

    }
    public IEnumerator OnDamage(Vector3 knockBack,string weaponType)
    {
        
        if (currentHealth > 0 && this!=null)
        {
        enemyDamaged(currentHealth,maxHealth,enemyName,enemyLevel);
        enemyAudio.PlayOneShot(enemyDamagedSound[Random.Range(0, enemyDamagedSound.Length)]);
            if(this!=null){
            foreach (MeshRenderer mesh in meshes)
            {
                mesh.material.color = Color.red;
            }
            yield return new WaitForSeconds(0.09f);
            if (weaponType == "Melee") {
                isChasing = false;
                foreach (MeshRenderer mesh in meshes)
                {
                    mesh.material.color = Color.white;
                }
                knockBack = knockBack.normalized;
                knockBack += Vector3.up;
                rigidbody.AddForce(knockBack * 10f, ForceMode.Impulse);
                yield return new WaitForSeconds(0.4f);
                isChasing = true;
            }
            else
            {
                foreach (MeshRenderer mesh in meshes)
                {
                    mesh.material.color = Color.white;
                }
                
            }
            }
            
        }
        //Death
        else
        {
            if (this!=null &&gameObject.layer!= 17) {
                gameObject.layer = 17;
                enemyDead(this);
                enemyDeadNoParam();

            foreach (MeshRenderer mesh in meshes)
            {
                mesh.material.color = Color.gray;
            }
                isChasing = false;
                nav.enabled = false;
                enemyAnimator.SetTrigger("Death");
                knockBack = knockBack.normalized;
                knockBack += Vector3.up;
                DropLoot();
                rigidbody.AddForce(knockBack*2.5f,ForceMode.Impulse);
                yield return new WaitForSeconds(0.10f);
                rigidbody.velocity = Vector3.zero;
                rigidbody.isKinematic = true;
                this.boxCollider.enabled = false;
                Destroy(gameObject, 2);

                
            }else{
                 Destroy(gameObject, 1);

            }
            
        }

    }
    #endregion
    #region -  Attack -
    public IEnumerator AttckPlayer()
    {
        isChasing = false;
        isAttacking = true;
        FreezeVelocity();
        enemyAnimator.SetBool("IsAttacking", true);
        switch (enemyType)
        {
            case Type.Norm:
                isChasing = false;
                if(gameObject !=null&& gameObject.layer != 17){
                    yield return new WaitForSeconds(0.5f);
                    meleeArea.enabled = true;
                    yield return new WaitForSeconds(0.2f);
                    meleeArea.enabled = false;
                    rigidbody.velocity = Vector3.zero;
                     FreezeVelocity();
                    yield return new WaitForSeconds(1.0f); 
                   
                }
                 isChasing = true;
                break;
            case Type.Dash:
                isChasing = false;
                yield return new WaitForSeconds(0.1f);
                if(gameObject!= null &&gameObject.layer != 17){
                    meleeArea.enabled = true;
                    rigidbody.AddForce(transform.forward * 40, ForceMode.Impulse);
                    yield return new WaitForSeconds(0.5f);
                    rigidbody.velocity = Vector3.zero;
                    yield return new WaitForSeconds(0.1f);
                    meleeArea.enabled = false;
                     FreezeVelocity();
                    yield return new WaitForSeconds(1.5f); 

                }
                isChasing = true;
                break;
            case Type.Range:
                isChasing = false;
                yield return new WaitForSeconds(0.3f);
                if(gameObject!=null &&gameObject.layer != 17)
                {
                    GameObject instantProjectile = Instantiate(projectile, transform.position, transform.rotation);
                    instantProjectile.transform.localScale = this.transform.localScale;
                    Rigidbody rigidBullet = instantProjectile.GetComponent<Rigidbody>();
                    rigidBullet.velocity = transform.forward * 40;
                    FreezeVelocity();
                    yield return new WaitForSeconds(2f);
                    
                }
                isChasing = true;
               
                break;
        }

        isAttacking = false;
        isChasing = true;
        enemyAnimator.SetBool("IsAttacking", false);
    }
    #endregion
    #region - Drop Loot -
    private void DropLoot()
    {

        CalculateDrop();
        int enemyCoin = (enemyLevel + Random.Range(1, 3)) * 5;
        int[] coins = CalculateCoin(enemyCoin);
        //Instantiate all coins
        if (coins[0] > 0) {
            for (int i = 0; i < coins[0]; ++i)
            {
                GameObject coin1 = Instantiate(currency[0], transform.position+Vector3.up, transform.rotation);
                Rigidbody coin = coin1.GetComponent<Rigidbody>();
                coin.velocity = new Vector3(Random.Range(-1.0f, 1.0f),Random.Range(0.0f, 1.0f),Random.Range(-1.0f, 1.0f)) * 5 ;
            }
        }
        if (coins[1] > 0)
        {
            for (int i = 0; i < coins[1]; ++i)
            {
                GameObject coin2 = Instantiate(currency[1], transform.position+Vector3.up, transform.rotation);
                Rigidbody coin = coin2.GetComponent<Rigidbody>();
                coin.velocity = new Vector3(Random.Range(-1.0f, 1.0f),Random.Range(0.0f, 1.0f),Random.Range(-1.0f, 1.0f)) * 5 ;
            }
        }
        if (coins[2] > 0)
        {
            for (int i = 0; i < coins[2]; ++i)
            {
                GameObject coin3 = Instantiate(currency[2], transform.position+Vector3.up, transform.rotation);
                Rigidbody coin = coin3.GetComponent<Rigidbody>();
                coin.velocity = new Vector3(Random.Range(-1.0f, 1.0f),Random.Range(0.0f, 1.0f),Random.Range(-1.0f, 1.0f)) * 5 ;
            }
        }
        if (Random.Range(0f,1f)< 0.2){
            GameObject coin3 = Instantiate
            (currency[3], transform.position+Vector3.up, transform.rotation);
            Rigidbody coin = coin3.GetComponent<Rigidbody>();
            coin.velocity = new Vector3(Random.Range(-1.0f, 1.0f),Random.Range(0.0f, 1.0f),Random.Range(-1.0f, 1.0f)) * 5 ;
        }
    }
    int[] CalculateCoin(int coin)
    {
        int[] _return = new int[3];
        _return[0] = coin / 100;
        _return[1] = (coin % 100) / 20;
        _return[2] = ((coin % 100) % 20) / 5;

        return _return;
    }
    void CalculateDrop(){
        foreach(scr_ItemObject x in dropList){
            if(x.dropRate> Random.Range(0f,1f)){
                if(x.type == ItemType.ETC){
                    var newGroundItem = groundItemPrefabs[2];
                   newGroundItem.GetComponent<GroundItem>().item = x;
                    Instantiate(newGroundItem, transform.position+Vector3.up*2, Quaternion.identity);
                }else if(x.type == ItemType.USE || x.type == ItemType.Potion || x.type == ItemType.WeaponScroll){
                   var newGroundItem = groundItemPrefabs[1];
                   newGroundItem.GetComponent<GroundItem>().item = x;
                    Instantiate(newGroundItem, transform.position+Vector3.up*2, Quaternion.identity);

                }else{
                   var newGroundItem = groundItemPrefabs[0];
                   newGroundItem.GetComponent<GroundItem>().item = x;
                    Instantiate(newGroundItem, transform.position+Vector3.up*2, Quaternion.identity);

                }
            }
        }
    }
    #endregion
}
