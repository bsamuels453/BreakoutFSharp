namespace global

[<AutoOpen>]
module GameFunctions =
    let GenerateSpriteId = 
        let count = ref 0;
        (fun () -> 
            count := !count + 1
            ObjectId(!count)
            )