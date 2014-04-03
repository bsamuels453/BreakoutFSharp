namespace global

[<AutoOpen>]
module GameFunctions =
    let generateSpriteId = 
        let count = ref 0;
        (fun () -> 
            count := !count + 1
            !count
            )