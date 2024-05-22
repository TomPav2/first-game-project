using Cinemachine;
using System.Collections;
using UnityEngine;
using static ScenePersistence;

public class Fight3Controller : FightController
{
    [SerializeField] private BattleArenaController arena;
    [SerializeField] private BossEnemyController enemy1;
    [SerializeField] private BossEnemyController enemy2;
    [SerializeField] private BossEnemyController enemy3;
    [SerializeField] private SpellCurse curse;

    // camera work
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private GameObject mainChar;
    [SerializeField] private GameObject cameraTarget;

    private static readonly Vector3 CAMERA_OFFSET = new Vector3(0, 7, 0);

    public override void begin()
    {
        enemiesToKill = 3;
        StartCoroutine(introRoutine());
    }

    public override void registerTakedown()
    {
        base.registerTakedown();
        if (enemiesToKill == 1 && enemy1 != null) enemy1.unlockAltSpell();
    }

    protected override IEnumerator introRoutine()
    {
        textHud.popUp("Excellent!", null, null);
        yield return new WaitForSeconds(2);
        textHud.popUp("Take a deep breath...", "Before the final challenge...", null);
        yield return new WaitForSeconds(3);

        lockControls = true;
        textHud.popUp("The Three Mages", null, null);
        
        // first enemy
        yield return new WaitForSeconds(2);
        setCamToManual();
        textHud.popUp("Angar the Red", "Pyromaniac", null, 0.2f);
        enemy1.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        enemy1.showOffEffect();
        yield return new WaitForSeconds(1);

        // second enemy
        cameraTarget.transform.position = enemy2.transform.position + CAMERA_OFFSET;
        textHud.popUp("Korreg the Green", "Heals / Curses", null, 0.2f);
        enemy2.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        curse.startCurse();
        yield return new WaitForSeconds(1);
        enemy2.showOffEffect();
        yield return new WaitForSeconds(1);

        // third enemy
        cameraTarget.transform.position = enemy3.transform.position + CAMERA_OFFSET;
        textHud.popUp("Zhon the Colourblind", "Summoner", null, 0.2f);
        enemy3.gameObject.SetActive(true);
        yield return new WaitForSeconds(5);

        // end
        setCamToAuto();
        lockControls = false;
        yield return new WaitForSeconds(0.5f);
        enemy1.beginFight();
        enemy2.beginFight();
        enemy3.beginFight();
        yield break;
    }

    private void setCamToManual()
    {
        virtualCamera.Follow = cameraTarget.transform;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 1.2f;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 1.2f;
    }

    private void setCamToAuto()
    {
        virtualCamera.Follow = mainChar.transform;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 0.6f;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 0.6f;
    }
}
