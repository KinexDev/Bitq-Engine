local hasWon: boolean = false
local player: GameObject = GameObject.Get("Player")
local bike: GameObject = GameObject.Get("Bike")
local wonSound: AudioSource = AudioSource.Get(GameObject.Get("YouWon"))

function _OnUpdate()
    if Input.GetKey("P") then
        hasWon = false    
    end

    if Vec3.Dist(player.position, bike.position) < 3 and not hasWon then
        wonSound:Play()
        hasWon = true
    end
end