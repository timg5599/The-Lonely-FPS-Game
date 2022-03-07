using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_Weapon : MonoBehaviour
{
    public enum Type { Melee, Range };
    public enum gunType { Pistol, Rifle,Shotgun };

    public Type type;
    public float damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletPerTap;
    public bool allowButtonHold;
    public int bulletsLeft, bulletsShot;
    [Header("SFX")]
    public AudioClip[] weaponSoundClip;

    public float recoilX;
    public float recoilY;
    public float recoilZ;


    public void Start()
    {
        bulletsLeft = magazineSize;
    }
    
}
