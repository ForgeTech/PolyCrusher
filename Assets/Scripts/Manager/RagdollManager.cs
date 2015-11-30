using UnityEngine;
using System.Collections;


public class RagdollManager : MonoBehaviour
{
    // Force which will be applied to the ragdoll objects.
    [SerializeField]
    private float explosionForce = 13f;
    // Modifier which defines the upward modifier.
    [SerializeField]
    private float upwardsModifier = 1f;

    // Radius of the explosion.
    [SerializeField]
    private float explosionRadius = 1f;

    // Lifetime after death of the ragdoll.
    [SerializeField]
    private float lifeTimeAfterDeath = 5f;

    // Array with the prefabs.
    [SerializeField]
    private GameObject[] ragdollPrefabs;

    /// <summary>
    /// Register the RagdollSpawn to the EnemyKilled delegate.
    /// </summary>
    void Awake()
    {
        BaseEnemy.EnemyKilled += RagdollSpawn;
    }

    /// <summary>
    /// Handle the spawn of the ragdoll prefab.
    /// </summary>
    /// <param name="enemy">Killed enemy.</param>
    protected void RagdollSpawn(BaseEnemy enemy)
    {
        // True when enemy is killed with a ragdoll attack.
        if (enemy.KilledWithRagdoll)
        {
            int prefabCounter = 0;
            int i = 0;
            // True when comparison was successful.
            bool stringComparison = false;

            // Compare the enemy name string with the prefab name string
            while (i < ragdollPrefabs.Length && !stringComparison)
            {
                // Get the ragdoll value script from the prefab array.
                RagdollEnemyScript ragdollValueScript = ragdollPrefabs[i].GetComponent<MonoBehaviour>() as RagdollEnemyScript;

                // Compare the names.
                if (string.Compare(ragdollValueScript.EnemyName.ToUpper(), enemy.EnemyName.ToUpper(), System.StringComparison.Ordinal) == 0)
                {
                    stringComparison = true;
                    prefabCounter = i;
                }
                i++;
            }

            if (stringComparison)
            {
                // Spawn the prefab and set the animation time to 0, so the animation won't be played and the enemy will be destroyed instantly.
                GameObject spawnedRagdoll = Instantiate(ragdollPrefabs[prefabCounter], enemy.transform.position, enemy.transform.rotation) as GameObject;
                enemy.LifeTimeAfterDeath = 0f;

                // Scaledown the enemy for smooth disappearance.
                StartCoroutine(spawnedRagdoll.transform.ScaleFrom(new Vector3(0.4f, 0.4f, 0.4f), lifeTimeAfterDeath, AnimCurveContainer.AnimCurve.downscale.Evaluate));

                // Get the bone array script where the bones are stored.
                RagdollEnemyScript boneArrayScript = spawnedRagdoll.GetComponent<MonoBehaviour>() as RagdollEnemyScript;

                // True if there are bones.
                if (boneArrayScript != null && boneArrayScript.BoneArray != null)
                {
                    ApplyForce(enemy.OriginRagdollForcePosition, boneArrayScript.BoneArray, explosionForce, explosionRadius, upwardsModifier);
                }
                else
                {
                    // Send warning if there are no bones accessable.
                    Debug.Log("**********Warning: No Bone Array Script attached OR no objects in Bone Array!**********");
                }

                // Destroy the spawned ragdoll after a time.
                Destroy(spawnedRagdoll, this.lifeTimeAfterDeath);
            }
            else
            {
                // Send warning if the Stringcomparison fails.
                Debug.Log("**********Warning: Stringcomparison in Ragdoll Manager failed, check enemy names!**********");
            }
        }
    }

    /// <summary>
    /// Applies the force to the rigidbody of the ragdoll objects
    /// </summary>
    /// <param name="originForcePosition">Origin position of the damage dealer (Charger, rocket, etc.).</param>
    /// <param name="boneArray">Array which contains the bones of the prefab.</param>
    /// <param name="explosionForce">Value of the explosion force.</param>
    /// <param name="explosionRadius">Radius of the explosion.</param>
    /// <param name="upwardsModifier">Upwards modifier which controls the upward force.</param>
    private void ApplyForce(Vector3 originForcePosition, GameObject[] boneArray, float explosionForce, float explosionRadius, float upwardsModifier)
    {    
        // Traverse the bones and apply the force to all of them.
        foreach (GameObject objects in boneArray)
        {            
            Rigidbody colliderRigidbody = objects.gameObject.GetComponent<Rigidbody>();
            colliderRigidbody.AddExplosionForce(explosionForce, originForcePosition, explosionRadius, upwardsModifier, ForceMode.Impulse);
            CheckAndSplitChild(objects);
        }
    }

    /// <summary>
    /// Checks if the child should be splitted from the parent object and splits it.
    /// </summary>
    /// <param name="childObject">Object which schould be checked.</param>
    private void CheckAndSplitChild(GameObject childObject)
    {
        SplitFromParent splittingChild = childObject.GetComponent<MonoBehaviour>() as SplitFromParent;
        if (splittingChild != null)
        {
            splittingChild.Split(lifeTimeAfterDeath);
        }
    }
}