-- Made by Everest
local Vector2 = {}
local module = {}

-- Those nice functions that help with everything

function Vector2:copy()
    return module:new(self.x, self.y)
end

-- magnitude: distance (always positive) from (0, 0)
function Vector2:magnitude()
    -- `x * x` is faster than `x^2`, at least on a large scale
    return math.sqrt((self.x * self.x) + (self.y * self.y))
end
Vector2.length = Vector2.magnitude

function Vector2:unit()
    local magnitude = self:magnitude()

    if magnitude > 0 then
        return module:new(self.x / magnitude, self.y / magnitude)
    end

    return self
end
Vector2.normalize = Vector2.unit

--- Added some funcitonality: If the second parameter is not nil then
-- it uses that for limiting as a minimum (ex: min -1 max 1)
-- I've kept it so max is first for backwards compatibillity for now
function Vector2:limit(max, min)
    -- Switch the two if they're in the wrong place
    if max and min and min > max then
        max, min = min, max
    end

    if min then
        self.x = math.max(self.x, min)
        self.y = math.max(self.y, min)
    end

    if max then
        self.x = math.min(self.x, max)
        self.y = math.min(self.y, max)
    end

    return self
end

function Vector2:dot(vec2)
    return (self.x * vec2.x) + (self.y * vec2.y)
end

-- Fun fact: The old version's dist forgot to square `a` and `b`
-- so this func and the static version were rewritten all the way
-- at the bottom of this module
function Vector2:dist(vec2)
    -- In this case, `(x - y)^2` is faster than `(x - y) * (x - y)`
    return math.sqrt((self.x - vec2.x)^2 + (self.y - vec2.y)^2)
end
Vector2.distance = Vector2.dist

-- Some functions for previous compatibility's sake

--- !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
--- !!!!!!!!!!!!!!!!!!!! IMPORTANT !!!!!!!!!!!!!!!!!!!!
--- !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

--- I've changed the functionality in that these functions only return a new vector2
--- instead of changing the current one. `1 + 1` should work the same as `self + a`.
--- If you'd instead like these functions to work like they previously did then just comment
--- this secion out and uncomment the secion under it.

--- Added functionality: the following functions and their static versions can take a
-- number in the place of a number
function Vector2:add(vec2)
    if type(vec2) == "number" then
        return module:new(self.x + vec2, self.y + vec2)
    else
        return module:new(self.x + vec2.x, self.y + vec2.y)
    end
end

function Vector2:sub(vec2)
    -- Could be shortened to `return self + -vec2` but let's not
    if type(vec2) == "number" then
        return module:new(self.x - vec2, self.y - vec2)
    else
        return module:new(self.x - vec2.x, self.y - vec2.y)
    end
end

function Vector2:mul(vec2)
    if type(vec2) == "number" then
        return module:new(self.x * vec2, self.y * vec2)
    else
        return module:new(self.x * vec2.x, self.y * vec2.y)
    end
end
Vector2.mult = Vector2.mul

function Vector2:div(vec2)
    if type(vec2) == "number" then
        return module:new(self.x / vec2, self.y / vec2)
    else
        return module:new(self.x / vec2.x, self.y / vec2.y)
    end
end

function Vector2:mod(num)
    return module:new(self.x % num, self.y % num)
end

function Vector2:pow(num)
    return module:new(self.x ^ num, self.y ^ num)
end

function Vector2:equals(vec2)
    return self.x == vec2.x and self.y == vec2.y
end

-- If you're wondering why this is commented out, read the note above before Vector2:add
--[[

function Vector2:add(vec2)
    if type(vec2) == "number" then
        self.x = self.x + vec2
        self.y = self.y + vec2
    else
        self.x = self.x + vec2.x
        self.y = self.y + vec2.y
    end
end

function Vector2:sub(vec2)
    -- Could be shortened to `return self + -vec2` but let's not
    if type(vec2) == "number" then
        self.x = self.x - vec2
        self.y = self.y - vec2
    else
        return module:new(self.x - vec2.x, self.y - vec2.y)
    end
end

function Vector2:mul(vec2)
    if type(vec2) == "number" then
        self.x = self.x * vec2
        self.y = self.y * vec2
    else
        self.x = self.x * vec2.x
        self.y = self.y * vec2.y
    end
end
Vector2.mult = Vector2.mul

function Vector2:div(vec2)
    if type(vec2) == "number" then
        self.x = self.x / vec2
        self.y = self.y / vec2
    else
        self.x = self.x / vec2.x
        self.y = self.y / vec2.y
    end
end

--]]

-- Static versions of the above functions

function module:Add(vec1, vec2)
    -- The `or vec1` part is in case `vec1 + vec2` returns nothing, it'll return vec1
    return vec1 + vec2 or vec1
end

function module:Sub(vec1, vec2)
    return vec1 - vec2 or vec1
end

function module:Mul(vec1, vec2)
    return vec1 * vec2 or vec1
end
module.Mult = module.Mul

function module:Div(vec1, vec2)
    return vec1 / vec2 or vec1
end

function module:Unit(vec2)
    return vec2:unit()
end
module.Normalize = module.unit

function module:Limit(vec2, max, min)
    return vec2:limit(max, min)
end

function module:Dist(vec1, vec2)
    return vec1:dist(vec2)
end
module.Distance = module.Dist

function module:new(x, y)
    return setmetatable(
        {x = x, y = y},
        {
            __index = Vector2;

            -- Useful for debugging, possibly
            __tostring = function(self)
                return "Vector2(" .. self.x .. ", " .. self.y .. ")"
            end;

            -- Length, `#vec2` == `vec2:magnitude()`
            __len = Vector2.magnitude;

            -- Stands for "unary minus" and is called when we do `-vec2`
            __unm = function(self)
                return self * -1
            end;

            -- I don't think I need to explain the math metamethods
            __add = Vector2.add;
            __sub = Vector2.sub;
            __mul = Vector2.mul;
            __div = Vector2.div;
            __mod = Vector2.mod;
            __pow = Vector2.pow;

            -- Comparison checks
            __eq = Vector2.equals;
            -- On less than and less than equals the two arguments are flipped
            -- when using the oposite comparison (`>` and `>=`)
            -- I've also chosen to make these two check distance as that could
            -- be useful in some places and gives me areason to actually
            -- implement the two to do something
            __lt = function(self, vec2)
                -- Remember: `#self` == `self:distance()`
                return #self < #vec2
            end;
            __le = function(self, vec2)
                return #self <= #vec2
            end;
        }
    )
end

return module
