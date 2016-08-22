using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour
{

    private Vector3 AntiRotation = Vector3.zero;

    //when instantiated, destroy the laser script after trapActiveTime
    void Awake()
    {
        StartCoroutine(DestroyAfterTime());
    }

    //laser update
    void FixedUpdate()
    {
        LaserTrap trap = gameObject.GetComponent<LaserTrap>();

        if (trap)
        {
            trap.line.GetComponent<Transform>().position = trap.startPos.position;         
            trap.line.SetPosition(1, trap.endPos.position - trap.startPos.position);
            AntiRotation.Set(trap.line.transform.rotation.eulerAngles.x, -transform.rotation.eulerAngles.y, trap.line.transform.rotation.eulerAngles.z);
            trap.line.transform.localEulerAngles =AntiRotation;

            RaycastHit[] hits;
            hits = Physics.RaycastAll(new Ray(trap.startPos.position, Vector3.Normalize(trap.endPos.position - trap.startPos.position)), Vector3.Distance(trap.startPos.position, trap.endPos.position), (1 << 8) | (1 << 9));
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];

                Debug.DrawLine(trap.startPos.position, hit.point, Color.magenta, 2f);

                if (hit.transform.GetComponent<MonoBehaviour>())
                {
                    MonoBehaviour gotHit = hit.transform.GetComponent<MonoBehaviour>();

                    if (gotHit is BasePlayer)
                    {
                        bool addScript = true;
                        // get the BasePlayer of the Game Object
                        BasePlayer player = hit.transform.GetComponent<BasePlayer>();

                        if (!player.IsDead)
                        {
                            Vector3 tmpPosition = player.GetComponent<Transform>().position;
                            Quaternion tmpRotation = player.GetComponent<Transform>().rotation;
                            player.CurrentDeathTime = 0.0f;
                            if (player.Health == 0)
                            {
                                addScript = false;
                            }
                            player.InstantKill(this);
                            Debug.Log("HHULLLAAAHUPP");

                            //create playerMesh to destroy it without destroying the real player
                            GameObject destroyMesh = null;
                            switch (player.name)
                            {
                                case "Birdman":
                                    destroyMesh = trap.playerMeshes[0];
                                    break;
                                case "Charger":
                                    destroyMesh = trap.playerMeshes[1];
                                    break;
                                case "Fatman":
                                    destroyMesh = trap.playerMeshes[2];
                                    break;
                                case "Timeshifter":
                                    destroyMesh = trap.playerMeshes[3];
                                    break;
                                case "Babuschka":
                                    destroyMesh = trap.playerMeshes[4];
                                    break;
                                case "Pantomime":
                                    destroyMesh = trap.playerMeshes[5];
                                    break;
                            }
                            if (destroyMesh != null)
                            {

                                //toDestroy.gameObject.AddComponent<PolyExplosion>();

                                if (addScript)
                                {
                                    GameObject toDestroy = Instantiate(destroyMesh, tmpPosition, tmpRotation) as GameObject;
                                    toDestroy.gameObject.AddComponent<CutUpMesh>();
                                }
                            }
                        }
                        
                    }

                    if (gotHit is BaseEnemy)
                    {
                        BaseEnemy enemy = hit.transform.GetComponent<BaseEnemy>();

                        if (gotHit is BossEnemy)
                        {
                            if (trap.bossDamage != 0)
                            {
                               //enemy.TakeDamage(trap.bossDamage, this);
                            }
                        }
                        else
                        {
                            enemy.InstantKill(this);
                            //enemy.gameObject.AddComponent<PolyExplosion>();

                            enemy.gameObject.AddComponent<CutUpMesh>();
                        }
                    }
                }
            }
        }
    }

    //destroy laser
    protected IEnumerator DestroyAfterTime()
    {
        LaserTrap trap = gameObject.GetComponent<LaserTrap>();

        if (trap)
        {
            yield return new WaitForSeconds(trap.trapActiveTime);
            trap.line.GetComponent<Transform>().position = trap.startPos.position;
            trap.line.SetPosition(1, new Vector3(0, 0, 0));
            Destroy(this, 0);
        }
    }


}
