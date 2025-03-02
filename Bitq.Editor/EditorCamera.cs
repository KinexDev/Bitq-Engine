using System.Numerics;
using Bitq.Core;
using Bitq.Rendering;
using Silk.NET.Input;
using Camera = Bitq.Core.Camera;

namespace Bitq.Editor;

public class EditorCamera : Camera
{
    public Vector3 _cameraPosition = new Vector3(0.0f, 0.0f, 5.0f);
    public Vector3 _cameraFront = new Vector3(0.0f, 0.0f, -1.0f);
    private Vector3 _cameraUp = Vector3.UnitY;
    public Vector3 _cameraDirection = Vector3.Zero;
    private float _cameraYaw = -90f;
    private float _cameraPitch;
    public override Matrix4x4 View => Matrix4x4.CreateLookAt(_cameraPosition, _cameraPosition + _cameraFront, _cameraUp);
    public override Matrix4x4  Projection => Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), (float)Engine.Window.FramebufferSize.X / Engine.Window.FramebufferSize.Y, Near, Far);

    private Vector2 _lastMousePosition;
    
    public void OnUpdate(double deltaTime)
    {
        if (Engine.Keyboard == null || Engine.Mouse == null)
            return;

        var moveSpeed = 9 * (float)deltaTime;
        
        var sensitivity = 0.1f;
        
        if (Engine.Keyboard.IsKeyPressed(Key.ShiftLeft))
            moveSpeed *= 2;
        
        if (Engine.Mouse.IsButtonPressed(MouseButton.Right))
        {
            Engine.Mouse.Cursor.CursorMode = CursorMode.Raw;
            
            if (Engine.Keyboard.IsKeyPressed(Key.W))
                _cameraPosition += moveSpeed * _cameraFront;
            if (Engine.Keyboard.IsKeyPressed(Key.S))
                _cameraPosition -= moveSpeed * _cameraFront;
        
            if (Engine.Keyboard.IsKeyPressed(Key.E))
                _cameraPosition += moveSpeed * _cameraUp;
            if (Engine.Keyboard.IsKeyPressed(Key.Q))
                _cameraPosition -= moveSpeed * _cameraUp;
            if (Engine.Keyboard.IsKeyPressed(Key.A))
                _cameraPosition -= Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * moveSpeed;
            if (Engine.Keyboard.IsKeyPressed(Key.D))
                _cameraPosition += Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * moveSpeed;

            var mouseDelta = (Engine.Mouse.Position - _lastMousePosition) * sensitivity;
            _cameraYaw += mouseDelta.X;
            _cameraPitch -= mouseDelta.Y;
            
            _cameraPitch = Math.Clamp(_cameraPitch, -89.0f, 89.0f);

            _cameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(_cameraYaw)) * MathF.Cos(MathHelper.DegreesToRadians(_cameraPitch));
            _cameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(_cameraPitch));
            _cameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(_cameraYaw)) * MathF.Cos(MathHelper.DegreesToRadians(_cameraPitch));
            _cameraFront = Vector3.Normalize(_cameraDirection);
        }
        else
            Engine.Mouse.Cursor.CursorMode = CursorMode.Normal;
        
        _lastMousePosition = Engine.Mouse.Position;
    }
}