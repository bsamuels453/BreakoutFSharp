namespace global

module Draw =
    open SFML.Window;
    open SFML.Graphics;
    open SFML.Audio;

    let mutable private drawablesToUpdate:ObjectId list = []
    let mutable private drawablesToAdd:((string*Texture) list -> SpriteState) list = []
    let mutable private drawablesToRemove:ObjectId list = []

    let QueueSpriteUpdate objectId =
        drawablesToUpdate <- List.Cons (objectId, drawablesToUpdate)

    let QueueSpriteAddition spriteInitializer =
        drawablesToAdd <- List.Cons (spriteInitializer, drawablesToAdd)

    let QueueSpriteDeletion objectId =
        drawablesToRemove <- List.Cons (objectId, drawablesToRemove)

    let private addSprites drawableSprites spritesToAdd textures =
        let generatedSprites = spritesToAdd |> List.map (fun f -> f textures)
        generatedSprites
            |> List.append drawableSprites
            |> List.sortBy (fun elem -> elem.ZLayer)

    let UpdateRenderState renderState gameState textures =
        let drawableSprites = 
            match drawablesToAdd.Length with
            | 0 -> renderState.Sprites
            | _ -> addSprites renderState.Sprites drawablesToAdd textures
        drawablesToAdd <- []

        let filteredSprites =  drawableSprites |> List.filter (fun elem -> not (List.exists (elem.Id.Equals) drawablesToRemove ))
        drawablesToRemove <- []

        let newRenderState = {Sprites=filteredSprites}

        let rec applyUpdates sprites updates =
            match updates with
            | [] -> ()
            | h::t ->
                let idx = sprites |> Array.findIndex (fun s -> s.Id.Equals h)
                let sprite = sprites.[idx]
                sprites.[idx] <- sprite.Update newRenderState gameState sprite
                applyUpdates sprites t
                
        let autoUpdateSprites = 
            filteredSprites 
            |> List.filter (fun sp -> sp.AutoUpdate)
            |> List.map (fun sp -> sp.Id)

        drawablesToUpdate <- List.append drawablesToUpdate autoUpdateSprites

        let newSprites = Array.ofList filteredSprites
        applyUpdates newSprites drawablesToUpdate
        drawablesToUpdate <- []

        {renderState with Sprites = List.ofArray newSprites}

    let Draw (win:RenderWindow) renderState =
        win.Clear (new Color(43uy, 43uy, 90uy, 255uy))

        renderState.Sprites |> List.map (fun s -> s.Sprite.Draw(win,RenderStates.Default)) |> ignore

        win.Display()
        ()