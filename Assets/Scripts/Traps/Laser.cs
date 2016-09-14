using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour
{
    [SerializeField]
    protected LineShaderUtility lineShader;
    protected GameObject lineShaderGameObject;

    private LaserTrap trap;

    private WaitForSeconds bossDamageCoolDown = new WaitForSeconds(0.5f);
    private bool bossTakesDamage = true;


    //when instantiated, destroy the laser script after trapActiveTime
    void Awake()
    {
        trap = GetComponent<LaserTrap>();
        lineShaderGameObject = Instantiate(trap.lineShader.gameObject);
        lineShader = lineShaderGameObject.GetComponent<LineShaderUtility>();

        StartCoroutine(DestroyAfterTime());
    }

    //laser update
    private void FixedUpdate()
    {
        lineShader.startPosition = trap.startPos.transform.position;
        lineShader.endPosition = trap.endPos.transform.position;

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
                            addScript = false;

                        player.InstantKill(this);

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
                            case "Tomic":
                                destroyMesh = trap.playerMeshes[6];
                                break;
                        }
                        if (destroyMesh != null && addScript)
                        {
                            GameObject toDestroy = Instantiate(destroyMesh, tmpPosition, tmpRotation) as GameObject;
                            toDestroy.gameObject.AddComponent<CutUpMesh>();
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
                            if (trap.bossCuttingParticles != null)
                                Destroy(Instantiate(trap.bossCuttingParticles, hit.point, hit.transform.rotation), 2);

                            if (bossTakesDamage)
                            {
                                bossTakesDamage = false;
                                StartCoroutine(StartBossDamageCoolDown());
                                enemy.TakeDamage(trap.bossDamage, this);
                            }
                        }
                    }
                    else
                    {
                        enemy.InstantKill(this);
                        enemy.gameObject.AddComponent<CutUpMesh>();
                    }
                }
            }
        }
    }

    private IEnumerator StartBossDamageCoolDown()
    {
        yield return bossDamageCoolDown;
        bossTakesDamage = true;
    }

    //destroy laser
    protected IEnumerator DestroyAfterTime()
    {
        LaserTrap trap = gameObject.GetComponent<LaserTrap>();

        if (trap)
        {
            yield return new WaitForSeconds(trap.trapActiveTime);
            Destroy(lineShaderGameObject);
            Destroy(this, 0);
        }
    }
}