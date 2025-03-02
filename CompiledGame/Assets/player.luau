local go: GameObject = GameObject.Get("Camera")
local player: GameObject = GameObject.Get("Player")
local controller: CharacterController = CharacterController.Get(player)
local song: AudioSource = AudioSource.Get(GameObject.Get("Song"))

local previousMousePosition: Vec2 = Input.GetMousePosition()

local sensitivity: number = 0.1
local mouseRotation: Vec2 = Vec2.New(115.11, -44.169)
local originalPos: Vec3 = player.position
local pained: boolean = false

Physics.timeStep = 2.55
Input.LockCursor(true)

function getAxis(left: string, right: string): number
    local leftKey: boolean = Input.GetKey(left)
    local rightKey: boolean = Input.GetKey(right)

    if leftKey and rightKey then
        return 0
    elseif leftKey then
        return -1
    elseif rightKey then
        return 1
    end

    return 0
end

function _OnFixedUpdate(dt: number)
    local newMousePosition: Vec2 = Input.GetMousePosition()
    mouseRotation += (newMousePosition - previousMousePosition) * sensitivity
    mouseRotation.y = math.clamp(mouseRotation.y, -89, 89)
    go.localRotation = Quat.Euler(mouseRotation.y, -mouseRotation.x, 0)
    previousMousePosition = newMousePosition
    
    local horizontal: number = getAxis("D", "A")
    local vertical: number = getAxis("S", "W")
    
    local moveDir: Vec3 = (Vec3.New(horizontal, 0, vertical)):Normalize() * Quat.Euler(0, go.rotation:ToEuler().y, 0)
    controller:Move(moveDir, dt)
    
    if controller.IsGrounded and Input.GetKey("Space") then
        controller:Jump()
    end

    if Input.GetKey("P") then
        if not pained then
            pained = true
            controller:Warp(originalPos)
            mouseRotation = Vec2.New(115.11, -44.169)
            song:Replay()
        end
    else
        pained = false
    end
end