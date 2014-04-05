namespace global

[<AutoOpen>]
module GameTypes =
    open SFML.Window
    open SFML.Graphics

    type Vec2 = {
        X : float32
        Y : float32
        }

    type ObjectId = int
    
    type ObjectId = ObjectId of int

    type BallState = {
        BallId : ObjectId
        Position : Vec2
        Velocity : Vec2
        NumBounces : int
        }
    
    type PaddleState = {
        PaddleId : ObjectId
        Position : Vec2
        }

    type KeyState =
        | Pressed
        | Released

    type KeyStateChange = {
        KeyState : KeyState
        Key : Keyboard.Key
    }

    type BlockState = {
        BlockId : ObjectId
        Position:Vec2   
    }

    type GameState = {
        BallState:BallState
        PaddleState:PaddleState
        ActiveBlocks:BlockState list
    }

    type RenderState = {
        Sprites : SpriteState list
    } 

    and SpriteState = {
        Id : ObjectId
        ZLayer : float
        AutoUpdate : bool
        Sprite : Shape
        Update : (RenderState -> GameState -> SpriteState -> SpriteState)
    }


    type ListBuilder() =
        member this.Bind(m, f) = 
            m |> List.collect f

        member this.Zero() = 
            []
        
        member this.Yield(x) = 
            [x]

        member this.YieldFrom(m) = 
            m

        member this.For(m,f:'c->'d list) =
            this.Bind(m,f)
        
        member this.Combine (a,b) = 
            List.concat [a;b]

        member this.Delay(f) = 
            f()

    type ArrayBuilder() =
        member this.Bind(m, f) = 
            m |> Array.collect f

        member this.Zero() = 
            []
        
        member this.Yield(x) = 
            [|x|]

        member this.YieldFrom(m) = 
            m

        member this.For(m,f:'c->'d array) =
            this.Bind(m,f)
        
        member this.Combine (a,b) = 
            Array.concat [a;b]

        member this.Delay(f) = 
            f()