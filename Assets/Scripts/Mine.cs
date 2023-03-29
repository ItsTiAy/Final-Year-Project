using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using static LevelManager;

public class Mine : SecondaryItem
{
    [SerializeField]
    private float duration;
    [SerializeField]
    private float radius;
    [SerializeField]
    private ParticleSystem explosion;

    private void Start()
    {
        StartCoroutine(LayMine());
    }

    public IEnumerator LayMine()
    {
        yield return new WaitForSeconds(duration);
        Explode();
    }

    public void Explode()
    {
        LevelData levelData = instance.GetLevelData();

        for (int i = 0; i < levelData.tiles[2].tilePositionsX.Count; i++)
        {
            if (Vector2.Distance(transform.position, new Vector2(levelData.tiles[2].tilePositionsX[i], levelData.tiles[2].tilePositionsY[i])) <= radius)
            {
                instance.tilemaps[2].SetTile(new Vector3Int(levelData.tiles[2].tilePositionsX[i], levelData.tiles[2].tilePositionsY[i]), null);
                Pathfinding.nodes[new Vector2Int(levelData.tiles[2].tilePositionsX[i], levelData.tiles[2].tilePositionsY[i])].SetIsWalkable(true);
            }
        }

        if (instance.tilemaps[2].GetComponent<TilemapCollider2D>().hasTilemapChanges)
        {
            // Need to update tilemap collider immediately so the bullets path can be recalculated
            instance.tilemaps[2].GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
        }

        foreach (Transform bullet in GameController.instance.bulletContainer)
        {
            bullet.GetComponent<Bullet>().CalculateTrajectory();
        }

        List<Enemy> enemiesToDestroy = new List<Enemy>();

        foreach (Enemy enemy in GameController.instance.enemies)
        {
            if (Vector2.Distance(transform.position, enemy.transform.position) <= radius)
            {
                enemiesToDestroy.Add(enemy);
            }
        }

        foreach (Enemy enemy in enemiesToDestroy)
        {
            enemy.DecreaseHealth();
        }

        List<Player> playersToDestroy = new List<Player>();

        foreach (Player player in GameController.instance.players)
        {
            if (Vector2.Distance(transform.position, player.transform.position) <= radius)
            {
                playersToDestroy.Add(player);
            }
        }

        foreach (Player player in playersToDestroy)
        {
            player.DecreaseHealth();
        }

        AudioManager.instance.Play("MineExplosion");
        explosion.Play();
        transform.DetachChildren();
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            collision.gameObject.GetComponent<Bullet>().DestroyBullet();
            Explode();
        }
    }

    public float GetRadius()
    {
        return radius;
    }

    public new int GetIndex()
    {
        return 0;
    }
}