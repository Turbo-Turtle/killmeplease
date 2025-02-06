using Unity.VisualScripting;
using UnityEngine;

public enum WeaponType
{
    RIFLE,
    SHOTGUN,
    GRENADE,
    COUNT
}

public abstract class Weapon
{
    // "How do we want *ALL* bullets to behave?" -- Each have a unique direction & speed
    public abstract void Shoot(Vector3 direction, float speed);

    // "How do we want *ALL* weapons to behave?" -- Each weapon needs a prefab, and a shooter
    public GameObject weaponPrefab;
    public GameObject shooter;

    public int amoCount;    // How much amo is currently in our clip
    public int amoMax;      // How much amo does our clip hold

    public float reloadCurrent; // How far into our reload cooldown
    public float reloadTotal;   // How long it takes to reload

    public float shootCurrent;  // How far into our shoot cooldown
    public float shootTotal;    // How long it take to shoot
}

public class Rifle : Weapon
{
    public override void Shoot(Vector3 direction, float speed)
    {
        GameObject bullet = GameObject.Instantiate(weaponPrefab);
        bullet.transform.position = shooter.transform.position + direction * 0.75f;
        bullet.GetComponent<Rigidbody2D>().velocity = direction * speed;
        bullet.GetComponent<SpriteRenderer>().color = Color.red;
        GameObject.Destroy(bullet, 1.0f);
    }
}


public class Shotgun : Weapon
{
    public override void Shoot(Vector3 direction, float speed)

    {
        GameObject bullet = GameObject.Instantiate(weaponPrefab);
        GameObject bulletLeft = GameObject.Instantiate(weaponPrefab);
        GameObject bulletRight = GameObject.Instantiate(weaponPrefab);
        Vector3 directionLeft = Quaternion.Euler(0.0f, 0.0f, 30.0f) * direction;
        Vector3 directionRight = Quaternion.Euler(0.0f, 0.0f, -30.0f) * direction;
        bullet.transform.position = bullet.transform.position + direction * 0.75f;
        bulletLeft.transform.position = bullet.transform.position + directionLeft * 0.75f;
        bulletRight.transform.position = bullet.transform.position + directionRight * 0.75f;

        bullet.GetComponent<Rigidbody2D>().velocity = direction * speed;
        bulletLeft.GetComponent<Rigidbody2D>().velocity = directionLeft * speed;
        bulletRight.GetComponent<Rigidbody2D>().velocity = directionRight * speed;
        bullet.GetComponent<SpriteRenderer>().color = Color.green;
        bulletLeft.GetComponent<SpriteRenderer>().color = Color.green;
        bulletRight.GetComponent<SpriteRenderer>().color = Color.green;

        GameObject.Destroy(bullet, 1.0f);
        GameObject.Destroy(bulletLeft, 1.0f);
        GameObject.Destroy(bulletRight, 1.0f);
    }

}

public class GrenadeLauncher : Weapon
{

    public override void Shoot(Vector3 direction, float speed)

    {
        GameObject grenade = GameObject.Instantiate(weaponPrefab);
        grenade.transform.position = shooter.transform.position + direction * 0.75f;
        grenade.GetComponent<Rigidbody2D>().velocity = direction * speed;
        GameObject.Destroy(grenade, 1.0f);
        }
    }


public class Weapons : MonoBehaviour
{
    [SerializeField]
    GameObject bulletPrefab;

    [SerializeField]
    GameObject grenadePrefab;

    float bulletSpeed = 10.0f;
    float moveSpeed = 5.0f;

    WeaponType weaponType = WeaponType.RIFLE;
    Weapon rifle = new Rifle();
    Weapon weapon = null;

    
    Weapon shotgun = new Shotgun();
  

    Weapon grenadelauncher = new GrenadeLauncher();
    


    void Start()
    {
        rifle.weaponPrefab = bulletPrefab;
        rifle.shooter = gameObject;

        rifle.shootCurrent = 0.0f;
        rifle.shootTotal = 0.25f;

        shotgun.weaponPrefab = bulletPrefab;
        shotgun.shooter = gameObject;

        shotgun.shootCurrent = 0.0f;
        shotgun.shootTotal = 0.5f;

        grenadelauncher.weaponPrefab = bulletPrefab;
        grenadelauncher.shooter = gameObject;

        grenadelauncher.shootCurrent = 0.0f;
        grenadelauncher.shootTotal = 1.0f;

        rifle.amoCount = 5;
        rifle.amoMax = 5;

        shotgun.amoCount = 30;
        shotgun.amoMax = 6;

        grenadelauncher.amoCount = 30;
        grenadelauncher.amoMax = 3;

        rifle.reloadCurrent = 0.0f;
        rifle.reloadTotal = 2.0f;

        shotgun.reloadCurrent = 0.0f;
        shotgun.reloadTotal = 3.0f;

        grenadelauncher.reloadCurrent = 0.0f;
        grenadelauncher.reloadTotal = 4.0f;

        weapon = rifle;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Directional movement
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.up;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector3.down;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.right;
        }
        direction = direction.normalized;
        Vector3 movement = direction * moveSpeed * dt;
        transform.position += movement;

        // Aiming with mouse cursor
        Vector3 mouse = Input.mousePosition;
        mouse = Camera.main.ScreenToWorldPoint(mouse);
        mouse.z = 0.0f;
        Vector3 mouseDirection = (mouse - transform.position).normalized;
        Debug.DrawLine(transform.position, transform.position + mouseDirection * 5.0f);

        // Shoot weapon with space
        if (Input.GetKey(KeyCode.Space))
        {
            if (weapon.amoCount > 0)
            {
                weapon.shootCurrent += dt;
                if (weapon.shootCurrent >= weapon.shootTotal)
                {
                    weapon.shootCurrent = 0.0f;
                    weapon.Shoot(mouseDirection, bulletSpeed);

                    weapon.amoCount--;
                    Debug.Log("Ammo = " + weapon.amoCount);
                }
            }
            else
            {
                // If we're done reloading, replenish amo
                weapon.reloadCurrent += dt;
                if (weapon.reloadCurrent >= weapon.reloadTotal)
                {
                    weapon.reloadCurrent = 0.0f;
                    weapon.amoCount = weapon.amoMax;
                }
            }
        }

        // Cycle weapon with left-shift
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            int weaponNumber = (int)++weaponType;
            weaponNumber %= (int)WeaponType.COUNT;
            weaponType = (WeaponType)weaponNumber;
            Debug.Log("Selected weapon: " + weaponType);
            switch (weaponType)
            {
                case WeaponType.RIFLE:
                    weapon = rifle;
                    break;
                
                case WeaponType.SHOTGUN:
                    weapon = shotgun;
                    break;
                
                case WeaponType.GRENADE:
                    weapon = grenadelauncher;
                    break;
            }
        }
    }
}
