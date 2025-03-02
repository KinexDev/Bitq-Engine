using System.Diagnostics;
using System.Numerics;
using Bitq.Rendering;
using BulletSharp;
using BulletSharp.Math;
using Hexa.NET.ImGui;
using Newtonsoft.Json;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Bitq.Core;

public class Rigidbody : Component
{
    public float mass = 1;
    public float LinearDamping;
    public float AngularDamping;
    public float Resitution;
    public float Friction = 0.5f;
    [JsonIgnore] public RigidBody rb;

    [JsonIgnore]
    public Vector3 linearVelocity
    {
        get
        {
            return rb.LinearVelocity.ToNumVec3();
        }
        set
        {
            rb.LinearVelocity = value.ToBtVec3();
        }
    }

    [JsonIgnore]
    public Vector3 angularVelocity
    {
        get
        {
            return rb.AngularVelocity.ToNumVec3();
        }
        set
        {
            rb.AngularVelocity = value.ToBtVec3();
        }
    }

    [JsonIgnore]
    public Vector3 position
    {
        get
        {
            return rb.WorldTransform.Origin.ToNumVec3();
        }
        set
        {
            var transform = rb.WorldTransform;
            transform.Origin = value.ToBtVec3();
            rb.WorldTransform = transform;
            rb.Activate();
        }
    }
    
    [JsonIgnore]
    public Quaternion rotation
    {
        get
        {
            rb.WorldTransform.Decompose(out _, out var rot, out _);
            return rot.ToNumQuat();
        }
        set
        {
            rb.WorldTransform.Decompose(out var scale, out var rot, out var translation);
            var transform = Matrix.Scaling(scale) * Matrix.RotationQuaternion(value.ToBtQuat()) *
                            Matrix.Translation(translation);
            rb.WorldTransform = transform;
            rb.Activate();
        }
    }
    
    public override void OnLoad()
    {
        if (GameObject.GetComponent<Collider>() == null)
            return;
        var col = GameObject.GetComponent<Collider>();
        if (col.shape == null)
            col.OnLoad();

        Matrix startMatrix =
            Matrix.RotationQuaternion(Transform.Rotation.ToBtQuat()) * Matrix.Translation(Transform.Position.ToBtVec3());

        DefaultMotionState motionState = new DefaultMotionState(startMatrix);

        BulletSharp.Math.Vector3 inertia = BulletSharp.Math.Vector3.Zero;
        
        if (mass > 0)
            col.shape.CalculateLocalInertia(mass, out inertia);

        RigidBodyConstructionInfo rigidBodyConstructionInfo = new RigidBodyConstructionInfo(mass, motionState, col.shape, inertia);
        rigidBodyConstructionInfo.LinearDamping = LinearDamping;
        rigidBodyConstructionInfo.AngularDamping = AngularDamping;
        rigidBodyConstructionInfo.Restitution = Resitution;
        rigidBodyConstructionInfo.Friction = Friction;
        
        rb = new RigidBody(rigidBodyConstructionInfo);
    
        Physics.dynamicsWorld.AddRigidBody(rb);
    }

    
    public override void OnInspectorGUI(float deltaTime)
    {
        ImGui.Text("Body Settings");
        ImGui.DragFloat("Mass", ref mass, 0.1f);
        ImGui.DragFloat("Resitution", ref Resitution, 0.1f);
        ImGui.DragFloat("Friction", ref Friction, 0.1f);
        ImGui.Text("Damping");
        ImGui.DragFloat("Linear Damping", ref LinearDamping, 0.1f);
        ImGui.DragFloat("Angular Damping", ref AngularDamping, 0.1f);
    }

    public override void OnActiveChanged(bool active)
    {
        if (rb != null)
        {
            if (active)
            {
                rb.ActivationState = ActivationState.ActiveTag;
            }
            else
            {
                rb.ActivationState = ActivationState.DisableDeactivation;
            }
        }
    }

    public override void OnLateFixedUpdate(double deltaTime)
    {
        rb.WorldTransform.Decompose(out _, out var rot, out var translation);
        Transform.Position = translation.ToNumVec3();
        Transform.Rotation = rot.ToNumQuat();
    }

    public override void OnDispose()
    {
        if (rb != null)
        {
            Physics.dynamicsWorld.RemoveRigidBody(rb);
            rb.Dispose();
        }
    }
    
}