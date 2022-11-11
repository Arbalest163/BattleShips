using UnityEngine;

[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory
{
    [SerializeField] private GameTileContent _singleDeckShipPrefab;
    [SerializeField] private GameTileContent _twoDeckShipPrefab;
    [SerializeField] private GameTileContent _threeDeckShipPrefab;
    [SerializeField] private GameTileContent _fourDeckShipPrefab;
    [SerializeField] private GameTileContent _emptyPrefab;
    [SerializeField] private Explosion _explosionMissing;
    [SerializeField] private Explosion _explosionShip;

    public void Reclaim(GameTileContent content)
    {
        Destroy(content.gameObject);
    }

    public GameTileContent Get(GameTileContentType type)
    {
        return type switch
        {
            GameTileContentType.SingleDeckShip => Get(_singleDeckShipPrefab),
            GameTileContentType.TwoDeckShip => Get(_twoDeckShipPrefab),
            GameTileContentType.ThreeDeckShip => Get(_threeDeckShipPrefab),
            GameTileContentType.FourDeckShip => Get(_fourDeckShipPrefab),
            GameTileContentType.ExplosionMissing => Get(_explosionMissing),
            GameTileContentType.ExplosionShip => Get(_explosionShip),
            _ => Get(_emptyPrefab)
        };
    }

    private T Get<T>(T prefab) where T : GameTileContent
    {
        T instance = CreateGameObjectInstance(prefab);
        instance.Initialize(this);
        return instance;
    }
}
