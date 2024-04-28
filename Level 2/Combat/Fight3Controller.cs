using Cinemachine;
using System.Collections;
using UnityEngine;
using static SceneLoader;

public class Fight3Controller : MonoBehaviour
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

    public void begin()
    {
        StartCoroutine(introRoutine());
    }

    private IEnumerator introRoutine()
    {
        lockControls = true;
        textHud.popUp("The Three Mages", null, null);
        
        // first enemy
        yield return new WaitForSeconds(2);
        setCamToManual();
        textHud.popUp("[name] the Red", "Pyromaniac", null);
        enemy1.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        enemy1.showOffEffect();
        yield return new WaitForSeconds(1);


        // end
        setCamToAuto();
        yield return new WaitForSeconds(1);
        lockControls = false;
        yield return new WaitForSeconds(0.5f);
        enemy1.beginFight();
        //enemy2.beginFight();
        //enemy3.beginFight();
        yield break;
    }

    private void setCamToManual()
    {
        virtualCamera.Follow = cameraTarget.transform;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 1;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 1;
    }

    private void setCamToAuto()
    {
        virtualCamera.Follow = mainChar.transform;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 0.6f;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 0.6f;
    }
}
