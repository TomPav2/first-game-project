using UnityEngine;

public interface ISpawnerHandler
{
    public abstract void stoppedSpawning(SpawnerController spawner);

    public abstract void fakeSpawnedEnemies(byte amount);

    public abstract SkellyController getSkeleton(bool overloaded);

    public abstract void removeFromLiving(GameObject enemy);
}
