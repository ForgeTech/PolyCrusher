using UnityEngine;
using System.Collections;


public delegate void CuttingActivatedHandler();
public delegate void CuttingDeactivateHandler();

public class CuttingLineLogic : MonoBehaviour
{

    public static event CuttingActivatedHandler CuttingActive;
    public static event CuttingDeactivateHandler CuttingInactive;

    private bool activateCutting;

    private int bossCuttingDamage = 100;

    private Vector3 bufferVectorA = new Vector3();

    private Vector3 bufferVectorB = new Vector3();

    private float timeActive = -0.1f;

    private WaitForSeconds bossDamageCoolDown = new WaitForSeconds(0.5f);
    private bool bossTakesDamage = true;

    private LineSystem lineSystem;

    private float volume = 1.0f;

    private float lineStartOffset = 0.0f;
    private GameObject laserParticles;
    private GameObject lightSabrePrefab;
    private GameObject lightSabreGameObject;
    private LineTweens lineTweens;

    private AudioClip cuttingSound;

    private int[] firstVertex = new int[] { 0, 1, 2, 0, 1, 2 };
    private int[] secondVertex = new int[] { 1, 2, 0, 3, 3, 3 };
    private int[] linesNeeded = new int[] { 0, 1, 3, 6 };

    #region properties
    public float TimeActive
    {
        get { return timeActive; }
        set { timeActive = value; }
    }

    public float LineStartOffset
    {
        set { lineStartOffset = value; }
    }

    public GameObject LaserParticles
    {
        set { laserParticles = value; }
    }

    public GameObject LighSabrePrefab
    {
        set { lightSabrePrefab = value; }
    }

    public LineTweens LineTweens
    {
        set { lineTweens = value; }
    }

    public LineSystem LineSystem
    {
        set { lineSystem = value; }
    }

    #endregion

    private void Awake()
    {
        LevelEndManager.levelExitEvent += ResetValues;
    }

    private void ResetValues()
    {
        CuttingActive = null;
        CuttingInactive = null;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCuttingStatus();

        if (activateCutting)
        {
            if (lineSystem.Players.Length > 1)
            {
                CuttingLinesPowerUp();
            }

            if (lineSystem.Players.Length == 1 && PlayerManager.PlayerCountInGameSession == 1)
            {
                PrepareLightSabre();
            }
        }
    }



    private void UpdateCuttingStatus()
    {
        if (timeActive >= 0.0f)
        {
            if (!activateCutting)
            {               
                activateCutting = true;
                OnCuttingActivated();
            }

            timeActive -= Time.deltaTime;

            if (timeActive <= 0.0f)
            {
                activateCutting = false;              
                OnCuttingDeactivated();
                for (int i = 0; i < lineSystem.LineShaderUtilities.Length; i++)
                {
                    lineTweens.TweenColor(i, -1, false);
                    lineTweens.TweenAmplitude(i, 0.0f, LineShaderType.SineWave);
                }
            }
        }
    }

    private void OnCuttingActivated()
    {
        if (CuttingActive != null)
        {
            CuttingActive();
        }
    }

    private void OnCuttingDeactivated()
    {
        if (CuttingInactive != null)
        {
            CuttingInactive();
        }
    }

    private void CuttingLinesPowerUp()
    {
        for (int i = 0; i < linesNeeded[lineSystem.Players.Length - 1]; i++)
        {
            RaycastHit[] hits;
            bufferVectorA = lineSystem.Players[firstVertex[i]].transform.position;
            bufferVectorA.y = lineStartOffset;

            bufferVectorB = lineSystem.Players[secondVertex[i]].transform.position;
            bufferVectorB.y = lineStartOffset;

            hits = Physics.RaycastAll(new Ray(bufferVectorB, Vector3.Normalize(bufferVectorA - bufferVectorB)), Vector3.Distance(bufferVectorB, bufferVectorA), (1 << 9));

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.GetComponent<MonoBehaviour>())
                {
                    MonoBehaviour gotHit = hit.transform.GetComponent<MonoBehaviour>();
                    if (gotHit is BaseEnemy)
                    {
                        BaseEnemy enemy = hit.transform.GetComponent<BaseEnemy>();
                        if (gotHit is BossEnemy)
                        {
                            Destroy(Instantiate(laserParticles, hit.point, hit.transform.rotation), 2);
                            if (bossTakesDamage)
                            {
                                bossTakesDamage = false;
                                StartCoroutine(StartBossDamageCoolDown());
                                enemy.TakeDamage(bossCuttingDamage, this);
                            }
                        }
                        else
                        {
                            enemy.InstantKill(this);
                            enemy.gameObject.AddComponent<CutUpMesh>();
                            SoundManager.SoundManagerInstance.Play(lineSystem.CuttingSound, Vector2.zero, lineSystem.Volume, 1.0f, false, AudioGroup.Effects);
                            Destroy(Instantiate(laserParticles, hit.point, hit.transform.rotation), 2);
                            new Event(Event.TYPE.kill).addCharacter("LineSystem").addEnemy(enemy.name).send();
                        }
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

    private void PrepareLightSabre()
    {
        if (lightSabreGameObject == null)
        {
            lightSabreGameObject = Instantiate(lightSabrePrefab, Vector3.zero, Quaternion.identity) as GameObject;
            lightSabreGameObject.transform.parent = lineSystem.Players[0].transform;
            lightSabreGameObject.transform.position = Vector3.zero;
            LightSabreScript lightSabreScript = lightSabreGameObject.GetComponent<LightSabreScript>();
            lightSabreScript.PowerUpDuration = timeActive;
            lightSabreScript.CuttingSound = lineSystem.CuttingSound;
            lightSabreScript.Volume = lineSystem.Volume;
        }
    }
}
