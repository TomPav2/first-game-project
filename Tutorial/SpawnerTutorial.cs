using UnityEngine;
using static GameValues;

public class SpawnerTutorial : SpawnerController
{
    private TutorialController tutorial;
    public override bool engage(int avgTime, float avgInterval, int health)
    {
        if (GetComponent<SpriteRenderer>().enabled) return false;

        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Animator>().enabled = true;
        emission.rateOverTimeMultiplier = 0;
        particleSpeed = 5;
        GetComponent<Animator>().SetTrigger(Trigger.ANIMATION_START);

        return true;
    }

    public void enableCollision(TutorialController tutorial)
    {
        this.tutorial = tutorial;
        GetComponent<CapsuleCollider2D>().enabled = true;
    }

    protected override void overload()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
        tutorial.nextStep();
    }

    // for visibility
    public void tutorialDisengage()
    {
        base.disengage();
    }
}
