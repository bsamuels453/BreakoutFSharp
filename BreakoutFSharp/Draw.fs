﻿namespace global

module Draw =
    open SFML.Window;
    open SFML.Graphics;
    open SFML.Audio;

    let mutable private drawablesToUpdate:ObjectId list = []
    let mutable private drawablesToAdd:((string*Texture) list -> SpriteState) list = []
    let mutable private drawablesToRemove:ObjectId list = []

    let queueSpriteUpdate objectId =
        drawablesToUpdate <- List.Cons (objectId, drawablesToUpdate)

    let queueSpriteAddition spriteInitializer =
        drawablesToAdd <- List.Cons (spriteInitializer, drawablesToAdd)

    let queueSpriteDeletion objectId =
        drawablesToRemove <- List.Cons (objectId, drawablesToRemove)

    let private addSprites drawableSprites spritesToAdd textures =
        let generatedSprites = spritesToAdd |> List.map (fun f -> f textures)
        generatedSprites
            |> List.append drawableSprites
            |> List.sortBy (fun elem -> elem.ZLayer)

    let private removeSprites drawableSprites idsToRemove =
        let spritesToRemove =  drawableSprites |> List.filter (fun elem -> (List.exists (elem.Id.Equals) idsToRemove ))
        spritesToRemove |> List.map (fun s -> s.Sprite.Dispose()) |> ignore
         
        drawableSprites |> List.filter (fun elem -> not (List.exists (elem.Id.Equals) idsToRemove ))

    let private updateSprites tempRenderState gameState sprites idsToUpdate=
        let rec applyUpdates sprites updates =
            match updates with
            | [] -> ()
            | h::t ->
                let idx = sprites |> Array.findIndex (fun s -> s.Id.Equals h)
                let sprite = sprites.[idx]
                sprites.[idx] <- sprite.Update tempRenderState gameState sprite
                applyUpdates sprites t
                
        let autoUpdateSprites = 
            sprites 
            |> List.filter (fun sp -> sp.AutoUpdate)
            |> List.map (fun sp -> sp.Id)

        let fullIdsToUpdate = List.append idsToUpdate autoUpdateSprites

        let spriteArr = Array.ofList sprites
        applyUpdates spriteArr fullIdsToUpdate
        spriteArr

    let updateRenderState renderState gameState textures =
        let drawableSprites = 
            match drawablesToAdd.Length with
            | 0 -> renderState.Sprites
            | _ -> addSprites renderState.Sprites drawablesToAdd textures
        drawablesToAdd <- []

        let filteredSprites = removeSprites drawableSprites drawablesToRemove
        drawablesToRemove <- []

        let tempRenderState = {renderState with Sprites=filteredSprites}

        let updatedSprites = updateSprites tempRenderState gameState filteredSprites drawablesToUpdate
        drawablesToUpdate <- []

        {renderState with Sprites = List.ofArray updatedSprites}

    let draw (win:RenderWindow) renderState =
        win.Clear (new Color(43uy, 43uy, 90uy, 255uy))
        win.SetView renderState.View

        renderState.Sprites |> List.map (fun s -> s.Sprite.Draw(win,RenderStates.Default)) |> ignore

        win.Display()
        ()