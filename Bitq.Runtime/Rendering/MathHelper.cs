using System;
using System.Numerics;
using BulletSharp.Math;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Bitq.Rendering;

public static class MathHelper
{
    public static float DegreesToRadians(float degrees)
    {
        return MathF.PI / 180f * degrees;
    }

    public static Vector3 Forward(this Quaternion quat)
    {
        return Vector3.Transform(Vector3.UnitZ, quat);
    }
    
    public static Vector3 Right(this Quaternion quat)
    {
        return Vector3.Transform(Vector3.UnitX, quat);
    }
    
    public static Vector3 Up(this Quaternion quat)
    {
        return Vector3.Transform(Vector3.UnitY, quat);
    }

    public static BulletSharp.Math.Vector3 ToBtVec3(this Vector3 x)
    {
        return new BulletSharp.Math.Vector3(x.X, x.Y, x.Z);
    }
    
    public static BulletSharp.Math.Quaternion ToBtQuat(this Quaternion x)
    {
        return new BulletSharp.Math.Quaternion(x.X, x.Y, x.Z, x.W);
    }
    
    public static Vector3 ToNumVec3(this BulletSharp.Math.Vector3 x)
    {
        return new Vector3(x.X, x.Y, x.Z);
    }
    
    public static Quaternion ToNumQuat(this BulletSharp.Math.Quaternion x)
    {
        return new Quaternion(x.X, x.Y, x.Z, x.W);
    }
    

    public static Quaternion ToQuaternion(this Vector3 euler)
    {
        float pitch = euler.X * (float)(Math.PI / 180);
        float yaw   = euler.Y * (float)(Math.PI / 180);
        float roll  = euler.Z * (float)(Math.PI / 180);

        float halfPitch = pitch * 0.5f;
        float halfYaw   = yaw   * 0.5f;
        float halfRoll  = roll  * 0.5f;

        float sp = (float)Math.Sin(halfPitch);
        float cp = (float)Math.Cos(halfPitch);
        float sy = (float)Math.Sin(halfYaw);
        float cy = (float)Math.Cos(halfYaw);
        float sr = (float)Math.Sin(halfRoll);
        float cr = (float)Math.Cos(halfRoll);

        return new Quaternion
        {
            W = cr * cp * cy + sr * sp * sy,
            X = sp * cy * cr - cp * sy * sr,
            Y = cp * sy * cr + sp * cy * sr,
            Z = cp * cy * sr - sp * sy * cr
        };
    }


    public static Vector3 ToEulerAngles(this Quaternion q)
    {
        Vector3 euler = new Vector3();

        double sinPitch = 2 * (q.W * q.X - q.Y * q.Z);
        sinPitch = Math.Clamp(sinPitch, -1.0, 1.0);
        euler.X = (float)Math.Asin(sinPitch);

        euler.Y = (float)Math.Atan2(2 * (q.W * q.Y + q.Z * q.X),
            1 - 2 * (q.X * q.X + q.Y * q.Y));

        euler.Z = (float)Math.Atan2(2 * (q.W * q.Z + q.X * q.Y),
            1 - 2 * (q.X * q.X + q.Z * q.Z));

        euler.X *= (float)(180 / Math.PI);
        euler.Y *= (float)(180 / Math.PI);
        euler.Z *= (float)(180 / Math.PI);

        if (euler.Y < 0)
            euler.Y += 360;

        return euler;
    }

}