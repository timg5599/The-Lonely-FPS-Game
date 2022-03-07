using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_EnemyProjectileRotation : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.right * 360 * Time.deltaTime);
    }
}
