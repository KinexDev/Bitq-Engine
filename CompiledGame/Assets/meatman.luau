local still: GameObject = GameObject.Get("still")
local walk1: GameObject = GameObject.Get("walk1")
local walk2: GameObject = GameObject.Get("walk2")
local player: GameObject = GameObject.Get("Player")
local meatman: GameObject = GameObject.Get("MeatMan")

local originalPosition = meatman.position
local hasPained: boolean = false
local index: number = 0
local dtOverTime: number = 0

function _OnUpdate(dt: number)
    if Input.GetKey("P") then
        if not hasPained then
            meatman.position = originalPosition
            hasPained = true  
        end
    else
        hasPained = false
    end

    if player.position.x - meatman.position.x > -1 and Vec3.Dist(player.position, meatman.position) < 10 then
        quit()
    end

    dtOverTime += dt

    if dtOverTime > 0.05 then
        dtOverTime = 0
        index += 1

        if index == 4 then
            index = 0
        end

        if index == 0 or index == 2 then
            still.active = true
            walk1.active = false
            walk2.active = false
        elseif index == 1 then
            still.active = false
            walk1.active = true
            walk2.active = false
        elseif index == 3 then
            still.active = false
            walk1.active = false
            walk2.active = true
        end
    end

    meatman.position += Vec3.New(-3, 0, 0) * dt
end
