using System.Numerics;
using BulletSharp;

namespace Bitq.Core;

public static class Physics
{
    public static float timeStep = 1;
    public static DiscreteDynamicsWorld dynamicsWorld;
    public static BroadphaseInterface broadphase;
    public static CollisionConfiguration collisionConfiguration;
    public static CollisionDispatcher dispatcher;
    public static SequentialImpulseConstraintSolver solver;
    
    public static void Initialize()
    {
        collisionConfiguration = new DefaultCollisionConfiguration();
        dispatcher = new CollisionDispatcher(collisionConfiguration);

        broadphase = new DbvtBroadphase();
        solver = new SequentialImpulseConstraintSolver();
        dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfiguration);
    }
    
    public static void Simulate(float deltaTime)
    {
        SceneManager.OnFixedUpdate(deltaTime);
        dynamicsWorld.StepSimulation(deltaTime * timeStep, 10);
        SceneManager.OnLateFixedUpdate(deltaTime);
    }
}