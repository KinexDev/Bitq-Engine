using System.Numerics;
using Bitq.Rendering;
using BulletSharp;
using BulletSharp.Math;
using Hexa.NET.ImGui;
using Newtonsoft.Json;
using Vector3 = System.Numerics.Vector3;

namespace Bitq.Core;
public class CharacterController : Component
{
    [JsonIgnore]
    private GhostPairCallback _ghostPairCallback;
    [JsonIgnore]
    public PairCachingGhostObject GhostObject { get; private set; }
    [JsonIgnore]
    public ClosestConvexResultCallback ConvexResultCallback { get; }
    [JsonIgnore]
    public CapsuleShape CameraSphere { get; }
    [JsonIgnore]
    public KinematicCharacterController Character { get; private set; }
    public float stepHeight = 0.35f;
    public float walkSpeed = 5f;
    public float jumpSpeed = 10f;
    public float gravity = 9.81f;
    public float capsuleHeight = 2f;
    public float capsuleRadius = 0.5f;

    
    [JsonIgnore]
    public bool IsGrounded {
        get
        {
            return Character.OnGround;
        }
    }
    public override void OnLoad()
    {
        CreateCharacter();
    }

    private void CreateCharacter()
    {
        _ghostPairCallback = new GhostPairCallback();
        Physics.broadphase.OverlappingPairCache.SetInternalGhostPairCallback(_ghostPairCallback);
        
        var capsule = new CapsuleShape(capsuleRadius, capsuleHeight);
        GhostObject = new PairCachingGhostObject()
        {
            CollisionShape = capsule,
            WorldTransform = Matrix.RotationQuaternion(Transform.Rotation.ToBtQuat()) * Matrix.Translation(Transform.Position.ToBtVec3())
        };
        GhostObject.CollisionFlags |= CollisionFlags.CharacterObject;
        Physics.dynamicsWorld.AddCollisionObject(GhostObject, CollisionFilterGroups.CharacterFilter, CollisionFilterGroups.StaticFilter | CollisionFilterGroups.DefaultFilter);
        Character = new KinematicCharacterController(GhostObject, capsule, stepHeight);
        Character.SetJumpSpeed(jumpSpeed);
        Physics.dynamicsWorld.AddAction(Character);
    }

    public void Move(Vector3 direction, float deltaTime)
    {
        Vector3 displacement = direction * walkSpeed * deltaTime;
        Character.SetWalkDirection(displacement.ToBtVec3());
    }

    public void Warp(Vector3 worldPosition)
    {
        var pos = worldPosition.ToBtVec3();
        Character.Warp(ref pos);
    }

    public void Jump()
    {
        Character.Jump();
    }
    
    public override void OnInspectorGUI(float deltaTime)
    {
        ImGui.DragFloat("Step Height", ref stepHeight, 0.01f);
        ImGui.DragFloat("Walk Speed", ref walkSpeed, 0.1f);
        ImGui.DragFloat("Jump Speed", ref jumpSpeed, 0.1f);
        ImGui.DragFloat("Gravity", ref gravity, 0.1f);
        ImGui.DragFloat("Capsule Height", ref capsuleHeight, 0.1f);
        ImGui.DragFloat("Capsule Radius", ref capsuleRadius, 0.1f);
    }

    public override void OnLateFixedUpdate(double deltaTime)
    {
        GhostObject.WorldTransform.Decompose(out _, out var rot, out var translation);
        Transform.Position = translation.ToNumVec3();
        Transform.Rotation = rot.ToNumQuat();
    }

    public override void OnDispose()
    {
        if (Character != null)
        {
            Physics.dynamicsWorld.RemoveAction(Character);
            Physics.dynamicsWorld.RemoveCollisionObject(GhostObject);
            CameraSphere.Dispose();
            ConvexResultCallback.Dispose();
            _ghostPairCallback.Dispose();
            Character = null;
        }
    }
}
