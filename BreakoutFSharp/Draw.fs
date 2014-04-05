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

    let UpdateRenderState renderState gameState textures =
        let mutable drawableSprites = renderState.Sprites |> Array.ofList

        if drawablesToAdd.Length <> 0 then
            let generatedSprites = drawablesToAdd |> List.map (fun f -> f textures)
            drawableSprites <- Array.append drawableSprites (Array.ofList generatedSprites)
            drawableSprites <- Array.sortBy (fun elem -> elem.ZLayer) drawableSprites
            drawablesToAdd <- []

        drawableSprites <- drawableSprites |> Array.filter (fun elem -> not( drawablesToRemove |> List.exists elem.Id.Equals))
        drawablesToRemove <- []

        let newRenderState = {Sprites=List.ofArray drawableSprites}

        let rec applyUpdates sprites updates =
            match updates with
            | [] -> ()
            | h::t ->
                let idx = sprites |> Array.findIndex (fun s -> s.Id.Equals h)
                let sprite = sprites.[idx]
                sprites.[idx] <- sprite.Update newRenderState gameState sprite
                applyUpdates sprites t

        let autoUpdateSprites = 
            drawableSprites 
            |> Array.filter (fun sp -> sp.AutoUpdate)
            |> Array.map (fun sp -> sp.Id)
            |> List.ofArray

        drawablesToUpdate <- List.append drawablesToUpdate autoUpdateSprites

        applyUpdates drawableSprites drawablesToUpdate
        drawablesToUpdate <- []

        {renderState with Sprites = List.ofArray drawableSprites}

    let Draw (win:RenderWindow) renderState =
        win.Clear Color.Black

        renderState.Sprites |> List.map (fun s -> s.Sprite.Draw(win,RenderStates.Default)) |> ignore

        win.Display()
        ()