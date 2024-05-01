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

    [SerializeField] private TextHudController textHud;

    // camera work
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private GameObject mainChar;
    [SerializeField] private GameObject cameraTarget;

    private static readonly Vector3 CAMERA_OFFSET = new Vector3(0, 7, 0);

    public void begin()
    {
        enemiesToKill = 3;
        StartCoroutine(introRoutine());
    }

    private IEnumerator introRoutine()
    {
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
        textHud.popUp("[name] the Green", "Heals / Curses", null, 0.2f);
        enemy2.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        enemy2.showOffEffect();
        yield return new WaitForSeconds(1);

        // second enemy
        cameraTarget.transform.position = enemy3.transform.position + CAMERA_OFFSET;
        textHud.popUp("[name] the Colourblind", "Summoner", null, 0.2f);
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
