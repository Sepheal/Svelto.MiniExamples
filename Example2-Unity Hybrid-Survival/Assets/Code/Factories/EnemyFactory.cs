using System.Collections;
using System.Collections.Generic;
using Svelto.ECS.Example.Survive.Enemies;
using Svelto.ECS.Example.Survive.HUD;
using Svelto.ECS.Example.Survive.ResourceManager;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    public class EnemyFactory
    {
        public EnemyFactory(GameObjectFactory gameObjectFactory, IEntityFactory entityFactory)
        {
            _gameobjectFactory = gameObjectFactory;
            _entityFactory     = entityFactory;
        }

        public IEnumerator Build(EnemySpawnData enemySpawnData, EnemyAttackComponent EnemyAttackComponent)
        {
            var build = _gameobjectFactory.Build(enemySpawnData.enemyPrefab, false);

            while (build.MoveNext())
                yield return null;

            GameObject enemyGO = build.Current;

            //implementors are ONLY necessary if you need to wrap objects around entity view structs. In the case
            //of Unity, they are needed to wrap Monobehaviours and not used in any other case

            List<IImplementor> implementors = new List<IImplementor>();
            enemyGO.GetComponentsInChildren(implementors);
            var egidHolderImplementor = enemyGO.AddComponent<EntityReferenceHolderImplementor>();
            implementors.Add(egidHolderImplementor);

            //using the GameObject GetInstanceID() as entityID would help to directly use the result of Unity functions
            //to index the entity in the Svelto database. However I want in this demo how to not rely on it.
            var initializer =
                _entityFactory.BuildEntity<EnemyEntityDescriptor>(new EGID(_enemiesCreated++, AliveEnemies.BuildGroup)
                                                                , implementors);

            egidHolderImplementor.reference = initializer.reference;
            //Initialize the pure EntityStructs. This should be the preferred pattern, there is much less boiler plate
            //too
            initializer.Init(EnemyAttackComponent);
            initializer.Init(new HealthComponent
            {
                currentHealth = 100
            });
            initializer.Init(new ScoreValueComponent
            {
                scoreValue = (int) (enemySpawnData.targetType + 1) * 10
            });
            initializer.Init(new EnemyComponent
            {
                enemyType = enemySpawnData.targetType
            });

            var transform = enemyGO.transform;
            var spawnPoint = enemySpawnData.spawnPoint;

            enemyGO.SetActive(true);
            transform.position = spawnPoint;
        }

        readonly IEntityFactory    _entityFactory;
        readonly GameObjectFactory _gameobjectFactory;
        uint                       _enemiesCreated;
    }
}