using System.Collections.Generic;
using UnityEngine;

public abstract class ShooterController : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;

    private readonly List<ProjectileSimpleController> freeProjectiles = new List<ProjectileSimpleController>();

    public void reuseProjectile(ProjectileSimpleController p)
    {
        freeProjectiles.Add(p);
    }

    protected ProjectileSimpleController getProjectile()
    {
        if (freeProjectiles.Count > 0)
        {
            ProjectileSimpleController p = freeProjectiles[freeProjectiles.Count - 1];
            freeProjectiles.RemoveAt(freeProjectiles.Count - 1);
            return p;
        }
        else
        {
            GameObject newProjectile = Instantiate(projectilePrefab);
            ProjectileSimpleController newController = newProjectile.GetComponent<ProjectileSimpleController>();
            newController.holder = this;
            return newController;
        }
    }
}
