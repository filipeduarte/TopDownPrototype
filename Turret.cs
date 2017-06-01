using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour {

    private Gun turret;
    private float nextCallTime;
    private float nextShotTime;
    GameObject target;

    // <<<<<<<<<<<<<<<<<<<<<<<<<TURRET BASE STATS>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
    [SerializeField]
    private float range = 6;    //How far the turret will seek a target to shoot.
    [SerializeField]
    private int turnSpeed = 3;          //How fast the turret can change direction.
    public float fireRate = 3;           //How fast the turret can shoot.
    private int turretDuration = 100;   //How long the turret will last if not destroyed.
    private float bulletDamage = 10;    //How much damage the turret do.
    public float bulletSpeed = 60;     //How fast the turret will shoot it's projectiles.
    private float bulletImpact = 1;     //How much impact the turret's projectiles will cause.
    private float shootingAngleOffset = 40; //The angle offset in wich the turret will start shooting.
    //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

    public string enemyTag;     //The tag used for the turret to acquire targets.
    public LayerMask viewMask;  //What the turret can see through.
    public Projectile projectile;
    public Transform muzzlePoint;

    public void SetTurretStats (float _range, float _damage, int _turnSpeed, int _fireRate, float _bSpeed,
        float _bDamage, float _bImpact)
    {
        range = _range;
        turnSpeed = _turnSpeed;
        fireRate = _fireRate;
        bulletDamage = _bDamage;
        bulletImpact = _bImpact;
        bulletSpeed = _bSpeed;
    }

    GameObject FindTarget()
    {
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, range); //Find objects in turret's range.
        foreach (Collider obj in objectsInRange) //Loop through the objects in range to find the target.
        {
            if (obj.tag == enemyTag)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, obj.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = obj.gameObject;
                }
            }
        }
        return nearestEnemy;
    }

    private void LockOnAndShoot(GameObject _target)
    {
        //Checks if target still in range:
        float distance = Vector3.Distance(transform.position, _target.transform.position);
        if (distance <= range)
        {
            //Find desired rotation without HEIGHT influence.
            float lookAtX = _target.transform.position.x - transform.position.x;
            float lookAtZ = _target.transform.position.z - transform.position.z;
            Vector3 lookAt = new Vector3(lookAtX, transform.position.y, lookAtZ); //Direction to target.
            Quaternion rotation = Quaternion.LookRotation(lookAt);
            Vector3 fwd = transform.TransformDirection(Vector3.forward);

            //Lock on TARGET:
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * turnSpeed);

            //Shoot as soon as the target is on aim:
            if (CheckFieldOfView(lookAt))
            {
                //SHOOT:
                Shoot();
            }
            Debug.DrawRay(transform.position, fwd * range, Color.blue); //Visualization
        }
        else
        {
            target = null;
        }
    }

    private void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            nextShotTime = Time.time + (1/fireRate);
            //Shoot:
            Projectile newProjectile = Instantiate(projectile, muzzlePoint.position, muzzlePoint.rotation) as Projectile;
            newProjectile.SetBulletStats(bulletSpeed, bulletDamage);
        }
    }

    private bool CheckFieldOfView(Vector3 directionToTarget)
    {
        directionToTarget = directionToTarget.normalized;
        float angleOffset = Vector3.Angle(transform.forward, directionToTarget);
        if (angleOffset < shootingAngleOffset / 2)
        {
            if (!Physics.Linecast(transform.position, target.transform.position, viewMask))
            {
                return true;
            }
        }
        return false;
    }

    private void Update()
    {
        float execTime = 0.4f;  //How fast the method will loop
        nextCallTime += Time.deltaTime; //Time for the next method call
        if ((nextCallTime >= execTime) && (target == null)) //If its time and the turret has no target yet. DO.
        {
            nextCallTime -= execTime;
            target = FindTarget();
        }
        else if (target != null)
        {
            LockOnAndShoot(target);
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
