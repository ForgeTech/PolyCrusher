using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {

    void Awake()
    {
        StartCoroutine(DestroyAfterTime());
    }
    
    void Update()
    {
        LaserTrap trap = gameObject.GetComponent<LaserTrap>();

        if (trap)
        {
            trap.line.GetComponent<Transform>().position = trap.startPos.position;
            trap.line.SetPosition(1, trap.endPos.position - trap.startPos.position);

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
                        // get the BasePlayer of the Game Object
                        BasePlayer player = hit.transform.GetComponent<BasePlayer>();
                        Vector3 tmpPosition = player.GetComponent<Transform>().position;
                        Quaternion tmpRotation = player.GetComponent<Transform>().rotation;
                        player.CurrentDeathTime = 0;
                        player.InstantKill();

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
                        }
                        if (destroyMesh != null)
                        {
                            GameObject toDestroy = Instantiate(destroyMesh, tmpPosition, tmpRotation) as GameObject;
                            toDestroy.gameObject.AddComponent<PolyExplosion>();
                        }
                    }

                    if (gotHit is BaseEnemy)
                    {
                        BaseEnemy enemy = hit.transform.GetComponent<BaseEnemy>();

                        if (gotHit is BossEnemy)
                        {
                            if (trap.bossDamage != 0)
                            {
                                //enemy.TakeDamage(42, null);
                            }

                        }
                        else
                        {
                            enemy.InstantKill();
                            enemy.gameObject.AddComponent<PolyExplosion>();
                        }
                    }

                }
            }
        }
    }

    protected IEnumerator DestroyAfterTime()
    {
        LaserTrap trap = gameObject.GetComponent<LaserTrap>();

        if (trap)
        {
            yield return new WaitForSeconds(trap.trapActiveTime);
            trap.line.GetComponent<Transform>().position = trap.startPos.position;
            trap.line.SetPosition(1, new Vector3(0,0,0));
            Destroy(this, 0);
        }
    }


}
