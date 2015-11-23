using UnityEngine;
using System.Collections;

public class WeaponTetris : Weapon
{
    [Header("Tetris bullet prefabs")]
    [SerializeField]
    [Tooltip("Tetris prefabs")]
    protected GameObject[] tetrisPrefabs;

    /// <summary>
    /// The Shoot mechanic of the machine gun.
    /// </summary>
    public override void Use()
    {
        if (shootIsAllowed)
        {
            base.Use();
            GameObject prefab = ChooseRandomPrefab();
            GameObject g = ObjectsPool.Spawn(prefab, Vector3.zero, prefab.transform.rotation);

            Bullet bullet;

            if (g != null && g.GetComponent<MonoBehaviour>() is Bullet)
            {
                bullet = g.GetComponent<MonoBehaviour>() as Bullet;
                bullet.OwnerScript = this.OwnerScript;
                bullet.name = "TetrisBullet";
                bullet.Damage = this.WeaponDamage;
                bullet.transform.position = transform.position;
                bullet.transform.rotation = Quaternion.LookRotation(transform.forward);
                //bullet.transform.Rotate(Vector3.forward, Random.Range(0, 360f));
                float speed = bullet.BulletSpeed;

                // Random rotation
                Quaternion rotation = Quaternion.Euler(0, Random.Range(-weaponAccuracy, weaponAccuracy), 0);
                Vector3 v = transform.forward;
                Vector3 rotationVector = rotation * v;

                bullet.Shoot(rotationVector, speed);
            }

            shootIsAllowed = false;
            StartCoroutine(WaitForNextShot());
        }
    }

    /// <summary>
    /// Chooses randomly a tetris prefab and returns it.
    /// </summary>
    /// <returns></returns>
    protected GameObject ChooseRandomPrefab()
    {
        GameObject tetris = tetrisPrefabs[Random.Range(0, tetrisPrefabs.Length)];
        return tetris;
    }
}